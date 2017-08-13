using System;
using System.IO;

namespace SJP.FsNotify
{
    /// <summary>
    /// Defines methods and properties used to reactively observe events on a file system.
    /// </summary>
    public interface IObservableFileSystemWatcher : IDisposable
    {
        /// <summary>
        /// Begins monitoring of file system events.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops monitoring of file system events.
        /// </summary>
        void Stop();

        /// <summary>
        /// Provides a subscription to file system events whenever a file or directory is changed.
        /// </summary>
        IObservable<FileSystemEventArgs> Changed { get; }

        /// <summary>
        /// Provides a subscription to file system events whenever a file or directory is renamed.
        /// </summary>
        IObservable<RenamedEventArgs> Renamed { get; }

        /// <summary>
        /// Provides a subscription to file system events whenever a file or directory is deleted.
        /// </summary>
        IObservable<FileSystemEventArgs> Deleted { get; }

        /// <summary>
        /// Provides a subscription to notifications when the instance of <see cref="IObservableFileSystemWatcher"/> is unable to continue monitoring changes.
        /// </summary>
        IObservable<ErrorEventArgs> Errors { get; }

        /// <summary>
        /// Provides a subscription to file system events whenever a file or directory is created.
        /// </summary>
        IObservable<FileSystemEventArgs> Created { get; }
    }
}
