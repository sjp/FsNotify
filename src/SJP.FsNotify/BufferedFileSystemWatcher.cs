using System;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using EnumsNET;

namespace SJP.FsNotify
{
    /// <summary>
    /// Listens to the file system change notifications and raises events when a directory, or file in a directory, changes. Improves upon <see cref="FileSystemWatcher"/> by having a much larger buffer.
    /// </summary>
    public class BufferedFileSystemWatcher : IFileSystemWatcher
    {
        /// <summary>
        /// Initializes an instance of the <see cref="BufferedFileSystemWatcher"/> class.
        /// </summary>
        /// <param name="capacity">The maximum number of file system events to buffer before stopping.</param>
        public BufferedFileSystemWatcher(int capacity = int.MaxValue)
            : this(new FileSystemWatcher(), capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferedFileSystemWatcher"/> class, given the specified directory and type of files to monitor.
        /// </summary>
        /// <param name="path">The directory to monitor, in standard or Universal Naming Convention (UNC) notation.</param>
        /// <param name="capacity">The maximum number of file system events to buffer before stopping.</param>
        public BufferedFileSystemWatcher(string path, int capacity = int.MaxValue)
            : this(new FileSystemWatcher(path), capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferedFileSystemWatcher"/> class, given the specified directory and type of files to monitor.
        /// </summary>
        /// <param name="directory">The directory to monitor, in standard or Universal Naming Convention (UNC) notation.</param>
        /// <param name="capacity">The maximum number of file system events to buffer before stopping.</param>
        /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <c>null</c>.</exception>
        public BufferedFileSystemWatcher(DirectoryInfo directory, int capacity = int.MaxValue)
            : this(new FileSystemWatcher(directory?.FullName ?? throw new ArgumentNullException(nameof(directory))), capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferedFileSystemWatcher"/> class, given a <see cref="FileSystemWatcher"/> to derive information from.
        /// </summary>
        /// <param name="path">The directory to monitor, in standard or Universal Naming Convention (UNC) notation.</param>
        /// <param name="filter">The type of files to watch. For example, <c>*.txt</c> watches for changes to all text files.</param>
        /// <param name="capacity">The maximum number of file system events to buffer before stopping.</param>
        public BufferedFileSystemWatcher(string path, string filter, int capacity = int.MaxValue)
            : this(new FileSystemWatcher(path, filter), capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferedFileSystemWatcher"/> class, given a <see cref="FileSystemWatcher"/> to derive information from.
        /// </summary>
        /// <param name="directory">The directory to monitor, in standard or Universal Naming Convention (UNC) notation.</param>
        /// <param name="filter">The type of files to watch. For example, <c>*.txt</c> watches for changes to all text files.</param>
        /// <param name="capacity">The maximum number of file system events to buffer before stopping.</param>
        /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <c>null</c>.</exception>
        public BufferedFileSystemWatcher(DirectoryInfo directory, string filter, int capacity = int.MaxValue)
            : this(new FileSystemWatcher(directory?.FullName ?? throw new ArgumentNullException(nameof(directory)), filter), capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferedFileSystemWatcher"/> class, given a <see cref="FileSystemWatcher"/> to derive information from.
        /// </summary>
        /// <param name="watcher">A file system watcher to derive events from.</param>
        /// <param name="capacity">The maximum number of file system events to buffer before stopping.</param>
        public BufferedFileSystemWatcher(FileSystemWatcherAdapter watcher, int capacity = int.MaxValue)
            : this(watcher as IFileSystemWatcher, capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferedFileSystemWatcher"/> class, given an <see cref="IFileSystemWatcher"/> to derive information from.
        /// </summary>
        /// <param name="watcher">A file system watcher to derive events from.</param>
        /// <param name="capacity">The maximum number of file system events to buffer before stopping.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than <c>1</c>.</exception>
        public BufferedFileSystemWatcher(IFileSystemWatcher watcher, int capacity = int.MaxValue)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(capacity), "The bounding capacity must be at least 1. Given: " + capacity.ToString());

            _buffer = new BlockingCollection<FileSystemEventArgs>(capacity);
            _watcher = watcher ?? throw new ArgumentNullException(nameof(watcher));
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="BufferedFileSystemWatcher"/> is enabled.
        /// </summary>
        public bool EnableRaisingEvents
        {
            get => _watcher.EnableRaisingEvents;
            set
            {
                if (_watcher.EnableRaisingEvents == value)
                    return;

                _watcher.EnableRaisingEvents = value;

                if (value)
                {
                    _cts = new CancellationTokenSource();
                    RaiseBufferedFileSystemEvents();
                }
            }
        }

        /// <summary>
        /// Gets or sets the filter string used to determine what files are monitored in a directory.
        /// </summary>
        public string Filter
        {
            get => _watcher.Filter;
            set => _watcher.Filter = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether subdirectories within the specified path should be monitored.
        /// </summary>
        public bool IncludeSubdirectories
        {
            get => _watcher.IncludeSubdirectories;
            set => _watcher.IncludeSubdirectories = value;
        }

        /// <summary>
        /// Gets or sets the path of the directory to watch.
        /// </summary>
        public string Path
        {
            get => _watcher.Path;
            set => _watcher.Path = value;
        }

        /// <summary>
        /// Gets or sets the type of changes to watch for.
        /// </summary>
        /// <exception cref="ArgumentException"><c>value</c> is not a valid enum.</exception>
        public NotifyFilters NotifyFilter
        {
            get => _watcher.NotifyFilter;
            set
            {
                if (!value.IsValid())
                    throw new ArgumentException($"The { nameof(NotifyFilters) } provided must be a valid enum.", nameof(value));

                _watcher.NotifyFilter = value;
            }
        }

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="Path"/> is created.
        /// </summary>
        public event EventHandler<FileSystemEventArgs> Created
        {
            add
            {
                if (_onCreated == null)
                    _watcher.Created += OnCreated;
                _onCreated += value;
            }
            remove
            {
                _onCreated -= value;
                if (_onCreated == null)
                    _watcher.Created -= OnCreated;
            }
        }

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="Path"/> is changed.
        /// </summary>
        public event EventHandler<FileSystemEventArgs> Changed
        {
            add
            {
                if (_onChanged == null)
                    _watcher.Changed += OnChanged;
                _onChanged += value;
            }
            remove
            {
                _onChanged -= value;
                if (_onChanged == null)
                    _watcher.Changed -= OnChanged;
            }
        }

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="Path"/> is deleted.
        /// </summary>
        public event EventHandler<FileSystemEventArgs> Deleted
        {
            add
            {
                if (_onDeleted == null)
                    _watcher.Deleted += OnDeleted;
                _onDeleted += value;
            }
            remove
            {
                _onDeleted -= value;
                if (_onDeleted == null)
                    _watcher.Deleted -= OnDeleted;
            }
        }

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="Path"/> is renamed.
        /// </summary>
        public event EventHandler<RenamedEventArgs> Renamed
        {
            add
            {
                if (_onRenamed == null)
                    _watcher.Renamed += OnRenamed;
                _onRenamed += value;
            }
            remove
            {
                _onRenamed -= value;
                if (_onRenamed == null)
                    _watcher.Renamed -= OnRenamed;
            }
        }

        /// <summary>
        /// Occurs when the instance of <see cref="BufferedFileSystemWatcher"/> is unable to continue monitoring changes.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error
        {
            add
            {
                if (_onError == null)
                    _watcher.Error += OnError;
                _onError += value;
            }
            remove
            {
                _onError -= value;
                if (_onError == null)
                    _watcher.Error -= OnError;
            }
        }

        /// <summary>
        /// A synchronous method that returns a structure that contains specific information on the change that occurred, given the type of change you want to monitor.
        /// </summary>
        /// <param name="changeType">The <see cref="WatcherChangeTypes"/> to watch for.</param>
        /// <returns>A <see cref="WaitForChangedResult"/> that contains specific information on the change that occurred.</returns>
        /// <exception cref="ArgumentException"><paramref name="changeType"/> is not a valid enum.</exception>
        public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType)
        {
            if (!changeType.IsValid())
                throw new ArgumentException($"The { nameof(WatcherChangeTypes) } provided must be a valid enum.", nameof(changeType));

            return _watcher.WaitForChanged(changeType);
        }

        /// <summary>
        /// A synchronous method that returns a structure that contains specific information on the change that occurred, given the type of change you want to monitor and the time (in milliseconds) to wait before timing out.
        /// </summary>
        /// <param name="changeType">The <see cref="WatcherChangeTypes"/> to watch for.</param>
        /// <param name="timeout">The time (in milliseconds) to wait before timing out.</param>
        /// <returns>A <see cref="WaitForChangedResult"/> that contains specific information on the change that occurred.</returns>
        /// <exception cref="ArgumentException"><paramref name="changeType"/> is not a valid enum.</exception>
        public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout)
        {
            if (!changeType.IsValid())
                throw new ArgumentException($"The { nameof(WatcherChangeTypes) } provided must be a valid enum.", nameof(changeType));

            return _watcher.WaitForChanged(changeType, timeout);
        }

        /// <summary>
        /// Releases any resources used by the <see cref="BufferedFileSystemWatcher"/> instance.
        /// </summary>
        public void Dispose() => Dispose(true);

        /// <summary>
        /// When called, begins buffering of file system events in the background.
        /// </summary>
        protected void RaiseBufferedFileSystemEvents()
        {
            if (_buffer == null || _cts == null)
                return;

            _ = Task.Run(() =>
              {
                  foreach (var fsEvent in _buffer.GetConsumingEnumerable(_cts.Token))
                  {
                      switch (fsEvent.ChangeType)
                      {
                          case WatcherChangeTypes.Created:
                              _onCreated?.Invoke(this, fsEvent);
                              break;
                          case WatcherChangeTypes.Changed:
                              _onChanged?.Invoke(this, fsEvent);
                              break;
                          case WatcherChangeTypes.Deleted:
                              _onDeleted?.Invoke(this, fsEvent);
                              break;
                          case WatcherChangeTypes.Renamed:
                              if (fsEvent is RenamedEventArgs renamedArgs)
                                  _onRenamed?.Invoke(this, renamedArgs);
                              break;
                          default:
                              throw new NotSupportedException($"Unknown or unexpected value for { nameof(WatcherChangeTypes) }.");
                      }
                  }
              });
        }

        private void OnCreated(object sender, FileSystemEventArgs e) => OnCreated(e);

        private void OnChanged(object sender, FileSystemEventArgs e) => OnChanged(e);

        private void OnDeleted(object sender, FileSystemEventArgs e) => OnDeleted(e);

        private void OnRenamed(object sender, RenamedEventArgs e) => OnRenamed(e);

        private void OnError(object sender, ErrorEventArgs e) => OnError(e);

        /// <summary>
        /// Raises the <see cref="Created"/> event.
        /// </summary>
        /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
        protected virtual void OnCreated(FileSystemEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (_buffer == null)
                return;

            if (!_buffer.TryAdd(e))
                OnBufferExceeded();
        }

        /// <summary>
        /// Raises the <see cref="Changed"/> event.
        /// </summary>
        /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
        protected virtual void OnChanged(FileSystemEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (_buffer == null)
                return;

            if (!_buffer.TryAdd(e))
                OnBufferExceeded();
        }

        /// <summary>
        /// Raises the <see cref="Deleted"/> event.
        /// </summary>
        /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
        protected virtual void OnDeleted(FileSystemEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (_buffer == null)
                return;

            if (!_buffer.TryAdd(e))
                OnBufferExceeded();
        }

        /// <summary>
        /// Raises the <see cref="Renamed"/> event.
        /// </summary>
        /// <param name="e">A <see cref="RenamedEventArgs"/> that contains the event data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
        protected virtual void OnRenamed(RenamedEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (_buffer == null)
                return;

            if (!_buffer.TryAdd(e))
                OnBufferExceeded();
        }

        /// <summary>
        /// Raises the <see cref="Error"/> event.
        /// </summary>
        /// <param name="e">A <see cref="ErrorEventArgs"/> that contains the event data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
        protected virtual void OnError(ErrorEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            _onError?.Invoke(this, e);
        }

        /// <summary>
        /// Handles the scenario where the in-memory buffer has been exhausted.
        /// </summary>
        protected virtual void OnBufferExceeded()
        {
            if (_buffer == null)
                return;

            var ex = new BufferExhaustedException($"File system event queue buffer exhausted. { _buffer.BoundedCapacity } events exceeded.", _buffer.BoundedCapacity);
            _onError?.Invoke(this, new ErrorEventArgs(ex));
        }

        /// <summary>
        /// Releases the managed resources used by the <see cref="BufferedFileSystemWatcher"/>.
        /// </summary>
        /// <param name="disposing"><c>true</c> if managed resources are to be disposed. <c>false</c> will not dispose any resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (!disposing)
                return;

            _cts?.Dispose();
            _buffer?.Dispose();
            _watcher.Dispose();

            _onCreated = null;
            _onChanged = null;
            _onDeleted = null;
            _onRenamed = null;
            _onError = null;

            _disposed = true;
        }

        private bool _disposed;
        private CancellationTokenSource? _cts;

        private EventHandler<FileSystemEventArgs>? _onCreated;
        private EventHandler<FileSystemEventArgs>? _onChanged;
        private EventHandler<FileSystemEventArgs>? _onDeleted;
        private EventHandler<RenamedEventArgs>? _onRenamed;
        private EventHandler<ErrorEventArgs>? _onError;

        private readonly IFileSystemWatcher _watcher;
        private readonly BlockingCollection<FileSystemEventArgs>? _buffer;
    }
}
