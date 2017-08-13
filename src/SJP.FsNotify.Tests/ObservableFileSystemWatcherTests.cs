using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.FsNotify.Tests
{
    [TestFixture]
    public class ObservableFileSystemWatcherTests : FsNotifyTest
    {
        [Test]
        public void Ctor_GivenNullFileSystemWatcher_ThrowsArgNullException()
        {
            FileSystemWatcher watcher = null;
            Assert.Throws<ArgumentNullException>(() => new ObservableFileSystemWatcher(watcher));
        }

        [Test]
        public void Ctor_GivenNullIFileSystemWatcher_ThrowsArgNullException()
        {
            IFileSystemWatcher watcher = null;
            Assert.Throws<ArgumentNullException>(() => new ObservableFileSystemWatcher(watcher));
        }

        [Test]
        public async Task Created_WhenFileCreated_PublishesCreate()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var obsWatcher = new ObservableFileSystemWatcher(watcher))
                {
                    var createdCalled = false;
                    obsWatcher.Created.Subscribe(e => createdCalled = true);
                    obsWatcher.Start();
                    await Task.Delay(100);

                    var testFile = GetTestFile(testDir);
                    testFile.Create().Dispose();

                    await Task.Delay(100);
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
        public async Task Changed_WhenFileChanged_PublishesChange()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var obsWatcher = new ObservableFileSystemWatcher(watcher))
                {
                    var changedCalled = false;
                    obsWatcher.Changed.Subscribe(e => changedCalled = true);
                    obsWatcher.Start();
                    await Task.Delay(100);

                    var testFile = GetTestFile(testDir);
                    testFile.Create().Dispose();

                    using (var writer = testFile.AppendText())
                        writer.WriteLine("trigger change");
                    testFile.LastWriteTime = new DateTime(2016, 1, 1);
                    testFile.Refresh();

                    await Task.Delay(100);
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
        public async Task Renamed_WhenFileRenamed_PublishedRename()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var obsWatcher = new ObservableFileSystemWatcher(watcher))
                {
                    var renamedCalled = false;
                    obsWatcher.Renamed.Subscribe(e => renamedCalled = true);
                    obsWatcher.Start();
                    await Task.Delay(100);

                    var testFile = GetTestFile(testDir);
                    var testFile2 = GetTestFile(testDir);
                    testFile.Create().Dispose();
                    File.Move(testFile.FullName, testFile2.FullName);

                    await Task.Delay(100);
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
        public async Task Deleted_WhenFileDeleted_PublishesDelete()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var obsWatcher = new ObservableFileSystemWatcher(watcher))
                {
                    var deletedCalled = false;
                    obsWatcher.Deleted.Subscribe(e => deletedCalled = true);
                    obsWatcher.Start();
                    await Task.Delay(100);

                    var testFile = GetTestFile(testDir);
                    testFile.Create().Dispose();
                    testFile.Delete();

                    await Task.Delay(100);
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
