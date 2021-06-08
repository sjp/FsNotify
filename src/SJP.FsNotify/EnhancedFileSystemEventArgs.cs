using System;
using System.IO;

namespace SJP.FsNotify
{
    /// <summary>
    /// asd
    /// </summary>
    public class EnhancedFileSystemEventArgs : FileSystemEventArgs
    {
        /// <summary>
        /// asd
        /// </summary>
        /// <param name="fsChangeType"></param>
        /// <param name="changeType"></param>
        /// <param name="directory"></param>
        /// <param name="name"></param>
        public EnhancedFileSystemEventArgs(FileSystemChangeType fsChangeType, WatcherChangeTypes changeType, string directory, string? name)
            : base(changeType, directory, name)
        {
            ChangeReason = fsChangeType;
        }

        /// <summary>
        /// asd
        /// </summary>
        public FileSystemChangeType ChangeReason { get; set; }
    }

    /// <summary>
    /// asd
    /// </summary>
    public class EnhancedRenamedEventArgs : EnhancedFileSystemEventArgs
    {
        /// <summary>
        /// asd
        /// </summary>
        /// <param name="oldFullPath"></param>
        /// <param name="oldName"></param>
        /// <param name="fsChangeType"></param>
        /// <param name="changeType"></param>
        /// <param name="directory"></param>
        /// <param name="name"></param>
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
}
