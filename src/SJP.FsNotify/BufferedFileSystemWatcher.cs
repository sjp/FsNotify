using System;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using EnumsNET;

namespace SJP.FsNotify
{
    public class BufferedFileSystemWatcher : IFileSystemWatcher
    {
        public BufferedFileSystemWatcher(int capacity = int.MaxValue)
            : this(new FileSystemWatcher(), capacity)
        {
        }

        public BufferedFileSystemWatcher(string path, int capacity = int.MaxValue)
            : this(new FileSystemWatcher(path), capacity)
        {
        }

        public BufferedFileSystemWatcher(string path, string filter, int capacity = int.MaxValue)
            : this(new FileSystemWatcher(path, filter), capacity)
        {
        }

        public BufferedFileSystemWatcher(FileSystemWatcherAdapter watcher, int capacity = int.MaxValue)
            : this(watcher as IFileSystemWatcher, capacity)
        {
        }

        public BufferedFileSystemWatcher(IFileSystemWatcher watcher, int capacity = int.MaxValue)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(capacity), "The bounding capacity must be at least 1. Given: " + capacity.ToString());

            _buffer = new BlockingCollection<FileSystemEventArgs>(capacity);
            _watcher = watcher ?? throw new ArgumentNullException(nameof(watcher));
        }

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

        public string Path
        {
            get => _watcher.Path;
            set => _watcher.Path = value;
        }

        public NotifyFilters NotifyFilter
        {
            get => _watcher.NotifyFilter;
            set
            {
                if (!value.IsValid())
                    throw new ArgumentException($"The { nameof(NotifyFilters) } provided must be a valid enum.", nameof(NotifyFilter));

                _watcher.NotifyFilter = value;
            }
        }

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
                _watcher.Created -= OnCreated;
                _onCreated -= value;
            }
        }

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
                _watcher.Changed -= OnChanged;
                _onChanged -= value;
            }
        }

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
                _watcher.Deleted -= OnDeleted;
                _onDeleted -= value;
            }
        }

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
                _watcher.Renamed -= OnRenamed;
                _onRenamed -= value;
            }
        }

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
                if (_onError == null)
                    _watcher.Error -= OnError;
                _onError -= value;
            }
        }

        public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType)
        {
            if (!changeType.IsValid())
                throw new ArgumentException($"The { nameof(WatcherChangeTypes) } provided must be a valid enum.", nameof(changeType));

            return _watcher.WaitForChanged(changeType);
        }

        public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout)
        {
            if (!changeType.IsValid())
                throw new ArgumentException($"The { nameof(WatcherChangeTypes) } provided must be a valid enum.", nameof(changeType));

            return _watcher.WaitForChanged(changeType, timeout);
        }

        public void Dispose() => Dispose(true);

        protected void RaiseBufferedFileSystemEvents()
        {
            Task.Run(() =>
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
                            _onRenamed?.Invoke(this, fsEvent as RenamedEventArgs);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(WatcherChangeTypes), $"Unknown or unexpected value for { nameof(WatcherChangeTypes) }.");
                    }
                }
            });
        }

        protected virtual void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (!_buffer.TryAdd(e))
                OnBufferExceeded();
        }

        protected virtual void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (!_buffer.TryAdd(e))
                OnBufferExceeded();
        }

        protected virtual void OnDeleted(object sender, FileSystemEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (!_buffer.TryAdd(e))
                OnBufferExceeded();
        }

        protected virtual void OnRenamed(object sender, RenamedEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (!_buffer.TryAdd(e))
                OnBufferExceeded();
        }

        protected virtual void OnError(object sender, ErrorEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            _onError?.Invoke(this, e);
        }

        protected virtual void OnBufferExceeded()
        {
            var ex = new BufferExhaustedException($"File system event queue buffer exhausted. { _buffer.BoundedCapacity } events exceeded.", _buffer.BoundedCapacity);
            _onError?.Invoke(this, new ErrorEventArgs(ex));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (!disposing)
                return;

            _cts?.Dispose();
            _watcher.Dispose();

            _onCreated = null;
            _onChanged = null;
            _onDeleted = null;
            _onRenamed = null;
            _onError = null;

            _disposed = true;
        }

        private bool _disposed;
        private CancellationTokenSource _cts;

        private EventHandler<FileSystemEventArgs> _onCreated;
        private EventHandler<FileSystemEventArgs> _onChanged;
        private EventHandler<FileSystemEventArgs> _onDeleted;
        private EventHandler<RenamedEventArgs> _onRenamed;
        private EventHandler<ErrorEventArgs> _onError;

        private readonly IFileSystemWatcher _watcher;
        private readonly BlockingCollection<FileSystemEventArgs> _buffer;
    }
}
