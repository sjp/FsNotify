using System;
using System.IO;
using System.Reactive.Linq;

namespace SJP.FsNotify
{
    public class EnhancedObservableFileSystemWatcher : IEnhancedObservableFileSystemWatcher
    {
        public EnhancedObservableFileSystemWatcher(int capacity = int.MaxValue)
            : this(new EnhancedFileSystemWatcher(capacity))
        {
        }

        public EnhancedObservableFileSystemWatcher(string path, int capacity = int.MaxValue)
            : this(new EnhancedFileSystemWatcher(path, capacity))
        {
        }

        public EnhancedObservableFileSystemWatcher(string path, string filter, int capacity = int.MaxValue)
            : this(new EnhancedFileSystemWatcher(path, filter, capacity))
        {
        }

        public EnhancedObservableFileSystemWatcher(FileSystemWatcherAdapter watcher, int capacity = int.MaxValue)
            : this(new EnhancedFileSystemWatcher(watcher, capacity))
        {
        }

        public EnhancedObservableFileSystemWatcher(IFileSystemWatcher watcher, int capacity = int.MaxValue)
            : this(new EnhancedFileSystemWatcher(watcher, capacity))
        {
        }

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

        public void Start() => _watcher.EnableRaisingEvents = true;

        public void Stop() => _watcher.EnableRaisingEvents = false;

        public IObservable<FileSystemEventArgs> Changed { get; }

        public IObservable<RenamedEventArgs> Renamed { get; }

        public IObservable<FileSystemEventArgs> Deleted { get; }

        public IObservable<ErrorEventArgs> Errors { get; }

        public IObservable<FileSystemEventArgs> Created { get; }

        public IObservable<FileSystemEventArgs> AttributeChanged { get; }

        public IObservable<FileSystemEventArgs> CreationTimeChanged { get; }

        public IObservable<FileSystemEventArgs> LastAccessChanged { get; }

        public IObservable<FileSystemEventArgs> LastWriteChanged { get; }

        public IObservable<FileSystemEventArgs> SecurityChanged { get; }

        public IObservable<FileSystemEventArgs> SizeChanged { get; }

        public void Dispose() => Dispose(true);

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
