using System;
using System.IO;

namespace SJP.FsNotify.Tests
{
    internal static class FsNotifyTest
    {
        public static FileInfo GetTestFile(DirectoryInfo testDir, string extension = "")
        {
            if (testDir == null)
                throw new ArgumentNullException(nameof(testDir));

            var fileName = Path.GetRandomFileName();
            if (!string.IsNullOrEmpty(extension))
                fileName = Path.ChangeExtension(fileName, extension);
            var filePath = Path.Combine(testDir.FullName, fileName);

            return new FileInfo(filePath);
        }
    }
}
