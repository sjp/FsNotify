using System;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
using EnumsNET;
using System.Linq;

namespace SJP.FsNotify
{
    public class EnhancedFileSystemWatcher : IEnhancedFileSystemWatcher
    {
        public EnhancedFileSystemWatcher(int capacity = int.MaxValue)
            : this(new FileSystemWatcher(), capacity)
        {
        }

        public EnhancedFileSystemWatcher(string path, int capacity = int.MaxValue)
            : this(new FileSystemWatcher(path), capacity)
        {
        }

        public EnhancedFileSystemWatcher(string path, string filter, int capacity = int.MaxValue)
            : this(new FileSystemWatcher(path, filter), capacity)
        {
        }

        public EnhancedFileSystemWatcher(FileSystemWatcherAdapter watcher, int capacity = int.MaxValue)
            : this(watcher as IFileSystemWatcher, capacity)
        {
        }

        public EnhancedFileSystemWatcher(IFileSystemWatcher watcher, int capacity = int.MaxValue)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(capacity), "The bounding capacity must be at least 1. Given: " + capacity.ToString());

            _buffer = new BlockingCollection<FileSystemEventArgs>(capacity);
            _watcher = watcher ?? throw new ArgumentNullException(nameof(watcher));
            UpdateFilterWatchers();
        }

        public bool EnableRaisingEvents
        {
            get => _watcher.EnableRaisingEvents;
            set
            {
                if (_watcher.EnableRaisingEvents == value)
                    return;

                _watcher.EnableRaisingEvents = value;
                foreach (var watcher in _changeWatchers.Values)
                    watcher.EnableRaisingEvents = value;

                if (value)
                {
                    _cts = new CancellationTokenSource();
                    //RaiseBufferedFileSystemEvents();
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
                _watcher.NotifyFilter = value;
                UpdateFilterWatchers();
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

        public event EventHandler<FileSystemEventArgs> AttributeChanged
        {
            add
            {
                var filter = NotifyFilters.Attributes;
                var handler = _notifyHandlers[filter];
                handler += value;
                _notifyHandlers[filter] = handler;

                var watcher = _changeWatchers[filter];
                if (watcher != null)
                    watcher.Changed += handler;
            }
            remove
            {
                var filter = NotifyFilters.Attributes;
                var handler = _notifyHandlers[filter];
                handler -= value;
                _notifyHandlers[filter] = handler;
            }
        }

        public event EventHandler<FileSystemEventArgs> CreationTimeChanged
        {
            add
            {
                var filter = NotifyFilters.CreationTime;
                var handler = _notifyHandlers[filter];
                handler += value;
                _notifyHandlers[filter] = handler;

                var watcher = _changeWatchers[filter];
                if (watcher != null)
                    watcher.Changed += handler;
            }
            remove
            {
                var filter = NotifyFilters.CreationTime;
                var handler = _notifyHandlers[filter];
                handler -= value;
                _notifyHandlers[filter] = handler;
            }
        }

        public event EventHandler<FileSystemEventArgs> DirectoryNameChanged
        {
            add
            {
                var filter = NotifyFilters.DirectoryName;
                var handler = _notifyHandlers[filter];
                handler += value;
                _notifyHandlers[filter] = handler;

                var watcher = _changeWatchers[filter];
                if (watcher != null)
                    watcher.Changed += handler;
            }
            remove
            {
                var filter = NotifyFilters.DirectoryName;
                var handler = _notifyHandlers[filter];
                handler -= value;
                _notifyHandlers[filter] = handler;
            }
        }

        public event EventHandler<FileSystemEventArgs> FileNameChanged
        {
            add
            {
                var filter = NotifyFilters.FileName;
                var handler = _notifyHandlers[filter];
                handler += value;
                _notifyHandlers[filter] = handler;

                var watcher = _changeWatchers[filter];
                if (watcher != null)
                    watcher.Changed += handler;
            }
            remove
            {
                var filter = NotifyFilters.FileName;
                var handler = _notifyHandlers[filter];
                handler -= value;
                _notifyHandlers[filter] = handler;
            }
        }

        public event EventHandler<FileSystemEventArgs> LastAccessChanged
        {
            add
            {
                var filter = NotifyFilters.LastAccess;
                var handler = _notifyHandlers[filter];
                handler += value;
                _notifyHandlers[filter] = handler;

                var watcher = _changeWatchers[filter];
                if (watcher != null)
                    watcher.Changed += handler;
            }
            remove
            {
                var filter = NotifyFilters.LastAccess;
                var handler = _notifyHandlers[filter];
                handler -= value;
                _notifyHandlers[filter] = handler;
            }
        }

        public event EventHandler<FileSystemEventArgs> LastWriteChanged
        {
            add
            {
                var filter = NotifyFilters.LastWrite;
                var handler = _notifyHandlers[filter];
                handler += value;
                _notifyHandlers[filter] = handler;

                var watcher = _changeWatchers[filter];
                if (watcher != null)
                    watcher.Changed += handler;
            }
            remove
            {
                var filter = NotifyFilters.LastWrite;
                var handler = _notifyHandlers[filter];
                handler -= value;
                _notifyHandlers[filter] = handler;
            }
        }

        public event EventHandler<FileSystemEventArgs> SecurityChanged
        {
            add
            {
                var filter = NotifyFilters.LastWrite;
                var handler = _notifyHandlers[filter];
                handler += value;
                _notifyHandlers[filter] = handler;

                var watcher = _changeWatchers[filter];
                if (watcher != null)
                    watcher.Changed += handler;
            }
            remove
            {
                var filter = NotifyFilters.LastWrite;
                var handler = _notifyHandlers[filter];
                handler -= value;
                _notifyHandlers[filter] = handler;
            }
        }


        public event EventHandler<FileSystemEventArgs> SizeChanged
        {
            add
            {
                var filter = NotifyFilters.Size;
                var handler = _notifyHandlers[filter];
                handler += value;
                _notifyHandlers[filter] = handler;

                var watcher = _changeWatchers[filter];
                if (watcher != null)
                    watcher.Changed += handler;
            }
            remove
            {
                var filter = NotifyFilters.Size;
                var handler = _notifyHandlers[filter];
                handler -= value;
                _notifyHandlers[filter] = handler;
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
                _watcher.Error -= OnError;
                _onError -= value;
            }
        }

        public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType) => _watcher.WaitForChanged(changeType);

        public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout) => _watcher.WaitForChanged(changeType, timeout);

        public void Dispose() => Dispose(true);

        protected void RaiseBufferedFileSystemEvents()
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
                }
            }
        }

        protected void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (!_buffer.TryAdd(e))
                OnBufferExceeded();
        }

        protected void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (!_buffer.TryAdd(e))
                OnBufferExceeded();
        }

        protected void OnDeleted(object sender, FileSystemEventArgs e)
        {
            if (!_buffer.TryAdd(e))
                OnBufferExceeded();
        }

        protected void OnRenamed(object sender, RenamedEventArgs e)
        {
            if (!_buffer.TryAdd(e))
                OnBufferExceeded();
        }

        protected void OnError(object sender, ErrorEventArgs e)
        {
            _onError?.Invoke(this, e);
        }

        protected void UpdateFilterWatchers()
        {
            var existingFilters = new HashSet<NotifyFilters>(_changeWatchers.Keys);
            var newFilters = new HashSet<NotifyFilters>(_watcher.NotifyFilter.GetFlags());

            // remove anything not currently part of the filter set
            var extraFilters = existingFilters.Except(newFilters);
            foreach (var filter in extraFilters)
            {
                var watcher = _changeWatchers[filter];
                watcher.Dispose();
                _changeWatchers.Remove(filter);
            }

            var missingFilters = newFilters.Except(existingFilters);
            foreach (var filter in missingFilters)
            {
                var watcher = new FileSystemWatcher(_watcher.Path, _watcher.Filter)
                {
                    NotifyFilter = filter,
                    IncludeSubdirectories = IncludeSubdirectories
                };
                var handler = _notifyHandlers[filter];
                if (handler != null)
                    watcher.Changed += handler.Invoke;

                if (EnableRaisingEvents)
                    watcher.EnableRaisingEvents = true;

                _changeWatchers[filter] = new FileSystemWatcherAdapter(watcher);
            }
        }

        protected void OnBufferExceeded()
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
            foreach (var changeWatcher in _changeWatchers.Values)
                changeWatcher.Dispose();

            _onCreated = null;
            _onChanged = null;
            _onDeleted = null;
            _onRenamed = null;
            _onError = null;

            foreach (var filter in _notifyHandlers.Keys)
                _notifyHandlers[filter] = null;

            //_onAttributeChanged = null;
            //_onCreationTimeChanged = null;
            //_onDirectoryNameChanged = null;
            //_onFileNameChanged = null;
            //_onLastAccessChanged = null;
            //_onLastWriteChanged = null;
            //_onSecurityChanged = null;
            //_onSizeChanged = null;

            _disposed = true;
        }

        private bool _disposed;
        private CancellationTokenSource _cts;

        private EventHandler<FileSystemEventArgs> _onCreated;
        private EventHandler<FileSystemEventArgs> _onChanged;
        private EventHandler<FileSystemEventArgs> _onDeleted;
        private EventHandler<RenamedEventArgs> _onRenamed;
        private EventHandler<ErrorEventArgs> _onError;

        // now for the specific change events
        private readonly IDictionary<NotifyFilters, EventHandler<FileSystemEventArgs>> _notifyHandlers = new Dictionary<NotifyFilters, EventHandler<FileSystemEventArgs>>
        {
            [NotifyFilters.Attributes] = null,
            [NotifyFilters.CreationTime] = null,
            [NotifyFilters.DirectoryName] = null,
            [NotifyFilters.FileName] = null,
            [NotifyFilters.LastAccess] = null,
            [NotifyFilters.LastWrite] = null,
            [NotifyFilters.Security] = null,
            [NotifyFilters.Size] = null
        };
        //private EventHandler<FileSystemEventArgs> _onAttributeChanged;
        //private EventHandler<FileSystemEventArgs> _onCreationTimeChanged;
        //private EventHandler<FileSystemEventArgs> _onDirectoryNameChanged;
        //private EventHandler<FileSystemEventArgs> _onFileNameChanged;
        //private EventHandler<FileSystemEventArgs> _onLastAccessChanged;
        //private EventHandler<FileSystemEventArgs> _onLastWriteChanged;
        //private EventHandler<FileSystemEventArgs> _onSecurityChanged;
        //private EventHandler<FileSystemEventArgs> _onSizeChanged;

        private readonly IFileSystemWatcher _watcher;
        private readonly BlockingCollection<FileSystemEventArgs> _buffer;
        private readonly IDictionary<NotifyFilters, IFileSystemWatcher> _changeWatchers = new Dictionary<NotifyFilters, IFileSystemWatcher>();
    }
}
