using System;
using System.IO;

namespace SJP.FsNotify
{
    /// <summary>
    /// Represents a file system watcher whose behavior matches that provided by <see cref="FileSystemWatcher"/>, but writes to channels instead.
    /// </summary>
    public interface IChannelFileSystemWatcher
    {
        /// <summary>
        /// Begin watching for file system events.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops watching for file system events and completes any channels (if previously active).
        /// The channel will be completed and a new instance of <see cref="IChannelFileSystemWatcher"/> is required.
        /// </summary>
        void Stop();
    }
}
