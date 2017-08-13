using System;
using System.IO;

namespace SJP.FsNotify
{
    /// <summary>
    /// Defines methods and properties used to reactively observe events on a file system in addition to those provided by <see cref="IObservableFileSystemWatcher"/>.
    /// </summary>
    public interface IEnhancedObservableFileSystemWatcher : IObservableFileSystemWatcher
    {
        /// <summary>
        /// Provides a subscription to file system events whenever a file or directory attribute has changed.
        /// </summary>
        IObservable<FileSystemEventArgs> AttributeChanged { get; }

        /// <summary>
        /// Provides a subscription to file system events whenever a file or directory creation time has changed.
        /// </summary>
        IObservable<FileSystemEventArgs> CreationTimeChanged { get; }

        /// <summary>
        /// Provides a subscription to file system events whenever a file or directory last access time has changed.
        /// </summary>
        IObservable<FileSystemEventArgs> LastAccessChanged { get; }

        /// <summary>
        /// Provides a subscription to file system events whenever a file or directory last write time has changed.
        /// </summary>
        IObservable<FileSystemEventArgs> LastWriteChanged { get; }

        /// <summary>
        /// Provides a subscription to file system events whenever file or directory security settings have changed.
        /// </summary>
        IObservable<FileSystemEventArgs> SecurityChanged { get; }

        /// <summary>
        /// Provides a subscription to file system events whenever a file or directory size has changed.
        /// </summary>
        IObservable<FileSystemEventArgs> SizeChanged { get; }
    }
}
