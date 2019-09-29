using System;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
using EnumsNET;
using System.Linq;
using System.Threading.Tasks;

namespace SJP.FsNotify
{
    /// <summary>
    /// Listens to the file system change notifications and raises events when a directory, or file in a directory, changes. Also provides further events in addition to those provided by <see cref="IFileSystemWatcher"/>.
    /// </summary>
    public class EnhancedFileSystemWatcher : IEnhancedFileSystemWatcher
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnhancedFileSystemWatcher"/> class, given the specified directory to monitor.
        /// </summary>
        /// <param name="path">The directory to monitor, in standard or Universal Naming Convention (UNC) notation.</param>
        /// <param name="capacity">The maximum number of file system events to buffer before stopping.</param>
        public EnhancedFileSystemWatcher(string path, int capacity = int.MaxValue)
            : this(new BufferedFileSystemWatcher(path), capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnhancedFileSystemWatcher"/> class, given the specified directory to monitor.
        /// </summary>
        /// <param name="directory">The directory to monitor, in standard or Universal Naming Convention (UNC) notation.</param>
        /// <param name="capacity">The maximum number of file system events to buffer before stopping.</param>
        public EnhancedFileSystemWatcher(DirectoryInfo directory, int capacity = int.MaxValue)
            : this(new BufferedFileSystemWatcher(directory), capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnhancedFileSystemWatcher"/> class, given the specified directory and type of files to monitor.
        /// </summary>
        /// <param name="path">The directory to monitor, in standard or Universal Naming Convention (UNC) notation.</param>
        /// <param name="filter">The type of files to watch. For example, <c>*.txt</c> watches for changes to all text files.</param>
        /// <param name="capacity">The maximum number of file system events to buffer before stopping.</param>
        public EnhancedFileSystemWatcher(string path, string filter, int capacity = int.MaxValue)
            : this(new BufferedFileSystemWatcher(path, filter), capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnhancedFileSystemWatcher"/> class, given the specified directory and type of files to monitor.
        /// </summary>
        /// <param name="directory">The directory to monitor, in standard or Universal Naming Convention (UNC) notation.</param>
        /// <param name="filter">The type of files to watch. For example, <c>*.txt</c> watches for changes to all text files.</param>
        /// <param name="capacity">The maximum number of file system events to buffer before stopping.</param>
        public EnhancedFileSystemWatcher(DirectoryInfo directory, string filter, int capacity = int.MaxValue)
            : this(new BufferedFileSystemWatcher(directory, filter), capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnhancedFileSystemWatcher"/> class, given a <see cref="FileSystemWatcher"/> to derive information from.
        /// </summary>
        /// <param name="watcher">A file system watcher to derive events from. A regular <see cref="FileSystemWatcher"/> can be provided.</param>
        /// <param name="capacity">The maximum number of file system events to buffer before stopping.</param>
        public EnhancedFileSystemWatcher(FileSystemWatcherAdapter watcher, int capacity = int.MaxValue)
            : this(new BufferedFileSystemWatcher(watcher), capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnhancedFileSystemWatcher"/> class, given an <see cref="IFileSystemWatcher"/> to derive information from.
        /// </summary>
        /// <param name="watcher">A file system watcher to derive events from.</param>
        /// <param name="capacity">The maximum number of file system events to buffer before stopping.</param>
        /// <exception cref="ArgumentNullException"><paramref name="watcher"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="watcher"/>'s <c>Path</c> property is <c>null</c> or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than <c>1</c>.</exception>
        public EnhancedFileSystemWatcher(IFileSystemWatcher watcher, int capacity = int.MaxValue)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(capacity), "The bounding capacity must be at least 1. Given: " + capacity.ToString());

            _watcher = watcher ?? throw new ArgumentNullException(nameof(watcher));
            if (string.IsNullOrEmpty(_watcher.Path))
                throw new ArgumentException($"The { nameof(Path) } property must be set.", nameof(watcher));

            _buffer = new BlockingCollection<EnhancedFileSystemEventArgs>(capacity);
            NotifyFilter = FlagEnums.GetAllFlags<NotifyFilters>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="EnhancedFileSystemWatcher"/> is enabled.
        /// </summary>
        public bool EnableRaisingEvents
        {
            get => _watcher.EnableRaisingEvents;
            set
            {
                if (_watcher.EnableRaisingEvents == value)
                    return;

                if (value)
                {
                    _cts = new CancellationTokenSource();
                    RaiseBufferedFileSystemEvents();
                }

                _watcher.EnableRaisingEvents = value;
                var changeWatchers = _changeWatchers.Values.Where(v => v != null);
                foreach (var watcher in changeWatchers)
                    watcher.EnableRaisingEvents = value;
            }
        }

        /// <summary>
        /// Gets or sets the filter string used to determine what files are monitored in a directory.
        /// </summary>
        public string Filter
        {
            get => _watcher.Filter;
            set
            {
                _watcher.Filter = value;
                var changeWatchers = _changeWatchers.Values.Where(v => v != null);
                foreach (var watcher in changeWatchers)
                    watcher.Filter = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether subdirectories within the specified path should be monitored.
        /// </summary>
        public bool IncludeSubdirectories
        {
            get => _watcher.IncludeSubdirectories;
            set
            {
                _watcher.IncludeSubdirectories = value;
                var changeWatchers = _changeWatchers.Values.Where(v => v != null);
                foreach (var watcher in changeWatchers)
                    watcher.IncludeSubdirectories = value;
            }
        }

        /// <summary>
        /// Gets or sets the path of the directory to watch.
        /// </summary>
        public string Path
        {
            get => _watcher.Path;
            set
            {
                _watcher.Path = value;
                var changeWatchers = _changeWatchers.Values.Where(v => v != null);
                foreach (var watcher in changeWatchers)
                    watcher.Path = value;
            }
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
                UpdateFilterWatchers();
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
        /// Occurs when the instance of <see cref="EnhancedFileSystemWatcher"/> is unable to continue monitoring changes.
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
        /// Occurs when a file or directory in the specified <see cref="Path"/> has an attribute change.
        /// </summary>
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
                _onAttributeChanged -= value;
                var watcher = _changeWatchers[NotifyFilters.Attributes];
                if (watcher != null && _onAttributeChanged == null)
                    watcher.Changed -= OnAttributeChanged;
            }
        }

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="Path"/> has its creation time changed.
        /// </summary>
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
                _onCreationTimeChanged -= value;
                var watcher = _changeWatchers[NotifyFilters.CreationTime];
                if (watcher != null && _onCreationTimeChanged == null)
                    watcher.Changed -= OnCreationTimeChanged;
            }
        }

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="Path"/> has its last access time changed.
        /// </summary>
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
                _onLastAccessChanged -= value;
                var watcher = _changeWatchers[NotifyFilters.LastAccess];
                if (watcher != null && _onLastAccessChanged == null)
                    watcher.Changed -= OnLastAccessChanged;
            }
        }

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="Path"/> has its last write time changed.
        /// </summary>
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
                _onLastWriteChanged -= value;
                var watcher = _changeWatchers[NotifyFilters.LastWrite];
                if (watcher != null && _onLastWriteChanged == null)
                    watcher.Changed -= OnLastWriteChanged;
            }
        }

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="Path"/> has its security settings changed.
        /// </summary>
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
                _onSecurityChanged -= value;
                var watcher = _changeWatchers[NotifyFilters.Security];
                if (watcher != null && _onSecurityChanged == null)
                    watcher.Changed -= OnSecurityChanged;
            }
        }

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="Path"/> has its size changed.
        /// </summary>
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
                _onSizeChanged -= value;
                var watcher = _changeWatchers[NotifyFilters.Size];
                if (watcher != null && _onSizeChanged == null)
                    watcher.Changed -= OnSizeChanged;
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
        /// Releases any resources used by the <see cref="EnhancedFileSystemWatcher"/> instance.
        /// </summary>
        public void Dispose() => Dispose(true);

        /// <summary>
        /// When called, begins buffering of file system events in the background.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">A <see cref="FileSystemEvent"/> that is not known to <see cref="EnhancedFileSystemWatcher"/> has been provided to the event buffer.</exception>
        protected void RaiseBufferedFileSystemEvents()
        {
            if (_buffer == null || _cts == null)
                return;

            Task.Run(() =>
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
                            if (fsEvent.EventArgs is RenamedEventArgs renamedArgs)
                                _onRenamed?.Invoke(this, renamedArgs);
                            break;
                        case FileSystemEvent.AttributeChange:
                        case FileSystemEvent.CreationTimeChange:
                        case FileSystemEvent.LastAccessChange:
                        case FileSystemEvent.LastWriteChange:
                        case FileSystemEvent.SecurityChange:
                        case FileSystemEvent.SizeChange:
                            var filter = _eventToFilterMap[fsEvent.Event];
                            var handler = GetNotifyHandler(filter);
                            handler?.Invoke(this, fsEvent.EventArgs);
                            break;
                        default:
                            throw new NotSupportedException($"Unknown or unexpected value for { nameof(FileSystemEvent) }.");
                    }
                }
            });
        }

        private void OnCreated(object sender, FileSystemEventArgs e) => OnCreated(e);

        private void OnChanged(object sender, FileSystemEventArgs e) => OnChanged(e);

        private void OnDeleted(object sender, FileSystemEventArgs e) => OnDeleted(e);

        private void OnRenamed(object sender, RenamedEventArgs e) => OnRenamed(e);

        private void OnError(object sender, ErrorEventArgs e) => OnError(e);

        private void OnAttributeChanged(object sender, FileSystemEventArgs e) => OnAttributeChanged(e);

        private void OnCreationTimeChanged(object sender, FileSystemEventArgs e) => OnCreationTimeChanged(e);

        private void OnLastAccessChanged(object sender, FileSystemEventArgs e) => OnLastAccessChanged(e);

        private void OnLastWriteChanged(object sender, FileSystemEventArgs e) => OnLastWriteChanged(e);

        private void OnSecurityChanged(object sender, FileSystemEventArgs e) => OnSecurityChanged(e);

        private void OnSizeChanged(object sender, FileSystemEventArgs e) => OnSizeChanged(e);

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

            var args = new EnhancedFileSystemEventArgs(FileSystemEvent.Create, e);
            if (!_buffer.TryAdd(args))
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

            var args = new EnhancedFileSystemEventArgs(FileSystemEvent.Change, e);
            if (!_buffer.TryAdd(args))
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

            var args = new EnhancedFileSystemEventArgs(FileSystemEvent.Delete, e);
            if (!_buffer.TryAdd(args))
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

            var args = new EnhancedFileSystemEventArgs(FileSystemEvent.Rename, e);
            if (!_buffer.TryAdd(args))
                OnBufferExceeded();
        }

        /// <summary>
        /// Raises the <see cref="AttributeChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
        protected virtual void OnAttributeChanged(FileSystemEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (_buffer == null)
                return;

            var args = new EnhancedFileSystemEventArgs(FileSystemEvent.AttributeChange, e);
            if (!_buffer.TryAdd(args))
                OnBufferExceeded();
        }

        /// <summary>
        /// Raises the <see cref="CreationTimeChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
        protected virtual void OnCreationTimeChanged(FileSystemEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (_buffer == null)
                return;

            var args = new EnhancedFileSystemEventArgs(FileSystemEvent.CreationTimeChange, e);
            if (!_buffer.TryAdd(args))
                OnBufferExceeded();
        }

        /// <summary>
        /// Raises the <see cref="LastAccessChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
        protected virtual void OnLastAccessChanged(FileSystemEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (_buffer == null)
                return;

            var args = new EnhancedFileSystemEventArgs(FileSystemEvent.LastAccessChange, e);
            if (!_buffer.TryAdd(args))
                OnBufferExceeded();
        }

        /// <summary>
        /// Raises the <see cref="LastWriteChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
        protected virtual void OnLastWriteChanged(FileSystemEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (_buffer == null)
                return;

            var args = new EnhancedFileSystemEventArgs(FileSystemEvent.LastWriteChange, e);
            if (!_buffer.TryAdd(args))
                OnBufferExceeded();
        }

        /// <summary>
        /// Raises the <see cref="SecurityChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
        protected virtual void OnSecurityChanged(FileSystemEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (_buffer == null)
                return;

            var args = new EnhancedFileSystemEventArgs(FileSystemEvent.SecurityChange, e);
            if (!_buffer.TryAdd(args))
                OnBufferExceeded();
        }

        /// <summary>
        /// Raises the <see cref="SizeChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
        protected virtual void OnSizeChanged(FileSystemEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (_buffer == null)
                return;

            var args = new EnhancedFileSystemEventArgs(FileSystemEvent.SizeChange, e);
            if (!_buffer.TryAdd(args))
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
        /// Used to refresh the underlying <see cref="IFileSystemWatcher"/> instances which track specific events. Only required when <see cref="NotifyFilter"/> has changed.
        /// </summary>
        protected void UpdateFilterWatchers()
        {
            var existingFilters = new HashSet<NotifyFilters>(_changeWatchers.Keys);
            var newFilters = new HashSet<NotifyFilters>(_watcher.NotifyFilter.GetFlags().Where(f => !_ignoredNotifyFilterFlags.Contains(f)));

            // remove anything not currently part of the filter set
            var extraFilters = existingFilters.Except(newFilters);
            foreach (var filter in extraFilters)
            {
                var watcher = _changeWatchers[filter];
                watcher.Dispose();
                _changeWatchers.Remove(filter);
            }

            // add anything not currently set
            var missingFilters = newFilters.Except(existingFilters);
            foreach (var filter in missingFilters)
            {
                var watcher = new BufferedFileSystemWatcher(_watcher.Path, _watcher.Filter)
                {
                    NotifyFilter = filter,
                    IncludeSubdirectories = IncludeSubdirectories
                };

                var handler = GetNotifyHandler(filter);
                if (handler != null)
                    watcher.Changed += handler.Invoke;

                if (EnableRaisingEvents)
                    watcher.EnableRaisingEvents = true;

                _changeWatchers[filter] = watcher;
            }
        }

        /// <summary>
        /// Retrieves the <see cref="EventHandler{FileSystemEventArgs}"/> instance which will handle a given event. For <see cref="NotifyFilters.DirectoryName"/> and <see cref="NotifyFilters.FileName"/>, this will always be <c>null</c>.
        /// </summary>
        /// <param name="filter">A single value of the <see cref="NotifyFilters"/> enum, which determines which specific event to track.</param>
        /// <returns>An <see cref="EventHandler{FileSystemEventArgs}"/> instance.</returns>
        /// <exception cref="ArgumentException"><paramref name="filter"/> is an invalid enum.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The value of <paramref name="filter"/> is not known to <see cref="GetNotifyHandler(NotifyFilters)"/>.</exception>
        protected EventHandler<FileSystemEventArgs>? GetNotifyHandler(NotifyFilters filter)
        {
            if (!filter.IsValid())
                throw new ArgumentException($"The { nameof(NotifyFilters) } provided must be a valid enum.", nameof(filter));

            switch (filter)
            {
                case NotifyFilters.Attributes:
                    return _onAttributeChanged;
                case NotifyFilters.CreationTime:
                    return _onCreationTimeChanged;
                case NotifyFilters.LastAccess:
                    return _onLastAccessChanged;
                case NotifyFilters.LastWrite:
                    return _onLastWriteChanged;
                case NotifyFilters.Security:
                    return _onSecurityChanged;
                case NotifyFilters.Size:
                    return _onSizeChanged;
                case NotifyFilters.DirectoryName:
                case NotifyFilters.FileName:
                    return null; // handled by rename event instead
                default:
                    throw new ArgumentOutOfRangeException(nameof(filter));
            }
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
        /// Releases the managed resources used by the <see cref="EnhancedFileSystemWatcher"/>.
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
            foreach (var changeWatcher in _changeWatchers.Values)
                changeWatcher.Dispose();

            _onCreated = null;
            _onChanged = null;
            _onDeleted = null;
            _onRenamed = null;
            _onError = null;

            _onAttributeChanged = null;
            _onCreationTimeChanged = null;
            _onLastAccessChanged = null;
            _onLastWriteChanged = null;
            _onSecurityChanged = null;
            _onSizeChanged = null;

            _disposed = true;
        }

        private bool _disposed;
        private CancellationTokenSource? _cts;

        private EventHandler<FileSystemEventArgs>? _onCreated;
        private EventHandler<FileSystemEventArgs>? _onChanged;
        private EventHandler<FileSystemEventArgs>? _onDeleted;
        private EventHandler<RenamedEventArgs>? _onRenamed;
        private EventHandler<ErrorEventArgs>? _onError;

        private EventHandler<FileSystemEventArgs>? _onAttributeChanged;
        private EventHandler<FileSystemEventArgs>? _onCreationTimeChanged;
        private EventHandler<FileSystemEventArgs>? _onLastAccessChanged;
        private EventHandler<FileSystemEventArgs>? _onLastWriteChanged;
        private EventHandler<FileSystemEventArgs>? _onSecurityChanged;
        private EventHandler<FileSystemEventArgs>? _onSizeChanged;

        private readonly IFileSystemWatcher _watcher;
        private readonly BlockingCollection<EnhancedFileSystemEventArgs>? _buffer;
        private readonly IDictionary<NotifyFilters, IFileSystemWatcher> _changeWatchers = new Dictionary<NotifyFilters, IFileSystemWatcher>();

        private readonly static IEnumerable<NotifyFilters> _ignoredNotifyFilterFlags = new[] { NotifyFilters.DirectoryName, NotifyFilters.FileName };

        private readonly static IReadOnlyDictionary<FileSystemEvent, NotifyFilters> _eventToFilterMap = new Dictionary<FileSystemEvent, NotifyFilters>
        {
            [FileSystemEvent.AttributeChange] = NotifyFilters.Attributes,
            [FileSystemEvent.CreationTimeChange] = NotifyFilters.CreationTime,
            [FileSystemEvent.LastAccessChange] = NotifyFilters.LastAccess,
            [FileSystemEvent.LastWriteChange] = NotifyFilters.LastWrite,
            [FileSystemEvent.SecurityChange] = NotifyFilters.Security,
            [FileSystemEvent.SizeChange] = NotifyFilters.Size
        };

        /// <summary>
        /// A type of file system event.
        /// </summary>
        protected enum FileSystemEvent
        {
            /// <summary>
            /// File or directory creation.
            /// </summary>
            Create,

            /// <summary>
            /// A generic change in a file or directory.
            /// </summary>
            Change,

            /// <summary>
            /// File or directory deletion.
            /// </summary>
            Delete,

            /// <summary>
            /// A rename for a file or directory.
            /// </summary>
            Rename,

            // the following are really just a type of 'Change' event
            /// <summary>
            /// File or directory attribute change.
            /// </summary>
            AttributeChange,

            /// <summary>
            /// File or directory creation time change.
            /// </summary>
            CreationTimeChange,

            /// <summary>
            /// File or directory last access time change.
            /// </summary>
            LastAccessChange,

            /// <summary>
            /// File or directory last write time change.
            /// </summary>
            LastWriteChange,

            /// <summary>
            /// File or directory security settings change.
            /// </summary>
            SecurityChange,

            /// <summary>
            /// File or directory size change.
            /// </summary>
            SizeChange
        }

        /// <summary>
        /// Provides data for enhanced file system events.
        /// </summary>
        protected class EnhancedFileSystemEventArgs : EventArgs
        {
            /// <summary>
            /// Initializes a new instance of the FileSystemEventArgs class.
            /// </summary>
            /// <param name="e">A file system event that has occurred.</param>
            /// <param name="args">Further information about the event from <paramref name="e"/>.</param>
            /// <exception cref="ArgumentException"><paramref name="e"/> is not a valid enum.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="args"/> is <c>null</c>.</exception>
            public EnhancedFileSystemEventArgs(FileSystemEvent e, FileSystemEventArgs args)
            {
                if (!e.IsValid())
                    throw new ArgumentException($"The { nameof(FileSystemEvent) } provided must be a valid enum.", nameof(e));

                Event = e;
                EventArgs = args ?? throw new ArgumentNullException(nameof(args));
            }

            /// <summary>
            /// The file system event that has occurred.
            /// </summary>
            public FileSystemEvent Event { get; }

            /// <summary>
            /// Further information about the file system event, <see cref="Event"/>.
            /// </summary>
            public FileSystemEventArgs EventArgs { get; }
        }
    }
}
