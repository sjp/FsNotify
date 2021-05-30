using System;
using System.IO;
using System.Threading.Channels;

namespace SJP.FsNotify
{
    /// <summary>
    /// Listens to filesystem change notifications and publishes to a channel when a directory, or file in a directory, changes.
    /// </summary>
    public class ChannelFileSystemWatcher : IChannelFileSystemWatcher, IDisposable
    {
        /// <summary>
        /// Initializes an instance of the <see cref="ChannelFileSystemWatcher"/> class.
        /// </summary>
        /// <param name="writer">A writer to a channel that will receive filesystem events.</param>
        /// <param name="options">Configuration parameters for writing filesystem events.</param>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> or <paramref name="options"/> are  <c>null</c>.</exception>
        public ChannelFileSystemWatcher(ChannelWriter<FileSystemEventArgs> writer, IChannelFileSystemWatcherOptions options)
            : this(writer, null, options, new FileSystemWatcherAdapter(new FileSystemWatcher()))
        {
        }

        /// <summary>
        /// Initializes an instance of the <see cref="ChannelFileSystemWatcher"/> class.
        /// </summary>
        /// <param name="writer">A writer to a channel that will receive filesystem events.</param>
        /// <param name="errorWriter">A writer to a channel that will receive error messages produced when filesystem change detection encounters errors.</param>
        /// <param name="options">Configuration parameters for writing filesystem events.</param>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> or <paramref name="errorWriter"/> or <paramref name="options"/> are <c>null</c>.</exception>
        public ChannelFileSystemWatcher(
            ChannelWriter<FileSystemEventArgs> writer,
            ChannelWriter<ErrorEventArgs> errorWriter,
            IChannelFileSystemWatcherOptions options
        ) : this(writer, errorWriter, options, new FileSystemWatcherAdapter(new FileSystemWatcher()))
        {
            if (errorWriter == null)
                throw new ArgumentNullException(nameof(errorWriter));
        }

        /// <summary>
        /// Initializes an instance of the <see cref="ChannelFileSystemWatcher"/> class.
        /// </summary>
        /// <param name="writer">A writer to a channel that will receive filesystem events.</param>
        /// <param name="errorWriter">A writer to a channel that will receive error messages produced when filesystem change detection encounters errors.</param>
        /// <param name="options">Configuration parameters for writing filesystem events.</param>
        /// <param name="watcher">An underlying filesystem watcher instance that will emit events.</param>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/>, <paramref name="options"/> or <paramref name="watcher"/> is <c>null</c>.</exception>
        internal ChannelFileSystemWatcher(
            ChannelWriter<FileSystemEventArgs> writer,
            ChannelWriter<ErrorEventArgs>? errorWriter,
            IChannelFileSystemWatcherOptions options,
            IFileSystemWatcher watcher
        )
        {
            Writer = writer ?? throw new ArgumentNullException(nameof(writer));
            Options = options ?? throw new ArgumentNullException(nameof(options));
            _watcher = watcher ?? throw new ArgumentNullException(nameof(watcher));

            ErrorWriter = errorWriter;

            _watcher.Created += OnCreated;
            _watcher.Changed += OnChanged;
            _watcher.Renamed += OnRenamed;
            _watcher.Deleted += OnDeleted;
            _watcher.Error += OnError;
        }

        /// <summary>
        /// A writer to a channel that will receive filesystem events.
        /// </summary>
        protected ChannelWriter<FileSystemEventArgs> Writer { get; }

        /// <summary>
        /// A writer to a channel that will receive error messages produced when filesystem change detection encounters errors.
        /// </summary>
        protected ChannelWriter<ErrorEventArgs>? ErrorWriter { get; }

        /// <summary>
        /// Options used to configure the filesystem watcher.
        /// </summary>
        protected IChannelFileSystemWatcherOptions Options { get; }

        /// <inheritdoc />
        public void Start() => EnableRaisingEvents = true;

        /// <inheritdoc />
        public void Stop() => EnableRaisingEvents = false;

        /// <summary>
        /// Gets or sets a value indicating whether the underlying <see cref="FileSystemWatcher"/> is enabled.
        /// </summary>
        protected bool EnableRaisingEvents
        {
            get => _watcher.EnableRaisingEvents;
            set
            {
                if (_watcher.EnableRaisingEvents == value)
                    return;

                if (_completed)
                {
                    throw new ArgumentException("Unable to start raising events again. The channels have been completed.", nameof(value));
                }

                if (value)
                {
                    _watcher.Path = Options.Path;
                    _watcher.IncludeSubdirectories = Options.IncludeSubdirectories;
                    _watcher.NotifyFilter = Options.NotifyFilter;
                    _watcher.Filter = Options.Filter;
                    foreach (var filter in Options.Filters)
                        _watcher.Filters.Add(filter);
                }

                _watcher.EnableRaisingEvents = value;
                if (!value)
                {
                    Writer.TryComplete();
                    ErrorWriter?.TryComplete();
                    _completed = true;
                }
            }
        }

        /// <summary>
        /// Releases any resources used by the <see cref="ChannelFileSystemWatcher"/> instance.
        /// </summary>
        public void Dispose() => Dispose(true);

        private void OnCreated(object? sender, FileSystemEventArgs e) => OnCreated(e);

        private void OnChanged(object? sender, FileSystemEventArgs e) => OnChanged(e);

        private void OnDeleted(object? sender, FileSystemEventArgs e) => OnDeleted(e);

        private void OnRenamed(object? sender, RenamedEventArgs e) => OnRenamed(e);

        private void OnError(object? sender, ErrorEventArgs e) => OnError(e);

        /// <summary>
        /// Raises an event when a file or directory is created.
        /// </summary>
        /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
        protected virtual void OnCreated(FileSystemEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            if (!Options.CreatedEnabled)
                return;

            Writer.TryWrite(e);
        }

        /// <summary>
        /// Raises an event when a file or directory is changed.
        /// </summary>
        /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
        protected virtual void OnChanged(FileSystemEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            if (!Options.ChangedEnabled)
                return;

            Writer.TryWrite(e);
        }

        /// <summary>
        /// Raises an event when a file or directory is deleted.
        /// </summary>
        /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
        protected virtual void OnDeleted(FileSystemEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            if (!Options.DeletedEnabled)
                return;

            Writer.TryWrite(e);
        }

        /// <summary>
        /// Raises an event when a file or directory is renamed.
        /// </summary>
        /// <param name="e">A <see cref="RenamedEventArgs"/> that contains the event data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
        protected virtual void OnRenamed(RenamedEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            if (!Options.RenamedEnabled)
                return;

            Writer.TryWrite(e);
        }

        /// <summary>
        /// Raises an event when a file or directory change detection fails.
        /// </summary>
        /// <param name="e">A <see cref="ErrorEventArgs"/> that contains the event data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
        protected virtual void OnError(ErrorEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            ErrorWriter?.TryWrite(e);
        }

        /// <summary>
        /// Releases the managed resources used by underlying <see cref="FileSystemWatcher"/>.
        /// </summary>
        /// <param name="disposing"><c>true</c> if managed resources are to be disposed. <c>false</c> will not dispose any resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (!disposing)
                return;

            if (_watcher is IDisposable disposable)
                disposable.Dispose();

            _disposed = true;
        }

        private bool _disposed;
        private bool _completed;

        private readonly IFileSystemWatcher _watcher;
    }
}
