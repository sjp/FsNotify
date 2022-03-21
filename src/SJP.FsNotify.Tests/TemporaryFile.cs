using System;
using System.IO;

namespace SJP.FsNotify.Tests;

/// <summary>
/// A temporary file that will be deleted once disposed.
/// </summary>
/// <seealso cref="IDisposable" />
public sealed class TemporaryFile : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemporaryFile"/> class.
    /// </summary>
    public TemporaryFile(DirectoryInfo testDir)
    {
        var fileName = Path.GetRandomFileName();
        FilePath = Path.Combine(testDir.FullName, fileName);
        FileInfo = new FileInfo(FilePath);
    }

    /// <summary>
    /// The file path of the temporary file, always a random location.
    /// </summary>
    /// <value>The directory path.</value>
    public string FilePath { get; }

    public FileInfo FileInfo { get; }

    /// <summary>
    /// Deletes the temporary file.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        if (File.Exists(FilePath))
        {
            try
            {
                if (FileInfo.Attributes.HasFlag(FileAttributes.ReadOnly))
                    FileInfo.Attributes &= ~FileAttributes.ReadOnly;
                File.Delete(FilePath);
            }
            catch { }
        }

        _disposed = true;
    }

    private bool _disposed;
}