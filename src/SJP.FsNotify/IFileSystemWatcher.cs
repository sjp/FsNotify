using System;
using System.IO;

namespace SJP.FsNotify
{
    /// <summary>
    /// Represents a file system watcher whose behavior matches those provided by <see cref="FileSystemWatcher"/>.
    /// </summary>
    public interface IFileSystemWatcher : IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating whether the file system watcher is monitoring file system events.
        /// </summary>
        bool EnableRaisingEvents { get; set; }

        /// <summary>
        /// Gets or sets the filter string used to determine what files are monitored in a directory.
        /// </summary>
        string Filter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether subdirectories within the specified path should be monitored.
        /// </summary>
        bool IncludeSubdirectories { get; set; }

        /// <summary>
        /// Gets or sets the type of changes to watch for.
        /// </summary>
        NotifyFilters NotifyFilter { get; set; }

        /// <summary>
        /// Gets or sets the path of the directory to watch.
        /// </summary>
        string Path { get; set; }

        /// <summary>
        /// A synchronous method that returns a structure that contains specific information on the change that occurred, given the type of change you want to monitor.
        /// </summary>
        /// <param name="changeType">The <see cref="WatcherChangeTypes"/> to watch for.</param>
        /// <returns>A <see cref="WaitForChangedResult"/> that contains specific information on the change that occurred.</returns>
        WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType);

        /// <summary>
        /// A synchronous method that returns a structure that contains specific information on the change that occurred, given the type of change you want to monitor and the time (in milliseconds) to wait before timing out.
        /// </summary>
        /// <param name="changeType">The <see cref="WatcherChangeTypes"/> to watch for.</param>
        /// <param name="timeout">The time (in milliseconds) to wait before timing out.</param>
        /// <returns>A <see cref="WaitForChangedResult"/> that contains specific information on the change that occurred.</returns>
        WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout);

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="Path"/> is changed.
        /// </summary>
        event EventHandler<FileSystemEventArgs> Changed;

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="Path"/> is created.
        /// </summary>
        event EventHandler<FileSystemEventArgs> Created;

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="Path"/> is deleted.
        /// </summary>
        event EventHandler<FileSystemEventArgs> Deleted;

        /// <summary>
        /// Occurs when the instance of <see cref="IFileSystemWatcher"/> is unable to continue monitoring changes.
        /// </summary>
        event EventHandler<ErrorEventArgs> Error;

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="Path"/> is renamed.
        /// </summary>
        event EventHandler<RenamedEventArgs> Renamed;
    }
}
