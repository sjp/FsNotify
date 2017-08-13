using System;
using System.IO;
using System.Reactive.Linq;

namespace SJP.FsNotify
{
    /// <summary>
    /// Reactively publishes file system change notifications and events when a directory, or file in a directory, changes. Also provides further subscriptions in addition to those provided by <see cref="IObservableFileSystemWatcher"/>.
    /// </summary>
    public class EnhancedObservableFileSystemWatcher : IEnhancedObservableFileSystemWatcher
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnhancedObservableFileSystemWatcher"/> class, given the specified directory to monitor.
        /// </summary>
        /// <param name="path">The directory to monitor, in standard or Universal Naming Convention (UNC) notation.</param>
        public EnhancedObservableFileSystemWatcher(string path)
            : this(new EnhancedFileSystemWatcher(path))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnhancedObservableFileSystemWatcher"/> class, given the specified directory and type of files to monitor.
        /// </summary>
        /// <param name="path">The directory to monitor, in standard or Universal Naming Convention (UNC) notation.</param>
        /// <param name="filter">The type of files to watch. For example, <c>*.txt</c> watches for changes to all text files.</param>
        public EnhancedObservableFileSystemWatcher(string path, string filter)
            : this(new EnhancedFileSystemWatcher(path, filter))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnhancedObservableFileSystemWatcher"/> class, given a <see cref="FileSystemWatcher"/> to derive information from.
        /// </summary>
        /// <param name="watcher">A file system watcher to derive events from. A regular <see cref="FileSystemWatcher"/> can be provided.</param>
        public EnhancedObservableFileSystemWatcher(FileSystemWatcherAdapter watcher)
            : this(new EnhancedFileSystemWatcher(watcher))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnhancedObservableFileSystemWatcher"/> class, given an <see cref="IFileSystemWatcher"/> to derive information from.
        /// </summary>
        /// <param name="watcher">A file system watcher to derive events from.</param>
        public EnhancedObservableFileSystemWatcher(IFileSystemWatcher watcher)
            : this(new EnhancedFileSystemWatcher(watcher))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnhancedFileSystemWatcher"/> class, given an <see cref="IEnhancedFileSystemWatcher"/> to derive information from.
        /// </summary>
        /// <param name="watcher">An enhanced file system watcher to derive events from.</param>
        public EnhancedObservableFileSystemWatcher(IEnhancedFileSystemWatcher watcher)
        {
            _watcher = watcher ?? throw new ArgumentNullException(nameof(watcher));

            Changed = Observable
                .FromEventPattern<EventHandler<FileSystemEventArgs>, FileSystemEventArgs>(h => _watcher.Changed += h, h => _watcher.Changed -= h)
                .Select(x => x.EventArgs);

            Created = Observable
                .FromEventPattern<EventHandler<FileSystemEventArgs>, FileSystemEventArgs>(h => _watcher.Created += h, h => _watcher.Created -= h)
                .Select(x => x.EventArgs);

            Deleted = Observable
                .FromEventPattern<EventHandler<FileSystemEventArgs>, FileSystemEventArgs>(h => _watcher.Deleted += h, h => _watcher.Deleted -= h)
                .Select(x => x.EventArgs);

            Errors = Observable
                .FromEventPattern<EventHandler<ErrorEventArgs>, ErrorEventArgs>(h => _watcher.Error += h, h => _watcher.Error -= h)
                .Select(x => x.EventArgs);

            Renamed = Observable
                .FromEventPattern<EventHandler<RenamedEventArgs>, RenamedEventArgs>(h => _watcher.Renamed += h, h => _watcher.Renamed -= h)
                .Select(x => x.EventArgs);

            AttributeChanged = Observable
                .FromEventPattern<EventHandler<FileSystemEventArgs>, FileSystemEventArgs>(h => _watcher.AttributeChanged += h, h => _watcher.AttributeChanged -= h)
                .Select(x => x.EventArgs);

            CreationTimeChanged = Observable
                .FromEventPattern<EventHandler<FileSystemEventArgs>, FileSystemEventArgs>(h => _watcher.CreationTimeChanged += h, h => _watcher.CreationTimeChanged -= h)
                .Select(x => x.EventArgs);

            LastAccessChanged = Observable
                .FromEventPattern<EventHandler<FileSystemEventArgs>, FileSystemEventArgs>(h => _watcher.LastAccessChanged += h, h => _watcher.LastAccessChanged -= h)
                .Select(x => x.EventArgs);

            LastWriteChanged = Observable
                .FromEventPattern<EventHandler<FileSystemEventArgs>, FileSystemEventArgs>(h => _watcher.LastWriteChanged += h, h => _watcher.LastWriteChanged -= h)
                .Select(x => x.EventArgs);

            LastWriteChanged = Observable
                .FromEventPattern<EventHandler<FileSystemEventArgs>, FileSystemEventArgs>(h => _watcher.LastWriteChanged += h, h => _watcher.LastWriteChanged -= h)
                .Select(x => x.EventArgs);

            SecurityChanged = Observable
                .FromEventPattern<EventHandler<FileSystemEventArgs>, FileSystemEventArgs>(h => _watcher.SecurityChanged += h, h => _watcher.SecurityChanged -= h)
                .Select(x => x.EventArgs);

            SizeChanged = Observable
                .FromEventPattern<EventHandler<FileSystemEventArgs>, FileSystemEventArgs>(h => _watcher.SizeChanged += h, h => _watcher.SizeChanged -= h)
                .Select(x => x.EventArgs);
        }

        /// <summary>
        /// Begins monitoring of file system events.
        /// </summary>
        public void Start() => _watcher.EnableRaisingEvents = true;

        /// <summary>
        /// Stops monitoring of file system events.
        /// </summary>
        public void Stop() => _watcher.EnableRaisingEvents = false;

        /// <summary>
        /// Provides a subscription to file system events whenever a file or directory is changed.
        /// </summary>
        public IObservable<FileSystemEventArgs> Changed { get; }

        /// <summary>
        /// Provides a subscription to file system events whenever a file or directory is renamed.
        /// </summary>
        public IObservable<RenamedEventArgs> Renamed { get; }

        /// <summary>
        /// Provides a subscription to file system events whenever a file or directory is deleted.
        /// </summary>
        public IObservable<FileSystemEventArgs> Deleted { get; }

        /// <summary>
        /// Provides a subscription to notifications when the instance of <see cref="IObservableFileSystemWatcher"/> is unable to continue monitoring changes.
        /// </summary>
        public IObservable<ErrorEventArgs> Errors { get; }

        /// <summary>
        /// Provides a subscription to file system events whenever a file or directory is created.
        /// </summary>
        public IObservable<FileSystemEventArgs> Created { get; }

        /// <summary>
        /// Provides a subscription to file system events whenever a file or directory attribute has changed.
        /// </summary>
        public IObservable<FileSystemEventArgs> AttributeChanged { get; }

        /// <summary>
        /// Provides a subscription to file system events whenever a file or directory creation time has changed.
        /// </summary>
        public IObservable<FileSystemEventArgs> CreationTimeChanged { get; }

        /// <summary>
        /// Provides a subscription to file system events whenever a file or directory last access time has changed.
        /// </summary>
        public IObservable<FileSystemEventArgs> LastAccessChanged { get; }

        /// <summary>
        /// Provides a subscription to file system events whenever a file or directory last write time has changed.
        /// </summary>
        public IObservable<FileSystemEventArgs> LastWriteChanged { get; }

        /// <summary>
        /// Provides a subscription to file system events whenever file or directory security settings have changed.
        /// </summary>
        public IObservable<FileSystemEventArgs> SecurityChanged { get; }

        /// <summary>
        /// Provides a subscription to file system events whenever a file or directory size has changed.
        /// </summary>
        public IObservable<FileSystemEventArgs> SizeChanged { get; }

        /// <summary>
        /// Releases any resources used by the <see cref="EnhancedObservableFileSystemWatcher"/> instance.
        /// </summary>
        public void Dispose() => Dispose(true);

        /// <summary>
        /// Releases the managed resources used by the <see cref="BufferedFileSystemWatcher"/>.
        /// </summary>
        /// <param name="disposing"><b>True</b> if managed resources are to be disposed. <b>False</b> will not dispose any resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _watcher.Dispose();

            _disposed = true;
        }

        private bool _disposed;
        private readonly IEnhancedFileSystemWatcher _watcher;
    }
}
