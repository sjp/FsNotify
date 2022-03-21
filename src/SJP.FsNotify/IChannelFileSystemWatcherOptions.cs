using System.Collections.ObjectModel;
using System.IO;

namespace SJP.FsNotify;

/// <summary>
/// Options used to configure file system watch behaviour for a channel-based watcher.
/// </summary>
public interface IChannelFileSystemWatcherOptions
{
    /// <summary>
    /// Whether an event should be raised whenever an underlying <see cref="FileSystemWatcher"/> detects a file or directory has changed.
    /// </summary>
    bool ChangedEnabled { get; init; }

    /// <summary>
    /// Whether an event should be raised whenever an underlying <see cref="FileSystemWatcher"/> detects a create operation.
    /// </summary>
    bool CreatedEnabled { get; init; }

    /// <summary>
    /// Whether an event should be raised whenever an underlying <see cref="FileSystemWatcher"/> detects a rename.
    /// </summary>
    bool DeletedEnabled { get; init; }

    /// <summary>
    /// Gets or sets the filter string used to determine what files are monitored in a directory.
    /// </summary>
    string Filter { get; init; }

    /// <summary>
    /// Gets the collection of all the filters used to determine what files are monitored in a directory.
    /// </summary>
    Collection<string> Filters { get; init; }

    /// <summary>
    /// asd
    /// </summary>
    bool IncludeSubdirectories { get; init; }

    /// <summary>
    /// Gets or sets the type of changes to watch for.
    /// </summary>
    NotifyFilters NotifyFilter { get; init; }

    /// <summary>
    /// Gets the path of the directory to watch.
    /// </summary>
    string Path { get; }

    /// <summary>
    /// Whether an event should be raised whenever an underlying <see cref="FileSystemWatcher"/> detects a rename.
    /// </summary>
    bool RenamedEnabled { get; init; }
}
