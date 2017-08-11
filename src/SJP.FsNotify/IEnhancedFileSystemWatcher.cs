using System;
using System.IO;

namespace SJP.FsNotify
{
    public interface IEnhancedFileSystemWatcher : IFileSystemWatcher
    {
        event EventHandler<FileSystemEventArgs> AttributeChanged;

        event EventHandler<FileSystemEventArgs> CreationTimeChanged;

        event EventHandler<FileSystemEventArgs> LastAccessChanged;

        event EventHandler<FileSystemEventArgs> LastWriteChanged;

        event EventHandler<FileSystemEventArgs> SecurityChanged;

        event EventHandler<FileSystemEventArgs> SizeChanged;
    }
}
