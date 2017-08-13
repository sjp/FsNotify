using System;
using System.IO;

namespace SJP.FsNotify
{
    /// <summary>
    /// Represents a file system watcher which provides more events than the standard <see cref="FileSystemWatcher"/> does.
    /// </summary>
    public interface IEnhancedFileSystemWatcher : IFileSystemWatcher
    {
        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="IFileSystemWatcher.Path"/> has one or more attributes changed.
        /// </summary>
        event EventHandler<FileSystemEventArgs> AttributeChanged;

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="IFileSystemWatcher.Path"/> has its creation time changed.
        /// </summary>
        event EventHandler<FileSystemEventArgs> CreationTimeChanged;

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="IFileSystemWatcher.Path"/> has its last access time changed.
        /// </summary>
        event EventHandler<FileSystemEventArgs> LastAccessChanged;

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="IFileSystemWatcher.Path"/> has its last write time changed.
        /// </summary>
        event EventHandler<FileSystemEventArgs> LastWriteChanged;

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="IFileSystemWatcher.Path"/> has its security settings changed.
        /// </summary>
        event EventHandler<FileSystemEventArgs> SecurityChanged;

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="IFileSystemWatcher.Path"/> has its size changed.
        /// </summary>
        event EventHandler<FileSystemEventArgs> SizeChanged;
    }
}
