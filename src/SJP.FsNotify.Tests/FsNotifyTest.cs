using System;
using System.IO;

namespace SJP.FsNotify.Tests
{
    public abstract class FsNotifyTest
    {
        protected virtual DirectoryInfo GetTestDirectory()
        {
            var tempPath = Path.GetTempPath();
            var tempDirName = Path.GetRandomFileName();

            var tempDirPath = Path.Combine(tempPath, tempDirName);
            return Directory.CreateDirectory(tempDirPath);
        }

        protected virtual FileInfo GetTestFile(DirectoryInfo testDir, string extension = "")
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
