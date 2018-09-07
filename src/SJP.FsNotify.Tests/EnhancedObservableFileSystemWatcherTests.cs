using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.FsNotify.Tests
{
    [TestFixture]
    internal sealed class EnhancedObservableFileSystemWatcherTests : FsNotifyTest
    {
        [Test]
        public static void Ctor_GivenNullFileSystemWatcher_ThrowsArgNullException()
        {
            FileSystemWatcher watcher = null;
            Assert.Throws<ArgumentNullException>(() => new EnhancedObservableFileSystemWatcher(watcher));
        }

        [Test]
        public static void Ctor_GivenNullIFileSystemWatcher_ThrowsArgNullException()
        {
            IFileSystemWatcher watcher = null;
            Assert.Throws<ArgumentNullException>(() => new EnhancedObservableFileSystemWatcher(watcher));
        }

        [Test]
        public static void Ctor_GivenNullDirectoryInfo_ThrowsArgNullException()
        {
            DirectoryInfo dir = null;
            Assert.Throws<ArgumentNullException>(() => new EnhancedObservableFileSystemWatcher(dir));
        }

        [Test]
        public static void Ctor_GivenNullDirectoryInfoAndValidFilter_ThrowsArgNullException()
        {
            DirectoryInfo dir = null;
            const string filter = "*.*";
            Assert.Throws<ArgumentNullException>(() => new EnhancedObservableFileSystemWatcher(dir, filter));
        }

        [Test]
        public static void Ctor_GivenNullPath_ThrowsArgNullException()
        {
            const string path = null;
            Assert.Throws<ArgumentNullException>(() => new EnhancedObservableFileSystemWatcher(path));
        }

        [Test]
        public static void Ctor_GivenNullPathAndValidFilter_ThrowsArgNullException()
        {
            const string path = null;
            const string filter = "*.*";
            Assert.Throws<ArgumentNullException>(() => new EnhancedObservableFileSystemWatcher(path, filter));
        }

        [Test]
        public static async Task Created_WhenFileCreated_PublishesCreate()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var obsWatcher = new EnhancedObservableFileSystemWatcher(watcher))
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
                using (var obsWatcher = new EnhancedObservableFileSystemWatcher(watcher))
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
                using (var obsWatcher = new EnhancedObservableFileSystemWatcher(watcher))
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
                using (var obsWatcher = new EnhancedObservableFileSystemWatcher(watcher))
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

        // The following are commented out because they are flaky -- haven't worked out why...
        // They run successfully when debugging, but not in a full test run.
        // Uncomment and debug to test properly.
        /*
        [Test]
        public async Task AttributeChanged_WhenFileAttributeModified_PublishesAttributeChange()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var obsWatcher = new EnhancedObservableFileSystemWatcher(watcher))
                {
                    var attributeChangeCalled = false;
                    obsWatcher.AttributeChanged.Subscribe(e => attributeChangeCalled = true);
                    obsWatcher.Start();
                    await Task.Delay(100);

                    var testFile = GetTestFile(testDir);
                    testFile.Create().Dispose();
                    testFile.Attributes = FileAttributes.Archive | FileAttributes.Normal | FileAttributes.Hidden;

                    await Task.Delay(100);
                    Assert.IsTrue(attributeChangeCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public async Task CreationTimeChanged_WhenFileCreationTimeChanged_PublishesCreationTimeChange()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var obsWatcher = new EnhancedObservableFileSystemWatcher(watcher))
                {
                    var creationTimeChangedCalled = false;
                    obsWatcher.CreationTimeChanged.Subscribe(e => creationTimeChangedCalled = true);
                    obsWatcher.Start();
                    await Task.Delay(100);

                    var testFile = GetTestFile(testDir);
                    testFile.Create().Dispose();

                    using (var writer = testFile.AppendText())
                        writer.WriteLine("trigger change");
                    testFile.CreationTime = new DateTime(2016, 1, 1);
                    testFile.Refresh();

                    await Task.Delay(100);
                    Assert.IsTrue(creationTimeChangedCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public async Task LastAccessChanged_WhenFileAccessTimeChanged_PublishesLastAccessChange()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var obsWatcher = new EnhancedObservableFileSystemWatcher(watcher))
                {
                    var lastAccessChangedCalled = false;
                    obsWatcher.LastAccessChanged.Subscribe(e => lastAccessChangedCalled = true);
                    obsWatcher.Start();
                    await Task.Delay(100);

                    var testFile = GetTestFile(testDir);
                    testFile.Create().Dispose();

                    using (var writer = testFile.AppendText())
                        writer.WriteLine("trigger change");
                    testFile.LastAccessTime = new DateTime(2016, 1, 1);
                    testFile.Refresh();

                    await Task.Delay(100);
                    Assert.IsTrue(lastAccessChangedCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public async Task LastWriteChanged_WhenFileWriteTimeChanged_PublishesLastWriteChange()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var obsWatcher = new EnhancedObservableFileSystemWatcher(watcher))
                {
                    var lastWriteChangedCalled = false;
                    obsWatcher.LastWriteChanged.Subscribe(e => lastWriteChangedCalled = true);
                    obsWatcher.Start();
                    await Task.Delay(100);

                    var testFile = GetTestFile(testDir);
                    testFile.Create().Dispose();

                    using (var writer = testFile.AppendText())
                        writer.WriteLine("trigger change");
                    testFile.LastWriteTime = new DateTime(2016, 1, 1);
                    testFile.Refresh();

                    await Task.Delay(100);
                    Assert.IsTrue(lastWriteChangedCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public async Task SizeChanged_WhenFileSizeChanged_PublishesSizeChange()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var obsWatcher = new EnhancedObservableFileSystemWatcher(watcher))
                {
                    var sizeChangedCalled = false;
                    obsWatcher.SizeChanged.Subscribe(e => sizeChangedCalled = true);
                    obsWatcher.Start();
                    await Task.Delay(100);

                    var testFile = GetTestFile(testDir);
                    testFile.Create().Dispose();

                    using (var writer = testFile.AppendText())
                        writer.WriteLine("trigger change");
                    testFile.LastWriteTime = new DateTime(2016, 1, 1);
                    testFile.Refresh();

                    await Task.Delay(100);
                    Assert.IsTrue(sizeChangedCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }
        */
    }
}
