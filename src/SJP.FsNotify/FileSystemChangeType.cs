namespace SJP.FsNotify;

/// <summary>
/// The possible reasons by which a file or directory has changed.
/// </summary>
public enum FileSystemChangeType
{
    /// <summary>
    /// Invalid value. An error has occurred if this is received.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// File or directory created.
    /// </summary>
    Created = 1,

    /// <summary>
    /// File or directory deleted.
    /// </summary>
    Deleted = 2,

    /// <summary>
    /// File or directory renamed.
    /// </summary>
    Renamed = 3,

    /// <summary>
    /// File or directory attribute change.
    /// </summary>
    AttributeChanged = 4,

    /// <summary>
    /// File or directory creation time change.
    /// </summary>
    CreationTimeChanged = 5,

    /// <summary>
    /// File or directory last access time change.
    /// </summary>
    LastAccessChanged = 6,

    /// <summary>
    /// File or directory last write time change.
    /// </summary>
    LastWriteChanged = 7,

    /// <summary>
    /// File or directory security settings change.
    /// </summary>
    SecurityChanged = 8,

    /// <summary>
    /// File or directory size change.
    /// </summary>
    SizeChanged = 9
}