using System;
using System.Collections.ObjectModel;
using System.IO;
using EnumsNET;

namespace SJP.FsNotify
{
    /// <summary>
    /// Listens to the file system change notifications and raises events when a directory, or file in a directory, changes. Intended to wrap an instance of <see cref="FileSystemWatcher"/> in order to expose as an <see cref="IFileSystemWatcher"/>.
    /// </summary>
    public class FileSystemWatcherAdapter : IFileSystemWatcher, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemWatcherAdapter"/> class, given a <see cref="FileSystemWatcher"/> to derive information from.
        /// </summary>
        /// <param name="watcher">A file system watcher to derive events from. A regular <see cref="FileSystemWatcher"/> can be provided.</param>
        /// <exception cref="ArgumentNullException"><paramref name="watcher"/> is <c>null</c>.</exception>
        public FileSystemWatcherAdapter(FileSystemWatcher watcher)
        {
            _watcher = watcher ?? throw new ArgumentNullException(nameof(watcher));
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FileSystemWatcherAdapter"/> class, given a <see cref="FileSystemWatcher"/> to derive information from.
        /// </summary>
        /// <param name="watcher">A file system watcher to derive events from. A regular <see cref="FileSystemWatcher"/> can be provided.</param>
        /// <returns>A new instance of <see cref="FileSystemWatcherAdapter"/> that wraps <see cref="FileSystemWatcher"/> as an <see cref="IFileSystemWatcher"/>.</returns>
        public static implicit operator FileSystemWatcherAdapter(FileSystemWatcher watcher) => new(watcher);

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="Path"/> is changed.
        /// </summary>
        public event EventHandler<FileSystemEventArgs> Changed
        {
            add => _watcher.Changed += value.Invoke;
            remove => _watcher.Changed -= value.Invoke;
        }

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="Path"/> is created.
        /// </summary>
        public event EventHandler<FileSystemEventArgs> Created
        {
            add => _watcher.Created += value.Invoke;
            remove => _watcher.Created -= value.Invoke;
        }

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="Path"/> is deleted.
        /// </summary>
        public event EventHandler<FileSystemEventArgs> Deleted
        {
            add => _watcher.Deleted += value.Invoke;
            remove => _watcher.Deleted -= value.Invoke;
        }

        /// <summary>
        /// Occurs when the instance of <see cref="FileSystemWatcherAdapter"/> is unable to continue monitoring changes.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error
        {
            add => _watcher.Error += value.Invoke;
            remove => _watcher.Error -= value.Invoke;
        }

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="Path"/> is renamed.
        /// </summary>
        public event EventHandler<RenamedEventArgs> Renamed
        {
            add => _watcher.Renamed += value.Invoke;
            remove => _watcher.Renamed -= value.Invoke;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="FileSystemWatcherAdapter"/> is enabled.
        /// </summary>
        public bool EnableRaisingEvents
        {
            get => _watcher.EnableRaisingEvents;
            set => _watcher.EnableRaisingEvents = value;
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
        /// Gets the collection of all the filters used to determine what files are monitored in a directory.
        /// </summary>
        public Collection<string> Filters => _watcher.Filters;

        /// <summary>
        /// Gets or sets a value indicating whether subdirectories within the specified path should be monitored.
        /// </summary>
        public bool IncludeSubdirectories
        {
            get => _watcher.IncludeSubdirectories;
            set => _watcher.IncludeSubdirectories = value;
        }

        /// <summary>
        /// Gets or sets the type of changes to watch for.
        /// </summary>
        /// <exception cref="ArgumentException"><c>value</c> is an invalid enum.</exception>
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
        /// Gets or sets the path of the directory to watch.
        /// </summary>
        public string Path
        {
            get => _watcher.Path;
            set => _watcher.Path = value;
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
        /// Releases any resources used by the <see cref="FileSystemWatcherAdapter"/> instance.
        /// </summary>
        public void Dispose() => Dispose(true);

        /// <summary>
        /// Releases the managed resources used by the <see cref="FileSystemWatcherAdapter"/>.
        /// </summary>
        /// <param name="disposing"><c>true</c> if managed resources are to be disposed. <c>false</c> will not dispose any resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _watcher.Dispose();

            _disposed = true;
        }

        private bool _disposed;
        private readonly FileSystemWatcher _watcher;
    }
}
