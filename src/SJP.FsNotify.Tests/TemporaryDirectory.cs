using System;
using System.IO;

namespace SJP.FsNotify.Tests
{
    /// <summary>
    /// A temporary directory that will be deleted once disposed.
    /// </summary>
    /// <seealso cref="IDisposable" />
    public sealed class TemporaryDirectory : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemporaryDirectory"/> class.
        /// </summary>
        public TemporaryDirectory()
        {
            DirectoryPath = GetTempDirectoryPath();
            DirectoryInfo = new DirectoryInfo(DirectoryPath);
            Directory.CreateDirectory(DirectoryPath);
        }

        /// <summary>
        /// The directory path of the temporary directory, always a random location.
        /// </summary>
        /// <value>The directory path.</value>
        public string DirectoryPath { get; }

        /// <summary>
        /// A <see cref="DirectoryInfo"/> instance that refers to the temporary directory, always a random location.
        /// </summary>
        public DirectoryInfo DirectoryInfo { get; }

        private static string GetTempDirectoryPath()
        {
            return Path.Combine(
                Path.GetTempPath(),
                Path.GetRandomFileName()
            );
        }

        /// <summary>
        /// Deletes the temporary directory, including all of its contents.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            if (Directory.Exists(DirectoryPath))
            {
                try
                {
                    Directory.Delete(DirectoryPath, true);
                }
                catch {}
            }

            _disposed = true;
        }

        private bool _disposed;
    }
}
