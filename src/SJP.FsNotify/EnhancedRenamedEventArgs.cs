using System;
using System.IO;

namespace SJP.FsNotify;

/// <summary>
/// Provides data for when an enhanced file system rename event has been raised.
/// </summary>
public class EnhancedRenamedEventArgs : EnhancedFileSystemEventArgs
{
    /// <summary>
    /// Constructs an instance of <see cref="EnhancedRenamedEventArgs"/>.
    /// </summary>
    /// <param name="oldFullPath">The old fully qualified path of the file or directory.</param>
    /// <param name="oldName">The old name of the affected file or directory.</param>
    /// <param name="fsChangeType">One of the <see cref="FileSystemChangeType"/> values.</param>
    /// <param name="changeType">One of the <see cref="WatcherChangeTypes"/> values.</param>
    /// <param name="directory">The directory in which the file or directory has been moved to.</param>
    /// <param name="name">The new name of the affected file or directory.</param>
    public EnhancedRenamedEventArgs(string oldFullPath, string? oldName, FileSystemChangeType fsChangeType, WatcherChangeTypes changeType, string directory, string? name)
        : base(fsChangeType, changeType, directory, name)
    {
        OldFullPath = oldFullPath ?? throw new ArgumentNullException(nameof(oldFullPath));
        OldName = oldName;
    }

    /// <summary>
    /// Gets the previous fully qualified path of the affected file or directory.
    /// </summary>
    /// <returns>The previous fully qualified path of the affected file or directory.</returns>
    public string OldFullPath { get; }

    /// <summary>
    /// Gets the old name of the affected file or directory.
    /// </summary>
    /// <returns>The previous name of the affected file or directory.</returns>
    public string? OldName { get; }
}