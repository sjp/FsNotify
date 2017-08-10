using System;
using System.IO;

namespace SJP.FsNotify
{
    public interface IFileSystemWatcher : IDisposable
    {
        bool EnableRaisingEvents { get; set; }

        string Filter { get; set; }

        bool IncludeSubdirectories { get; set; }

        NotifyFilters NotifyFilter { get; set; }

        string Path { get; set; }

        WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType);

        WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout);

        event EventHandler<FileSystemEventArgs> Changed;

        event EventHandler<FileSystemEventArgs> Created;

        event EventHandler<FileSystemEventArgs> Deleted;

        event EventHandler<ErrorEventArgs> Error;

        event EventHandler<RenamedEventArgs> Renamed;
    }
}
