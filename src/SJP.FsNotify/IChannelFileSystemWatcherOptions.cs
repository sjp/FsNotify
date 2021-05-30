using System.Collections.ObjectModel;
using System.IO;

namespace SJP.FsNotify
{
    /// <summary>
    /// asd
    /// </summary>
    public interface IChannelFileSystemWatcherOptions
    {
        /// <summary>
        /// adsd
        /// </summary>
        bool ChangedEnabled { get; init; }

        /// <summary>
        /// asd
        /// </summary>
        bool CreatedEnabled { get; init; }

        /// <summary>
        /// asd
        /// </summary>
        bool DeletedEnabled { get; init; }

        /// <summary>
        /// asd
        /// </summary>
        string Filter { get; init; }

        /// <summary>
        /// asd
        /// </summary>
        Collection<string> Filters { get; init; }

        /// <summary>
        /// asd
        /// </summary>
        bool IncludeSubdirectories { get; init; }

        /// <summary>
        /// asd
        /// </summary>
        NotifyFilters NotifyFilter { get; init; }

        /// <summary>
        /// asd
        /// </summary>
        string Path { get; }

        /// <summary>
        /// asd
        /// </summary>
        bool RenamedEnabled { get; init; }
    }
}