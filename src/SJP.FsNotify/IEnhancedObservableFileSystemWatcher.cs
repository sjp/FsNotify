using System;
using System.IO;

namespace SJP.FsNotify
{
    public interface IEnhancedObservableFileSystemWatcher : IObservableFileSystemWatcher
    {
        IObservable<FileSystemEventArgs> CreationTimeChanged { get; }

        IObservable<FileSystemEventArgs> DirectoryNameChanged { get; }

        IObservable<FileSystemEventArgs> FileNameChanged { get; }

        IObservable<FileSystemEventArgs> LastAccessChanged { get; }

        IObservable<FileSystemEventArgs> LastWriteChanged { get; }

        IObservable<FileSystemEventArgs> SecurityChanged { get; }

        IObservable<FileSystemEventArgs> SizeChanged { get; }
    }
}
