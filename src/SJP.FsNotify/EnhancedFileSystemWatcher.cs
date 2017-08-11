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

            _buffer = new BlockingCollection<EnhancedFileSystemEventArgs>(capacity);
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
                var watcher = _changeWatchers[NotifyFilters.Attributes];
                if (watcher != null && _onAttributeChanged == null)
                    watcher.Changed += OnAttributeChanged;
                _onAttributeChanged += value;
            }
            remove
            {
                var watcher = _changeWatchers[NotifyFilters.Attributes];
                if (watcher != null)
                    watcher.Changed -= OnAttributeChanged;
                _onAttributeChanged -= value;
            }
        }

        public event EventHandler<FileSystemEventArgs> CreationTimeChanged
        {
            add
            {
                var watcher = _changeWatchers[NotifyFilters.CreationTime];
                if (watcher != null && _onCreationTimeChanged == null)
                    watcher.Changed += OnCreationTimeChanged;
                _onCreationTimeChanged += value;
            }
            remove
            {
                var watcher = _changeWatchers[NotifyFilters.CreationTime];
                if (watcher != null)
                    watcher.Changed -= OnCreationTimeChanged;
                _onCreationTimeChanged -= value;
            }
        }

        public event EventHandler<FileSystemEventArgs> DirectoryNameChanged
        {
            add
            {
                var watcher = _changeWatchers[NotifyFilters.DirectoryName];
                if (watcher != null && _onDirectoryNameChanged == null)
                    watcher.Changed += OnDirectoryNameChanged;
                _onDirectoryNameChanged += value;
            }
            remove
            {
                var watcher = _changeWatchers[NotifyFilters.DirectoryName];
                if (watcher != null)
                    watcher.Changed -= OnDirectoryNameChanged;
                _onDirectoryNameChanged -= value;
            }
        }

        public event EventHandler<FileSystemEventArgs> FileNameChanged
        {
            add
            {
                var watcher = _changeWatchers[NotifyFilters.FileName];
                if (watcher != null && _onFileNameChanged == null)
                    watcher.Changed += OnFileNameChanged;
                _onFileNameChanged += value;
            }
            remove
            {
                var watcher = _changeWatchers[NotifyFilters.FileName];
                if (watcher != null)
                    watcher.Changed -= OnFileNameChanged;
                _onFileNameChanged -= value;
            }
        }

        public event EventHandler<FileSystemEventArgs> LastAccessChanged
        {
            add
            {
                var watcher = _changeWatchers[NotifyFilters.LastAccess];
                if (watcher != null && _onLastAccessChanged == null)
                    watcher.Changed += OnLastAccessChanged;
                _onLastAccessChanged += value;
            }
            remove
            {
                var watcher = _changeWatchers[NotifyFilters.LastAccess];
                if (watcher != null)
                    watcher.Changed -= OnLastAccessChanged;
                _onLastAccessChanged -= value;
            }
        }

        public event EventHandler<FileSystemEventArgs> LastWriteChanged
        {
            add
            {
                var watcher = _changeWatchers[NotifyFilters.LastWrite];
                if (watcher != null && _onLastWriteChanged == null)
                    watcher.Changed += OnLastWriteChanged;
                _onLastWriteChanged += value;
            }
            remove
            {
                var watcher = _changeWatchers[NotifyFilters.LastWrite];
                if (watcher != null)
                    watcher.Changed -= OnLastWriteChanged;
                _onLastWriteChanged -= value;
            }
        }

        public event EventHandler<FileSystemEventArgs> SecurityChanged
        {
            add
            {
                var watcher = _changeWatchers[NotifyFilters.Security];
                if (watcher != null && _onSecurityChanged == null)
                    watcher.Changed += OnSecurityChanged;
                _onSecurityChanged += value;
            }
            remove
            {
                var watcher = _changeWatchers[NotifyFilters.Security];
                if (watcher != null)
                    watcher.Changed -= OnSecurityChanged;
                _onSecurityChanged -= value;
            }
        }


        public event EventHandler<FileSystemEventArgs> SizeChanged
        {
            add
            {
                var watcher = _changeWatchers[NotifyFilters.Size];
                if (watcher != null && _onSizeChanged == null)
                    watcher.Changed += OnSizeChanged;
                _onSizeChanged += value;
            }
            remove
            {
                var watcher = _changeWatchers[NotifyFilters.Size];
                if (watcher != null)
                    watcher.Changed -= OnSizeChanged;
                _onSizeChanged -= value;
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
                switch (fsEvent.Event)
                {
                    case FileSystemEvent.Create:
                        _onCreated?.Invoke(this, fsEvent.EventArgs);
                        break;
                    case FileSystemEvent.Change:
                        _onChanged?.Invoke(this, fsEvent.EventArgs);
                        break;
                    case FileSystemEvent.Delete:
                        _onDeleted?.Invoke(this, fsEvent.EventArgs);
                        break;
                    case FileSystemEvent.Rename:
                        _onRenamed?.Invoke(this, fsEvent.EventArgs as RenamedEventArgs);
                        break;
                    case FileSystemEvent.AttributeChange:
                    case FileSystemEvent.CreationTimeChange:
                    case FileSystemEvent.DirectoryNameChange:
                    case FileSystemEvent.FileNameChange:
                    case FileSystemEvent.LastAccessChange:
                    case FileSystemEvent.LastWriteChange:
                    case FileSystemEvent.SecurityChange:
                    case FileSystemEvent.SizeChange:
                        var filter = _eventToFilterMap[fsEvent.Event];
                        var handler = GetNotifyHandler(filter);
                        handler?.Invoke(this, fsEvent.EventArgs);
                        break;
                }
            }
        }

        protected void OnCreated(object sender, FileSystemEventArgs e)
        {
            var args = new EnhancedFileSystemEventArgs(FileSystemEvent.Create, e);
            if (!_buffer.TryAdd(args))
                OnBufferExceeded();
        }

        protected void OnChanged(object sender, FileSystemEventArgs e)
        {
            var args = new EnhancedFileSystemEventArgs(FileSystemEvent.Change, e);
            if (!_buffer.TryAdd(args))
                OnBufferExceeded();
        }

        protected void OnDeleted(object sender, FileSystemEventArgs e)
        {
            var args = new EnhancedFileSystemEventArgs(FileSystemEvent.Delete, e);
            if (!_buffer.TryAdd(args))
                OnBufferExceeded();
        }

        protected void OnRenamed(object sender, RenamedEventArgs e)
        {
            var args = new EnhancedFileSystemEventArgs(FileSystemEvent.Rename, e);
            if (!_buffer.TryAdd(args))
                OnBufferExceeded();
        }

        protected void OnAttributeChanged(object sender, FileSystemEventArgs e)
        {
            var args = new EnhancedFileSystemEventArgs(FileSystemEvent.AttributeChange, e);
            if (!_buffer.TryAdd(args))
                OnBufferExceeded();

        }

        protected void OnCreationTimeChanged(object sender, FileSystemEventArgs e)
        {
            var args = new EnhancedFileSystemEventArgs(FileSystemEvent.CreationTimeChange, e);
            if (!_buffer.TryAdd(args))
                OnBufferExceeded();
        }

        protected void OnDirectoryNameChanged(object sender, FileSystemEventArgs e)
        {
            var args = new EnhancedFileSystemEventArgs(FileSystemEvent.DirectoryNameChange, e);
            if (!_buffer.TryAdd(args))
                OnBufferExceeded();
        }

        protected void OnFileNameChanged(object sender, FileSystemEventArgs e)
        {
            var args = new EnhancedFileSystemEventArgs(FileSystemEvent.FileNameChange, e);
            if (!_buffer.TryAdd(args))
                OnBufferExceeded();
        }

        protected void OnLastAccessChanged(object sender, FileSystemEventArgs e)
        {
            var args = new EnhancedFileSystemEventArgs(FileSystemEvent.LastAccessChange, e);
            if (!_buffer.TryAdd(args))
                OnBufferExceeded();
        }

        protected void OnLastWriteChanged(object sender, FileSystemEventArgs e)
        {
            var args = new EnhancedFileSystemEventArgs(FileSystemEvent.LastWriteChange, e);
            if (!_buffer.TryAdd(args))
                OnBufferExceeded();
        }

        protected void OnSecurityChanged(object sender, FileSystemEventArgs e)
        {
            var args = new EnhancedFileSystemEventArgs(FileSystemEvent.SecurityChange, e);
            if (!_buffer.TryAdd(args))
                OnBufferExceeded();
        }

        protected void OnSizeChanged(object sender, FileSystemEventArgs e)
        {
            var args = new EnhancedFileSystemEventArgs(FileSystemEvent.SizeChange, e);
            if (!_buffer.TryAdd(args))
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
                var handler = GetNotifyHandler(filter);
                if (handler != null)
                    watcher.Changed += handler.Invoke;

                if (EnableRaisingEvents)
                    watcher.EnableRaisingEvents = true;

                _changeWatchers[filter] = new FileSystemWatcherAdapter(watcher);
            }
        }

        protected EventHandler<FileSystemEventArgs> GetNotifyHandler(NotifyFilters filter)
        {
            switch (filter)
            {
                case NotifyFilters.Attributes:
                    return _onAttributeChanged;
                case NotifyFilters.CreationTime:
                    return _onCreationTimeChanged;
                case NotifyFilters.DirectoryName:
                    return _onDirectoryNameChanged;
                case NotifyFilters.FileName:
                    return _onFileNameChanged;
                case NotifyFilters.LastAccess:
                    return _onLastAccessChanged;
                case NotifyFilters.LastWrite:
                    return _onLastWriteChanged;
                case NotifyFilters.Security:
                    return _onSecurityChanged;
                case NotifyFilters.Size:
                    return _onSizeChanged;
                default:
                    throw new ArgumentOutOfRangeException(nameof(filter));
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

            //foreach (var filter in _notifyHandlers.Keys)
             //   _notifyHandlers[filter] = null;

            _onAttributeChanged = null;
            _onCreationTimeChanged = null;
            _onDirectoryNameChanged = null;
            _onFileNameChanged = null;
            _onLastAccessChanged = null;
            _onLastWriteChanged = null;
            _onSecurityChanged = null;
            _onSizeChanged = null;

            _disposed = true;
        }

        private bool _disposed;
        private CancellationTokenSource _cts;

        private EventHandler<FileSystemEventArgs> _onCreated;
        private EventHandler<FileSystemEventArgs> _onChanged;
        private EventHandler<FileSystemEventArgs> _onDeleted;
        private EventHandler<RenamedEventArgs> _onRenamed;
        private EventHandler<ErrorEventArgs> _onError;

        private readonly static IReadOnlyDictionary<FileSystemEvent, NotifyFilters> _eventToFilterMap = new Dictionary<FileSystemEvent, NotifyFilters>
        {
            [FileSystemEvent.AttributeChange] = NotifyFilters.Attributes,
            [FileSystemEvent.CreationTimeChange] = NotifyFilters.CreationTime,
            [FileSystemEvent.DirectoryNameChange] = NotifyFilters.DirectoryName,
            [FileSystemEvent.FileNameChange] = NotifyFilters.FileName,
            [FileSystemEvent.LastAccessChange] = NotifyFilters.LastAccess,
            [FileSystemEvent.LastWriteChange] = NotifyFilters.LastWrite,
            [FileSystemEvent.SecurityChange] = NotifyFilters.Security,
            [FileSystemEvent.SizeChange] = NotifyFilters.Size
        };

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
        private EventHandler<FileSystemEventArgs> _onAttributeChanged;
        private EventHandler<FileSystemEventArgs> _onCreationTimeChanged;
        private EventHandler<FileSystemEventArgs> _onDirectoryNameChanged;
        private EventHandler<FileSystemEventArgs> _onFileNameChanged;
        private EventHandler<FileSystemEventArgs> _onLastAccessChanged;
        private EventHandler<FileSystemEventArgs> _onLastWriteChanged;
        private EventHandler<FileSystemEventArgs> _onSecurityChanged;
        private EventHandler<FileSystemEventArgs> _onSizeChanged;

        private readonly IFileSystemWatcher _watcher;
        private readonly BlockingCollection<EnhancedFileSystemEventArgs> _buffer;
        private readonly IDictionary<NotifyFilters, IFileSystemWatcher> _changeWatchers = new Dictionary<NotifyFilters, IFileSystemWatcher>();
    }

    public class EnhancedFileSystemEventArgs : EventArgs
    {
        public EnhancedFileSystemEventArgs(FileSystemEvent e, FileSystemEventArgs args)
        {
            Event = e;
            EventArgs = args ?? throw new ArgumentNullException(nameof(args));
        }

        public FileSystemEvent Event { get; }

        public FileSystemEventArgs EventArgs { get; }
    }

    public enum FileSystemEvent
    {
        Create,
        Change,
        Delete,
        Rename,
        // the following are really just a type of 'Change' event
        AttributeChange,
        CreationTimeChange,
        DirectoryNameChange,
        FileNameChange,
        LastAccessChange,
        LastWriteChange,
        SecurityChange,
        SizeChange
    }
}
