namespace SJP.FsNotify
{
    public enum FileSystemEvent
    {
        Create,
        Change,
        Delete,
        Rename,
        // the following are really just a type of 'Change' event
        AttributeChange,
        CreationTimeChange,
        LastAccessChange,
        LastWriteChange,
        SecurityChange,
        SizeChange
    }
}
