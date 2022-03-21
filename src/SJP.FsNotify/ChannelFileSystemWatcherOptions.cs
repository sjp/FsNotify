using System;
using System.Collections.ObjectModel;
using System.IO;
using EnumsNET;

namespace SJP.FsNotify;

/// <summary>
/// Configuration options for a file system watcher.
/// </summary>
public sealed class ChannelFileSystemWatcherOptions : IChannelFileSystemWatcherOptions
{
    /// <summary>
    /// Creates a new instance of <see cref="ChannelFileSystemWatcherOptions"/>, used to configure a <see cref="ChannelFileSystemWatcher"/> instance.
    /// </summary>
    /// <param name="path">The path of the directory to watch.</param>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is <c>null</c> or whitespace.</exception>
    public ChannelFileSystemWatcherOptions(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));

        Path = path;
    }

    /// <summary>
    /// Gets the path of the directory to watch.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets or sets the filter string used to determine what files are monitored in a directory.
    /// </summary>
    public string Filter { get; init; } = "*.*";

    /// <summary>
    /// Gets the collection of all the filters used to determine what files are monitored in a directory.
    /// </summary>
    public Collection<string> Filters { get; init; } = new Collection<string>();

    /// <summary>
    /// Gets or sets a value indicating whether subdirectories within the specified path should be monitored.
    /// </summary>
    public bool IncludeSubdirectories { get; init; }

    /// <summary>
    /// Gets or sets the type of changes to watch for.
    /// </summary>
    /// <exception cref="ArgumentException"><c>value</c> is an invalid enum.</exception>
    public NotifyFilters NotifyFilter
    {
        get => _filters;
        init
        {
            if (!value.IsValid())
                throw new ArgumentException($"The { nameof(NotifyFilters) } provided must be a valid enum.", nameof(value));

            _filters = value;
        }
    }

    private NotifyFilters _filters = DefaultNotifyFilters;

    /// <summary>
    /// Whether an event should be raised whenever an underlying <see cref="FileSystemWatcher"/> detects a file or directory has changed.
    /// </summary>
    public bool ChangedEnabled { get; init; } = true;

    /// <summary>
    /// Whether an event should be raised whenever an underlying <see cref="FileSystemWatcher"/> detects a create operation.
    /// </summary>
    public bool CreatedEnabled { get; init; } = true;

    /// <summary>
    /// Whether an event should be raised whenever an underlying <see cref="FileSystemWatcher"/> detects a deletion.
    /// </summary>
    public bool DeletedEnabled { get; init; } = true;

    /// <summary>
    /// Whether an event should be raised whenever an underlying <see cref="FileSystemWatcher"/> detects a rename.
    /// </summary>
    public bool RenamedEnabled { get; init; } = true;

    /// <summary>
    /// A convenience property that returns default notification filters that <see cref="NotifyFilter"/> uses.
    /// </summary>
    /// <remarks>Not intended to be used except as a reference. However, it can passed directly to <see cref="NotifyFilter"/>, e.g. <c>NotifyFilter = ChannelFileSystemWatcherOptions.DefaultNotifyFilters</c></remarks>
    public static NotifyFilters DefaultNotifyFilters { get; } = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

    /// <summary>
    /// A convenience property to ensure that all possible notification filters for <see cref="NotifyFilter"/> are enabled.
    /// </summary>
    /// <remarks>Intended to be passed directly to <see cref="NotifyFilter"/>, e.g. <c>NotifyFilter = ChannelFileSystemWatcherOptions.AllNotifyFilters</c></remarks>
    public static NotifyFilters AllNotifyFilters { get; } = FlagEnums.GetAllFlags<NotifyFilters>();
}
