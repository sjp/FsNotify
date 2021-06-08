using System;
using System.IO;

namespace SJP.FsNotify
{
    /// <summary>
    /// Provides data for when an enhanced file system event has been raised.
    /// </summary>
    public class EnhancedFileSystemEventArgs : FileSystemEventArgs
    {
        /// <summary>
        /// Constructs an instance of <see cref="EnhancedFileSystemEventArgs"/>.
        /// </summary>
        /// <param name="fsChangeType">One of the <see cref="FileSystemChangeType"/> values.</param>
        /// <param name="changeType">One of the <see cref="WatcherChangeTypes"/> values.</param>
        /// <param name="directory">The directory in which the file or directory has been affected.</param>
        /// <param name="name">The name of the affected file or directory.</param>
        public EnhancedFileSystemEventArgs(FileSystemChangeType fsChangeType, WatcherChangeTypes changeType, string directory, string? name)
            : base(changeType, directory, name)
        {
            ChangeReason = fsChangeType;
        }

        /// <summary>
        /// The reason that the event has been triggered.
        /// </summary>
        public FileSystemChangeType ChangeReason { get; set; }
    }
}
