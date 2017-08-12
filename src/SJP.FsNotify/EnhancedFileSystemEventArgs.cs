using System;
using System.IO;
using EnumsNET;

namespace SJP.FsNotify
{
    public class EnhancedFileSystemEventArgs : EventArgs
    {
        public EnhancedFileSystemEventArgs(FileSystemEvent e, FileSystemEventArgs args)
        {
            if (!e.IsValid())
                throw new ArgumentException($"The { nameof(FileSystemEvent) } provided must be a valid enum.", nameof(e));

            Event = e;
            EventArgs = args ?? throw new ArgumentNullException(nameof(args));
        }

        public FileSystemEvent Event { get; }

        public FileSystemEventArgs EventArgs { get; }
    }
}
