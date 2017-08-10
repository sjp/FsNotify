using System;
using System.IO;
using System.Reactive.Linq;

namespace SJP.FsNotify
{
    public interface IObservableFileSystemWatcher : IDisposable
    {
        void Start();

        void Stop();

        IObservable<FileSystemEventArgs> Changed { get; }

        IObservable<RenamedEventArgs> Renamed { get; }

        IObservable<FileSystemEventArgs> Deleted { get; }

        IObservable<ErrorEventArgs> Errors { get; }

        IObservable<FileSystemEventArgs> Created { get; }
    }
}
