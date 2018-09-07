using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.FsNotify.Tests
{
    [TestFixture]
    internal sealed class ObservableFileSystemWatcherTests : FsNotifyTest
    {
        [Test]
        public static void Ctor_GivenNullFileSystemWatcher_ThrowsArgNullException()
        {
            FileSystemWatcher watcher = null;
            Assert.Throws<ArgumentNullException>(() => new ObservableFileSystemWatcher(watcher));
        }

        [Test]
        public static void Ctor_GivenNullIFileSystemWatcher_ThrowsArgNullException()
        {
            IFileSystemWatcher watcher = null;
            Assert.Throws<ArgumentNullException>(() => new ObservableFileSystemWatcher(watcher));
        }

        [Test]
        public static void Ctor_GivenNullDirectoryInfo_ThrowsArgNullException()
        {
            DirectoryInfo dir = null;
            Assert.Throws<ArgumentNullException>(() => new ObservableFileSystemWatcher(dir));
        }

        [Test]
        public static void Ctor_GivenNullDirectoryInfoAndValidFilter_ThrowsArgNullException()
        {
            DirectoryInfo dir = null;
            const string filter = "*.*";
            Assert.Throws<ArgumentNullException>(() => new ObservableFileSystemWatcher(dir, filter));
        }

        [Test]
        public static void Ctor_GivenNullPath_ThrowsArgNullException()
        {
            const string path = null;
            Assert.Throws<ArgumentNullException>(() => new ObservableFileSystemWatcher(path));
        }

        [Test]
        public static void Ctor_GivenNullPathAndValidFilter_ThrowsArgNullException()
        {
            const string path = null;
            const string filter = "*.*";
            Assert.Throws<ArgumentNullException>(() => new ObservableFileSystemWatcher(path, filter));
        }

        [Test]
        public static async Task Created_WhenFileCreated_PublishesCreate()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var obsWatcher = new ObservableFileSystemWatcher(watcher))
                {
                    var createdCalled = false;
                    obsWatcher.Created.Subscribe(_ => createdCalled = true);
                    obsWatcher.Start();
                    await Task.Delay(100).ConfigureAwait(false);

                    var testFile = GetTestFile(testDir);
                    testFile.Create().Dispose();

                    await Task.Delay(100).ConfigureAwait(false);
                    Assert.IsTrue(createdCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static async Task Changed_WhenFileChanged_PublishesChange()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var obsWatcher = new ObservableFileSystemWatcher(watcher))
                {
                    var changedCalled = false;
                    obsWatcher.Changed.Subscribe(_ => changedCalled = true);
                    obsWatcher.Start();
                    await Task.Delay(100).ConfigureAwait(false);

                    var testFile = GetTestFile(testDir);
                    testFile.Create().Dispose();

                    using (var writer = testFile.AppendText())
                        await writer.WriteLineAsync("trigger change").ConfigureAwait(false);
                    testFile.LastWriteTime = new DateTime(2016, 1, 1);
                    testFile.Refresh();

                    await Task.Delay(100).ConfigureAwait(false);
                    Assert.IsTrue(changedCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static async Task Renamed_WhenFileRenamed_PublishedRename()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var obsWatcher = new ObservableFileSystemWatcher(watcher))
                {
                    var renamedCalled = false;
                    obsWatcher.Renamed.Subscribe(_ => renamedCalled = true);
                    obsWatcher.Start();
                    await Task.Delay(100).ConfigureAwait(false);

                    var testFile = GetTestFile(testDir);
                    var testFile2 = GetTestFile(testDir);
                    testFile.Create().Dispose();
                    File.Move(testFile.FullName, testFile2.FullName);

                    await Task.Delay(100).ConfigureAwait(false);
                    Assert.IsTrue(renamedCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static async Task Deleted_WhenFileDeleted_PublishesDelete()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var obsWatcher = new ObservableFileSystemWatcher(watcher))
                {
                    var deletedCalled = false;
                    obsWatcher.Deleted.Subscribe(_ => deletedCalled = true);
                    obsWatcher.Start();
                    await Task.Delay(100).ConfigureAwait(false);

                    var testFile = GetTestFile(testDir);
                    testFile.Create().Dispose();
                    testFile.Delete();

                    await Task.Delay(100).ConfigureAwait(false);
                    Assert.IsTrue(deletedCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }
    }
}
