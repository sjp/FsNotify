using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Channels;

namespace SJP.FsNotify;

/// <summary>
/// Listens to filesystem change notifications and publishes to a channel when a directory, or file in a directory, changes.
/// </summary>
public class EnhancedChannelFileSystemWatcher : IChannelFileSystemWatcher, IDisposable
{
    /// <summary>
    /// Initializes an instance of the <see cref="EnhancedChannelFileSystemWatcher"/> class.
    /// </summary>
    /// <param name="writer">A writer to a channel that will receive filesystem events.</param>
    /// <param name="options">Configuration parameters for writing filesystem events.</param>
    /// <exception cref="ArgumentNullException"><paramref name="writer"/> or <paramref name="options"/> are  <c>null</c>.</exception>
    public EnhancedChannelFileSystemWatcher(ChannelWriter<EnhancedFileSystemEventArgs> writer, IChannelFileSystemWatcherOptions options)
        : this(writer, null, options, new FileSystemWatcherAdapter(new FileSystemWatcher()), ConstructChangeWatchers(options))
    {
    }

    /// <summary>
    /// Initializes an instance of the <see cref="EnhancedChannelFileSystemWatcher"/> class.
    /// </summary>
    /// <param name="writer">A writer to a channel that will receive filesystem events.</param>
    /// <param name="errorWriter">A writer to a channel that will receive error messages produced when filesystem change detection encounters errors.</param>
    /// <param name="options">Configuration parameters for writing filesystem events.</param>
    /// <exception cref="ArgumentNullException"><paramref name="writer"/> or <paramref name="errorWriter"/> or <paramref name="options"/> are <c>null</c>.</exception>
    public EnhancedChannelFileSystemWatcher(
        ChannelWriter<EnhancedFileSystemEventArgs> writer,
        ChannelWriter<ErrorEventArgs> errorWriter,
        IChannelFileSystemWatcherOptions options
    ) : this(writer, errorWriter, options, new FileSystemWatcherAdapter(new FileSystemWatcher()), ConstructChangeWatchers(options))
    {
        ArgumentNullException.ThrowIfNull(errorWriter);
    }

    /// <summary>
    /// Initializes an instance of the <see cref="EnhancedChannelFileSystemWatcher"/> class.
    /// </summary>
    /// <param name="writer">A writer to a channel that will receive filesystem events.</param>
    /// <param name="errorWriter">A writer to a channel that will receive error messages produced when filesystem change detection encounters errors.</param>
    /// <param name="options">Configuration parameters for writing filesystem events.</param>
    /// <param name="watcher">An underlying filesystem watcher instance that will emit events not related to change events.</param>
    /// <param name="changeWatchers">A dictionary that contains watchers that will listen to a specific set of changes.</param>
    /// <exception cref="ArgumentNullException"><paramref name="writer"/>, <paramref name="options"/> or <paramref name="watcher"/> or <paramref name="changeWatchers"/> is <c>null</c>.</exception>
    internal EnhancedChannelFileSystemWatcher(
        ChannelWriter<EnhancedFileSystemEventArgs> writer,
        ChannelWriter<ErrorEventArgs>? errorWriter,
        IChannelFileSystemWatcherOptions options,
        IFileSystemWatcher watcher,
        IReadOnlyDictionary<NotifyFilters, IFileSystemWatcher> changeWatchers
    )
    {
        Writer = writer ?? throw new ArgumentNullException(nameof(writer));
        Options = options ?? throw new ArgumentNullException(nameof(options));
        _watcher = watcher ?? throw new ArgumentNullException(nameof(watcher));
        _changeWatchers = changeWatchers ?? throw new ArgumentNullException(nameof(changeWatchers));

        ErrorWriter = errorWriter;
        if (errorWriter != null)
        {
            _watcher.Error += OnError;
        }

        _watcher.Created += OnCreated;
        _watcher.Renamed += OnRenamed;
        _watcher.Deleted += OnDeleted;

        foreach (var kv in _changeWatchers)
        {
            switch (kv.Key)
            {
                case NotifyFilters.Attributes:
                    kv.Value.Changed += OnAttributesChanged;
                    break;
                case NotifyFilters.Size:
                    kv.Value.Changed += OnSizeChanged;
                    break;
                case NotifyFilters.LastWrite:
                    kv.Value.Changed += OnLastWriteChanged;
                    break;
                case NotifyFilters.LastAccess:
                    kv.Value.Changed += OnLastAccessChanged;
                    break;
                case NotifyFilters.CreationTime:
                    kv.Value.Changed += OnCreationTimeChanged;
                    break;
                case NotifyFilters.Security:
                    kv.Value.Changed += OnSecurityChanged;
                    break;
            }
        }
    }

    /// <summary>
    /// A writer to a channel that will receive filesystem events.
    /// </summary>
    protected ChannelWriter<EnhancedFileSystemEventArgs> Writer { get; }

    /// <summary>
    /// A writer to a channel that will receive error messages produced when filesystem change detection encounters errors.
    /// </summary>
    protected ChannelWriter<ErrorEventArgs>? ErrorWriter { get; }

    /// <summary>
    /// Options used to configure the filesystem watcher.
    /// </summary>
    protected IChannelFileSystemWatcherOptions Options { get; }

    /// <inheritdoc />
    public void Start() => EnableRaisingEvents = true;

    /// <inheritdoc />
    public void Stop() => EnableRaisingEvents = false;

    /// <summary>
    /// Gets or sets a value indicating whether the underlying <see cref="FileSystemWatcher"/> is enabled.
    /// </summary>
    protected bool EnableRaisingEvents
    {
        get => _watcher.EnableRaisingEvents;
        set
        {
            if (_watcher.EnableRaisingEvents == value)
                return;

            if (_completed)
            {
                throw new ArgumentException("Unable to start raising events again. The channels have been completed.", nameof(value));
            }

            if (value)
            {
                _watcher.Path = Options.Path;
                _watcher.IncludeSubdirectories = Options.IncludeSubdirectories;
                _watcher.NotifyFilter = Options.NotifyFilter;
                _watcher.Filter = Options.Filter;
                foreach (var filter in Options.Filters)
                    _watcher.Filters.Add(filter);

                foreach (var watcher in _changeWatchers)
                {
                    watcher.Value.Path = Options.Path;
                    watcher.Value.IncludeSubdirectories = Options.IncludeSubdirectories;
                    watcher.Value.NotifyFilter = watcher.Key;
                    watcher.Value.Filter = Options.Filter;
                    foreach (var filter in Options.Filters)
                        watcher.Value.Filters.Add(filter);
                }
            }

            _watcher.EnableRaisingEvents = value;
            foreach (var watcher in _changeWatchers)
                watcher.Value.EnableRaisingEvents = value;
            if (!value)
            {
                Writer.TryComplete();
                ErrorWriter?.TryComplete();
                _completed = true;
            }
        }
    }

    /// <summary>
    /// Releases any resources used by the <see cref="ChannelFileSystemWatcher"/> instance.
    /// </summary>
    public void Dispose() => Dispose(true);

    private void OnCreated(object? sender, FileSystemEventArgs e) => OnCreated(e);

    private void OnDeleted(object? sender, FileSystemEventArgs e) => OnDeleted(e);

    private void OnRenamed(object? sender, RenamedEventArgs e) => OnRenamed(e);

    private void OnError(object? sender, ErrorEventArgs e) => OnError(e);

    private void OnAttributesChanged(object? sender, FileSystemEventArgs e) => OnAttributesChanged(e);

    private void OnSizeChanged(object? sender, FileSystemEventArgs e) => OnSizeChanged(e);

    private void OnLastWriteChanged(object? sender, FileSystemEventArgs e) => OnLastWriteChanged(e);

    private void OnLastAccessChanged(object? sender, FileSystemEventArgs e) => OnLastAccessChanged(e);

    private void OnCreationTimeChanged(object? sender, FileSystemEventArgs e) => OnCreationTimeChanged(e);

    private void OnSecurityChanged(object? sender, FileSystemEventArgs e) => OnSecurityChanged(e);

    /// <summary>
    /// Raises an event when a file or directory is created.
    /// </summary>
    /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
    /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
    protected virtual void OnCreated(FileSystemEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        if (!Options.CreatedEnabled)
            return;

        var enhancedArgs = new EnhancedFileSystemEventArgs(FileSystemChangeType.Created, WatcherChangeTypes.Created, Options.Path, e.Name);
        Writer.TryWrite(enhancedArgs);
    }

    /// <summary>
    /// Raises an event when a file or directory is deleted.
    /// </summary>
    /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
    /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
    protected virtual void OnDeleted(FileSystemEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        if (!Options.DeletedEnabled)
            return;

        var enhancedArgs = new EnhancedFileSystemEventArgs(FileSystemChangeType.Deleted, WatcherChangeTypes.Deleted, Options.Path, e.Name);
        Writer.TryWrite(enhancedArgs);
    }

    /// <summary>
    /// Raises an event when a file or directory is renamed.
    /// </summary>
    /// <param name="e">A <see cref="RenamedEventArgs"/> that contains the event data.</param>
    /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
    protected virtual void OnRenamed(RenamedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        if (!Options.RenamedEnabled)
            return;

        var enhancedArgs = new EnhancedRenamedEventArgs(e.OldFullPath, e.OldName, FileSystemChangeType.Renamed, WatcherChangeTypes.Renamed, Options.Path, e.Name);
        Writer.TryWrite(enhancedArgs);
    }

    /// <summary>
    /// Raises an event when a file or directory change detection fails.
    /// </summary>
    /// <param name="e">A <see cref="ErrorEventArgs"/> that contains the event data.</param>
    /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
    protected virtual void OnError(ErrorEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        ErrorWriter?.TryWrite(e);
    }

    /// <summary>
    /// Raises an event when a file or directory is changed.
    /// </summary>
    /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
    /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
    protected virtual void OnAttributesChanged(FileSystemEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        if (!Options.ChangedEnabled || !_changeWatchers.ContainsKey(NotifyFilters.Attributes))
            return;

        var enhancedArgs = new EnhancedFileSystemEventArgs(FileSystemChangeType.AttributeChanged, WatcherChangeTypes.Changed, Options.Path, e.Name);
        Writer.TryWrite(enhancedArgs);
    }

    /// <summary>
    /// Raises an event when a file or directory is changed.
    /// </summary>
    /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
    /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
    protected virtual void OnSizeChanged(FileSystemEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        if (!Options.ChangedEnabled || !_changeWatchers.ContainsKey(NotifyFilters.Size))
            return;

        var enhancedArgs = new EnhancedFileSystemEventArgs(FileSystemChangeType.SizeChanged, WatcherChangeTypes.Changed, Options.Path, e.Name);
        Writer.TryWrite(enhancedArgs);
    }

    /// <summary>
    /// Raises an event when a file or directory is changed.
    /// </summary>
    /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
    /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
    protected virtual void OnLastWriteChanged(FileSystemEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        if (!Options.ChangedEnabled || !_changeWatchers.ContainsKey(NotifyFilters.LastWrite))
            return;

        var enhancedArgs = new EnhancedFileSystemEventArgs(FileSystemChangeType.LastWriteChanged, WatcherChangeTypes.Changed, Options.Path, e.Name);
        Writer.TryWrite(enhancedArgs);
    }

    /// <summary>
    /// Raises an event when a file or directory is changed.
    /// </summary>
    /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
    /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
    protected virtual void OnLastAccessChanged(FileSystemEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        if (!Options.ChangedEnabled || !_changeWatchers.ContainsKey(NotifyFilters.LastAccess))
            return;

        var enhancedArgs = new EnhancedFileSystemEventArgs(FileSystemChangeType.LastAccessChanged, WatcherChangeTypes.Changed, Options.Path, e.Name);
        Writer.TryWrite(enhancedArgs);
    }

    /// <summary>
    /// Raises an event when a file or directory is changed.
    /// </summary>
    /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
    /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
    protected virtual void OnCreationTimeChanged(FileSystemEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        if (!Options.ChangedEnabled || !_changeWatchers.ContainsKey(NotifyFilters.CreationTime))
            return;

        var enhancedArgs = new EnhancedFileSystemEventArgs(FileSystemChangeType.CreationTimeChanged, WatcherChangeTypes.Changed, Options.Path, e.Name);
        Writer.TryWrite(enhancedArgs);
    }

    /// <summary>
    /// Raises an event when a file or directory is changed.
    /// </summary>
    /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
    /// <exception cref="ArgumentNullException"><paramref name="e"/> is <c>null</c>.</exception>
    protected virtual void OnSecurityChanged(FileSystemEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        if (!Options.ChangedEnabled || !_changeWatchers.ContainsKey(NotifyFilters.Security))
            return;

        var enhancedArgs = new EnhancedFileSystemEventArgs(FileSystemChangeType.SecurityChanged, WatcherChangeTypes.Changed, Options.Path, e.Name);
        Writer.TryWrite(enhancedArgs);
    }

    /// <summary>
    /// Releases the managed resources used by underlying <see cref="FileSystemWatcher"/>.
    /// </summary>
    /// <param name="disposing"><c>true</c> if managed resources are to be disposed. <c>false</c> will not dispose any resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (!disposing)
            return;

        if (_watcher is IDisposable disposable)
            disposable.Dispose();

        foreach (var watcher in _changeWatchers)
        {
            if (watcher.Value is IDisposable disposableChangeWatcher)
                disposableChangeWatcher.Dispose();
        }

        _disposed = true;
    }

    private static IReadOnlyDictionary<NotifyFilters, IFileSystemWatcher> ConstructChangeWatchers(IChannelFileSystemWatcherOptions options)
    {
        var result = new Dictionary<NotifyFilters, IFileSystemWatcher>();

        foreach (var changeFilter in ChangeFilters)
        {
            if (!options.NotifyFilter.HasFlag(changeFilter))
                continue;
            var watcher = new FileSystemWatcherAdapter(new FileSystemWatcher());
            result[changeFilter] = watcher;
        }

        return result;
    }

    private bool _disposed;
    private bool _completed;

    private readonly IFileSystemWatcher _watcher;

    private readonly IReadOnlyDictionary<NotifyFilters, IFileSystemWatcher> _changeWatchers;

    private static readonly IEnumerable<NotifyFilters> ChangeFilters =
    [
        NotifyFilters.Attributes,
        NotifyFilters.Size,
        NotifyFilters.LastWrite,
        NotifyFilters.LastAccess,
        NotifyFilters.CreationTime,
        NotifyFilters.Security
    ];
}