using System;
using System.IO;

namespace SJP.FsNotify
{
    public interface IEnhancedFileSystemWatcher : IFileSystemWatcher
    {
        event EventHandler<FileSystemEventArgs> AttributeChanged;

        event EventHandler<FileSystemEventArgs> CreationTimeChanged;

        event EventHandler<FileSystemEventArgs> DirectoryNameChanged;

        event EventHandler<FileSystemEventArgs> FileNameChanged;

        event EventHandler<FileSystemEventArgs> LastAccessChanged;

        event EventHandler<FileSystemEventArgs> LastWriteChanged;

        event EventHandler<FileSystemEventArgs> SecurityChanged;

        event EventHandler<FileSystemEventArgs> SizeChanged;
    }
}
