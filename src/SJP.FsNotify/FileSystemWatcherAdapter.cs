using System;
using System.IO;

namespace SJP.FsNotify
{
    public class FileSystemWatcherAdapter : IFileSystemWatcher
    {
        public FileSystemWatcherAdapter(FileSystemWatcher watcher)
        {
            _watcher = watcher ?? throw new ArgumentNullException(nameof(watcher));
        }

        public static implicit operator FileSystemWatcherAdapter(FileSystemWatcher watcher) => new FileSystemWatcherAdapter(watcher);

        public event EventHandler<FileSystemEventArgs> Changed
        {
            add => _watcher.Changed += value.Invoke;
            remove => _watcher.Changed -= value.Invoke;
        }

        public event EventHandler<FileSystemEventArgs> Created
        {
            add => _watcher.Created += value.Invoke;
            remove => _watcher.Created -= value.Invoke;
        }

        public event EventHandler<FileSystemEventArgs> Deleted
        {
            add => _watcher.Deleted += value.Invoke;
            remove => _watcher.Deleted -= value.Invoke;
        }

        public event EventHandler<ErrorEventArgs> Error
        {
            add => _watcher.Error += value.Invoke;
            remove => _watcher.Error -= value.Invoke;
        }

        public event EventHandler<RenamedEventArgs> Renamed
        {
            add => _watcher.Renamed += value.Invoke;
            remove => _watcher.Renamed -= value.Invoke;
        }

        public bool EnableRaisingEvents
        {
            get => _watcher.EnableRaisingEvents;
            set => _watcher.EnableRaisingEvents = value;
        }

        public string Filter
        {
            get => _watcher.Filter;
            set => _watcher.Filter = value;
        }

        public bool IncludeSubdirectories
        {
            get => _watcher.IncludeSubdirectories;
            set => _watcher.IncludeSubdirectories = value;
        }

        public NotifyFilters NotifyFilter
        {
            get => _watcher.NotifyFilter;
            set => _watcher.NotifyFilter = value;
        }

        public string Path
        {
            get => _watcher.Path;
            set => _watcher.Path = value;
        }

        public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType) => _watcher.WaitForChanged(changeType);

        public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout) => _watcher.WaitForChanged(changeType, timeout);

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
        private readonly FileSystemWatcher _watcher;
    }
}
