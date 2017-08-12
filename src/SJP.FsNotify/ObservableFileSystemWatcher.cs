using System;
using System.IO;
using System.Reactive.Linq;

namespace SJP.FsNotify
{
    public class ObservableFileSystemWatcher : IObservableFileSystemWatcher
    {
        public ObservableFileSystemWatcher(FileSystemWatcherAdapter watcher)
            : this(watcher as IFileSystemWatcher)
        {
        }

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

        public void Start() => _watcher.EnableRaisingEvents = true;

        public void Stop() => _watcher.EnableRaisingEvents = false;

        public IObservable<FileSystemEventArgs> Changed { get; }

        public IObservable<RenamedEventArgs> Renamed { get; }

        public IObservable<FileSystemEventArgs> Deleted { get; }

        public IObservable<ErrorEventArgs> Errors { get; }

        public IObservable<FileSystemEventArgs> Created { get; }

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
        private readonly IFileSystemWatcher _watcher;
    }
}
