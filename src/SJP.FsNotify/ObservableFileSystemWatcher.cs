using System;
using System.IO;
using System.Reactive.Linq;

namespace SJP.FsNotify
{
    /// <summary>
    /// Reactively publishes file system change notifications and events when a directory, or file in a directory, changes.
    /// </summary>
    public class ObservableFileSystemWatcher : IObservableFileSystemWatcher
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableFileSystemWatcher"/> class, given the specified directory to monitor.
        /// </summary>
        /// <param name="path">The directory to monitor, in standard or Universal Naming Convention (UNC) notation.</param>
        public ObservableFileSystemWatcher(string path)
            : this(new BufferedFileSystemWatcher(path))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableFileSystemWatcher"/> class, given the specified directory to monitor.
        /// </summary>
        /// <param name="directory">The directory to monitor, in standard or Universal Naming Convention (UNC) notation.</param>
        public ObservableFileSystemWatcher(DirectoryInfo directory)
            : this(new BufferedFileSystemWatcher(directory))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableFileSystemWatcher"/> class, given the specified directory and type of files to monitor.
        /// </summary>
        /// <param name="path">The directory to monitor, in standard or Universal Naming Convention (UNC) notation.</param>
        /// <param name="filter">The type of files to watch. For example, <c>*.txt</c> watches for changes to all text files.</param>
        public ObservableFileSystemWatcher(string path, string filter)
            : this(new BufferedFileSystemWatcher(path, filter))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableFileSystemWatcher"/> class, given the specified directory and type of files to monitor.
        /// </summary>
        /// <param name="directory">The directory to monitor, in standard or Universal Naming Convention (UNC) notation.</param>
        /// <param name="filter">The type of files to watch. For example, <c>*.txt</c> watches for changes to all text files.</param>
        public ObservableFileSystemWatcher(DirectoryInfo directory, string filter)
            : this(new BufferedFileSystemWatcher(directory, filter))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableFileSystemWatcher"/> class, given a <see cref="FileSystemWatcher"/> to derive information from.
        /// </summary>
        /// <param name="watcher">A file system watcher to derive events from. A regular <see cref="FileSystemWatcher"/> can be provided.</param>
        public ObservableFileSystemWatcher(FileSystemWatcherAdapter watcher)
            : this(new BufferedFileSystemWatcher(watcher))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableFileSystemWatcher"/> class, given an <see cref="IFileSystemWatcher"/> to derive information from.
        /// </summary>
        /// <param name="watcher">A file system watcher to derive events from.</param>
        public ObservableFileSystemWatcher(IFileSystemWatcher watcher)
        {
            _watcher = watcher ?? throw new ArgumentNullException(nameof(watcher));

            Changed = Observable
                .FromEventPattern<EventHandler<FileSystemEventArgs>, FileSystemEventArgs>(h => _watcher.Changed += h, h => _watcher.Changed -= h)
                .Select(x => x.EventArgs);

            Renamed = Observable
                .FromEventPattern<EventHandler<RenamedEventArgs>, RenamedEventArgs>(h => _watcher.Renamed += h, h => _watcher.Renamed -= h)
                .Select(x => x.EventArgs);

            Deleted = Observable
                .FromEventPattern<EventHandler<FileSystemEventArgs>, FileSystemEventArgs>(h => _watcher.Deleted += h, h => _watcher.Deleted -= h)
                .Select(x => x.EventArgs);

            Errors = Observable
                .FromEventPattern<EventHandler<ErrorEventArgs>, ErrorEventArgs>(h => _watcher.Error += h, h => _watcher.Error -= h)
                .Select(x => x.EventArgs);

            Created = Observable
                .FromEventPattern<EventHandler<FileSystemEventArgs>, FileSystemEventArgs>(h => _watcher.Created += h, h => _watcher.Created -= h)
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
        /// Releases any resources used by the <see cref="ObservableFileSystemWatcher"/> instance.
        /// </summary>
        public void Dispose() => Dispose(true);

        /// <summary>
        /// Releases the managed resources used by the <see cref="ObservableFileSystemWatcher"/>.
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
        private readonly IFileSystemWatcher _watcher;
    }
}
