using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.FsNotify.Tests
{
    [TestFixture]
    internal static class EnhancedFileSystemWatcherTests
    {
        [Test]
        public static void Ctor_GivenNullFileSystemWatcher_ThrowsArgNullException()
        {
            FileSystemWatcher watcher = null;
            Assert.Throws<ArgumentNullException>(() => new EnhancedFileSystemWatcher(watcher));
        }

        [Test]
        public static void Ctor_GivenNullIFileSystemWatcher_ThrowsArgNullException()
        {
            IFileSystemWatcher watcher = null;
            Assert.Throws<ArgumentNullException>(() => new EnhancedFileSystemWatcher(watcher));
        }

        [Test]
        public static void Ctor_GivenNullDirectoryInfo_ThrowsArgNullException()
        {
            DirectoryInfo dir = null;
            Assert.Throws<ArgumentNullException>(() => new EnhancedFileSystemWatcher(dir));
        }

        [Test]
        public static void Ctor_GivenNullDirectoryInfoAndValidFilter_ThrowsArgNullException()
        {
            DirectoryInfo dir = null;
            const string filter = "*.*";
            Assert.Throws<ArgumentNullException>(() => new EnhancedFileSystemWatcher(dir, filter));
        }

        [Test]
        public static void Ctor_GivenNullPath_ThrowsArgNullException()
        {
            const string path = null;
            Assert.Throws<ArgumentNullException>(() => new EnhancedFileSystemWatcher(path));
        }

        [Test]
        public static void Ctor_GivenNullPathAndValidFilter_ThrowsArgNullException()
        {
            const string path = null;
            const string filter = "*.*";
            Assert.Throws<ArgumentNullException>(() => new EnhancedFileSystemWatcher(path, filter));
        }

        [Test]
        public static void Ctor_GivenWatcherWithoutPathSet_ThrowsArgNullException()
        {
            var watcher = new FileSystemWatcher();
            Assert.Throws<ArgumentException>(() => new EnhancedFileSystemWatcher(watcher));
        }

        [Test]
        public static void Ctor_GivenWatcherWithBadCapacity_ThrowsArgOutOfRangeException()
        {
            var watcher = new FileSystemWatcher();
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnhancedFileSystemWatcher(watcher, 0));
        }

        [Test]
        public static void NotifyFilters_PropertyGet_HasAllFlags()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                testDir.Create();
                var enhancedWatcher = new EnhancedFileSystemWatcher(testDir.FullName);
                const NotifyFilters allFilters = NotifyFilters.Attributes
                    | NotifyFilters.CreationTime
                    | NotifyFilters.DirectoryName
                    | NotifyFilters.FileName
                    | NotifyFilters.LastAccess
                    | NotifyFilters.LastWrite
                    | NotifyFilters.Security
                    | NotifyFilters.Size;
                Assert.AreEqual(allFilters, enhancedWatcher.NotifyFilter);
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static void Path_WhenSetInCtor_RetrievedFromPropertyUnchanged()
        {
            var testDir = FsNotifyTest.GetTestDirectory();

            var watcher = new FileSystemWatcher(testDir.FullName);
            using (var enhancedWatcher = new EnhancedFileSystemWatcher(watcher))
            {
                Assert.Multiple(() =>
                {
                    Assert.AreEqual(testDir.FullName, enhancedWatcher.Path);
                    Assert.AreEqual(testDir.FullName, watcher.Path);
                });
            }
        }

        [Test]
        public static void Path_WhenSetInProperty_RetrievedFromPropertyCorrectly()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            var testDir2 = FsNotifyTest.GetTestDirectory();

            var watcher = new FileSystemWatcher(testDir.FullName);
            using (var enhancedWatcher = new EnhancedFileSystemWatcher(watcher))
            {
                enhancedWatcher.Path = testDir2.FullName;

                Assert.Multiple(() =>
                {
                    Assert.AreEqual(testDir2.FullName, enhancedWatcher.Path);
                    Assert.AreEqual(testDir2.FullName, watcher.Path);
                });
            }
        }

        [Test]
        public static void IncludeSubdirectories_WhenSetInCtor_RetrievedFromPropertyUnchanged()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                var watcher = new FileSystemWatcher(testDir.FullName) { IncludeSubdirectories = true };
                using (var enhancedWatcher = new EnhancedFileSystemWatcher(watcher))
                {
                    Assert.Multiple(() =>
                    {
                        Assert.IsTrue(watcher.IncludeSubdirectories);
                        Assert.IsTrue(enhancedWatcher.IncludeSubdirectories);
                    });
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static void IncludeSubdirectories_WhenSetInProperty_RetrievedFromPropertyCorrectly()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new EnhancedFileSystemWatcher(watcher))
                {
                    enhancedWatcher.IncludeSubdirectories = true;

                    Assert.Multiple(() =>
                    {
                        Assert.IsTrue(watcher.IncludeSubdirectories);
                        Assert.IsTrue(enhancedWatcher.IncludeSubdirectories);
                    });
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static void Filter_WhenSetInCtor_RetrievedFromPropertyUnchanged()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                const string filter = "*.exe";
                var watcher = new FileSystemWatcher(testDir.FullName) { Filter = filter };
                using (var enhancedWatcher = new EnhancedFileSystemWatcher(watcher))
                {
                    Assert.Multiple(() =>
                    {
                        Assert.AreEqual(filter, watcher.Filter);
                        Assert.AreEqual(filter, enhancedWatcher.Filter);
                    });
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static void Filter_WhenSetInProperty_RetrievedFromPropertyCorrectly()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                const string filter = "*.exe";
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new EnhancedFileSystemWatcher(watcher))
                {
                    enhancedWatcher.Filter = filter;

                    Assert.Multiple(() =>
                    {
                        Assert.AreEqual(filter, watcher.Filter);
                        Assert.AreEqual(filter, enhancedWatcher.Filter);
                    });
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static void NotifyFilter_WhenSetInProperty_RetrievedFromPropertyCorrectly()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                const NotifyFilters filter = NotifyFilters.Security | NotifyFilters.Attributes;
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new EnhancedFileSystemWatcher(watcher))
                {
                    enhancedWatcher.NotifyFilter = filter;

                    Assert.Multiple(() =>
                    {
                        Assert.AreEqual(filter, watcher.NotifyFilter);
                        Assert.AreEqual(filter, enhancedWatcher.NotifyFilter);
                    });
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static void EnableRaisingEvents_WhenSetInCtor_RetrievedFromPropertyUnchanged()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName) { EnableRaisingEvents = true };
                using (var enhancedWatcher = new EnhancedFileSystemWatcher(watcher))
                {
                    Assert.Multiple(() =>
                    {
                        Assert.IsTrue(watcher.EnableRaisingEvents);
                        Assert.IsTrue(enhancedWatcher.EnableRaisingEvents);
                    });
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static void EnableRaisingEvents_WhenSetInProperty_RetrievedFromPropertyCorrectly()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new EnhancedFileSystemWatcher(watcher))
                {
                    enhancedWatcher.EnableRaisingEvents = true;

                    Assert.Multiple(() =>
                    {
                        Assert.IsTrue(watcher.EnableRaisingEvents);
                        Assert.IsTrue(enhancedWatcher.EnableRaisingEvents);
                    });
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static void NotifyFilters_WhenGivenInvalidEnum_ThrowsArgumentException()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new EnhancedFileSystemWatcher(watcher))
                {
                    const NotifyFilters filter = (NotifyFilters)32908;
                    Assert.Throws<ArgumentException>(() => enhancedWatcher.NotifyFilter = filter);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static void WaitForChanged_WhenGivenInvalidEnum_ThrowsArgumentException()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new EnhancedFileSystemWatcher(watcher))
                {
                    const WatcherChangeTypes changeType = (WatcherChangeTypes)32908;
                    Assert.Throws<ArgumentException>(() => enhancedWatcher.WaitForChanged(changeType));
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static void WaitForChanged_WhenGivenInvalidEnumWithTimeout_ThrowsArgumentException()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new EnhancedFileSystemWatcher(watcher))
                {
                    const WatcherChangeTypes changeType = (WatcherChangeTypes)32908;
                    Assert.Throws<ArgumentException>(() => enhancedWatcher.WaitForChanged(changeType, 123));
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static async Task OnCreated_WhenFileCreatedAndEventBound_CallsMethod()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new FakeEnhancedFileSystemWatcher(watcher))
                {
                    enhancedWatcher.Created += (s, e) => { };
                    enhancedWatcher.EnableRaisingEvents = true;
                    await Task.Delay(100).ConfigureAwait(false);

                    var testFile = FsNotifyTest.GetTestFile(testDir);
                    testFile.Create().Dispose();

                    await Task.Delay(10).ConfigureAwait(false);
                    Assert.IsTrue(enhancedWatcher.OnCreatedCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static async Task OnChanged_WhenFileChangedAndEventBound_CallsMethod()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new FakeEnhancedFileSystemWatcher(watcher))
                {
                    enhancedWatcher.Changed += (s, e) => { };
                    enhancedWatcher.EnableRaisingEvents = true;
                    await Task.Delay(100).ConfigureAwait(false);

                    var testFile = FsNotifyTest.GetTestFile(testDir);
                    testFile.Create().Dispose();

                    using (var writer = testFile.AppendText())
                        await writer.WriteLineAsync("trigger change").ConfigureAwait(false);
                    testFile.LastWriteTime = new DateTime(2016, 1, 1);

                    await Task.Delay(10).ConfigureAwait(false);
                    Assert.IsTrue(enhancedWatcher.OnChangedCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static async Task OnRenamed_WhenFileRenamedAndEventBound_CallsMethod()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new FakeEnhancedFileSystemWatcher(watcher))
                {
                    enhancedWatcher.Renamed += (s, e) => { };
                    enhancedWatcher.EnableRaisingEvents = true;
                    await Task.Delay(100).ConfigureAwait(false);

                    var testFile = FsNotifyTest.GetTestFile(testDir);
                    var testFile2 = FsNotifyTest.GetTestFile(testDir);
                    testFile.Create().Dispose();
                    File.Move(testFile.FullName, testFile2.FullName);

                    await Task.Delay(10).ConfigureAwait(false);
                    Assert.IsTrue(enhancedWatcher.OnRenamedCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static async Task OnDeleted_WhenFileDeletedAndEventBound_CallsMethod()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new FakeEnhancedFileSystemWatcher(watcher))
                {
                    enhancedWatcher.Deleted += (s, e) => { };
                    enhancedWatcher.EnableRaisingEvents = true;
                    await Task.Delay(100).ConfigureAwait(false);

                    var testFile = FsNotifyTest.GetTestFile(testDir);
                    testFile.Create().Dispose();
                    testFile.Delete();

                    await Task.Delay(10).ConfigureAwait(false);
                    Assert.IsTrue(enhancedWatcher.OnDeletedCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static async Task OnError_WhenErrorOccursAndEventBound_CallsMethod()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new ErrorHandlingEnhancedFileSystemWatcher(watcher, 1))
                {
                    enhancedWatcher.Created += async (s, e) => await Task.Delay(100).ConfigureAwait(false);
                    enhancedWatcher.Error += (s, e) => { };
                    enhancedWatcher.EnableRaisingEvents = true;
                    await Task.Delay(100).ConfigureAwait(false);

                    var testFile1 = FsNotifyTest.GetTestFile(testDir);
                    var testFile2 = FsNotifyTest.GetTestFile(testDir);
                    var testFile3 = FsNotifyTest.GetTestFile(testDir);

                    testFile1.Create().Dispose();
                    testFile2.Create().Dispose();
                    testFile3.Create().Dispose();

                    await Task.Delay(1000).ConfigureAwait(false);
                    Assert.IsTrue(enhancedWatcher.OnErrorCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static async Task OnBufferExceeded_WhenBufferExceeded_CallsMethod()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new ErrorHandlingEnhancedFileSystemWatcher(watcher, 1))
                {
                    enhancedWatcher.Created += async (s, e) => await Task.Delay(100).ConfigureAwait(false);
                    enhancedWatcher.EnableRaisingEvents = true;
                    await Task.Delay(100).ConfigureAwait(false);

                    var testFile1 = FsNotifyTest.GetTestFile(testDir);
                    var testFile2 = FsNotifyTest.GetTestFile(testDir);
                    var testFile3 = FsNotifyTest.GetTestFile(testDir);

                    testFile1.Create().Dispose();
                    testFile2.Create().Dispose();
                    testFile3.Create().Dispose();

                    await Task.Delay(1000).ConfigureAwait(false);
                    Assert.IsTrue(enhancedWatcher.OnBufferExceededCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static async Task Create_WhenFileCreatedAndEventBound_RaisesEvent()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new EnhancedFileSystemWatcher(watcher))
                {
                    var createdCalled = false;
                    enhancedWatcher.Created += (s, e) => createdCalled = true;
                    enhancedWatcher.EnableRaisingEvents = true;

                    await Task.Delay(100).ConfigureAwait(false);
                    var testFile = FsNotifyTest.GetTestFile(testDir);
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
        public static async Task Change_WhenFileChangedAndEventBound_RaisesEvent()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new EnhancedFileSystemWatcher(watcher))
                {
                    var changedCalled = false;
                    enhancedWatcher.Changed += (s, e) => changedCalled = true;
                    enhancedWatcher.EnableRaisingEvents = true;

                    await Task.Delay(100).ConfigureAwait(false);
                    var testFile = FsNotifyTest.GetTestFile(testDir);
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
        public static async Task Rename_WhenFileRenamedAndEventBound_RaisesEvent()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new EnhancedFileSystemWatcher(watcher))
                {
                    var renamedCalled = false;
                    enhancedWatcher.Renamed += (s, e) => renamedCalled = true;
                    enhancedWatcher.EnableRaisingEvents = true;

                    await Task.Delay(100).ConfigureAwait(false);
                    var testFile = FsNotifyTest.GetTestFile(testDir);
                    var testFile2 = FsNotifyTest.GetTestFile(testDir);
                    testFile.Create().Dispose();
                    await Task.Delay(10).ConfigureAwait(false);
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
        public static async Task Delete_WhenFileDeletedAndEventBound_RaisesEvent()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new EnhancedFileSystemWatcher(watcher))
                {
                    var deletedCalled = false;
                    enhancedWatcher.Deleted += (s, e) => deletedCalled = true;
                    enhancedWatcher.EnableRaisingEvents = true;

                    await Task.Delay(100).ConfigureAwait(false);
                    var testFile = FsNotifyTest.GetTestFile(testDir);
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

        [Test]
        public static async Task Error_WhenErrorOccursAndEventBound_RaisesEvent()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new EnhancedFileSystemWatcher(watcher, 1))
                {
                    var errorCalled = false;
                    enhancedWatcher.Created += async (s, e) => await Task.Delay(10000).ConfigureAwait(false);
                    enhancedWatcher.Error += (s, e) => errorCalled = true;
                    enhancedWatcher.EnableRaisingEvents = true;

                    await Task.Delay(100).ConfigureAwait(false);
                    var testFile1 = FsNotifyTest.GetTestFile(testDir);
                    await Task.Delay(100).ConfigureAwait(false);

                    var testFile2 = FsNotifyTest.GetTestFile(testDir);
                    await Task.Delay(100).ConfigureAwait(false);

                    var testFile3 = FsNotifyTest.GetTestFile(testDir);
                    await Task.Delay(100).ConfigureAwait(false);

                    testFile1.Create().Dispose();
                    testFile2.Create().Dispose();
                    testFile3.Create().Dispose();

                    await Task.Delay(100).ConfigureAwait(false);
                    Assert.IsTrue(errorCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static async Task OnAttributeChanged_WhenFileAttributeChangedAndEventBound_CallsMethod()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new FakeEnhancedFileSystemWatcher(watcher))
                {
                    enhancedWatcher.AttributeChanged += (s, e) => { };
                    enhancedWatcher.EnableRaisingEvents = true;
                    await Task.Delay(100).ConfigureAwait(false);

                    var testFile = FsNotifyTest.GetTestFile(testDir);
                    testFile.Create().Dispose();
                    await Task.Delay(100).ConfigureAwait(false);
                    testFile.Attributes = FileAttributes.Archive | FileAttributes.Hidden;
                    testFile.Refresh();

                    await Task.Delay(100).ConfigureAwait(false);
                    Assert.IsTrue(enhancedWatcher.OnAttributeChangedCalled);
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
        public static async Task OnCreationTimeChanged_WhenFileCreationChangedAndEventBound_CallsMethod()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new FakeEnhancedFileSystemWatcher(watcher))
                {
                    enhancedWatcher.CreationTimeChanged += (s, e) => { };
                    enhancedWatcher.EnableRaisingEvents = true;

                    var testFile = FsNotifyTest.GetTestFile(testDir);
                    testFile.Create().Dispose();
                    await Task.Delay(100);
                    testFile.CreationTime = new DateTime(2016, 1, 1);
                    testFile.Refresh();

                    await Task.Delay(100);
                    Assert.IsTrue(enhancedWatcher.OnCreationTimeChangedCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static async Task OnLastAccessChanged_WhenFileAccessChangedAndEventBound_CallsMethod()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new FakeEnhancedFileSystemWatcher(watcher))
                {
                    enhancedWatcher.LastAccessChanged += (s, e) => { };
                    enhancedWatcher.EnableRaisingEvents = true;

                    var testFile = FsNotifyTest.GetTestFile(testDir);
                    testFile.Create().Dispose();
                    await Task.Delay(100);
                    testFile.Refresh();

                    testFile.LastAccessTime = new DateTime(2016, 1, 1);
                    testFile.Refresh();

                    await Task.Delay(100);
                    Assert.IsTrue(enhancedWatcher.OnLastAccessChangedCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static async Task OnLastWriteChanged_WhenFileLastWriteChangedAndEventBound_CallsMethod()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new FakeEnhancedFileSystemWatcher(watcher))
                {
                    enhancedWatcher.LastWriteChanged += (s, e) => { };
                    enhancedWatcher.EnableRaisingEvents = true;

                    var testFile = FsNotifyTest.GetTestFile(testDir);
                    testFile.Create().Dispose();
                    await Task.Delay(100);
                    testFile.LastWriteTime = new DateTime(2016, 1, 1);
                    testFile.Refresh();

                    await Task.Delay(100);
                    Assert.IsTrue(enhancedWatcher.OnLastWriteChangedCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static async Task OnSizeChanged_WhenSizeChangedAndEventBound_CallsMethod()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new FakeEnhancedFileSystemWatcher(watcher))
                {
                    enhancedWatcher.SizeChanged += (s, e) => { };
                    enhancedWatcher.EnableRaisingEvents = true;

                    var testFile = FsNotifyTest.GetTestFile(testDir);
                    testFile.Create().Dispose();
                    await Task.Delay(100);
                    using (var writer = new StreamWriter(testFile.OpenWrite()))
                        writer.Write("extra text");

                    await Task.Delay(100);
                    Assert.IsTrue(enhancedWatcher.OnSizeChangedCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static async Task CreationTimeChanged_WhenFileCreationChangedAndEventBound_RaisesEvent()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new EnhancedFileSystemWatcher(watcher))
                {
                    var creationTimeChanged = false;
                    enhancedWatcher.CreationTimeChanged += (s, e) => creationTimeChanged = true;
                    enhancedWatcher.EnableRaisingEvents = true;

                    var testFile = FsNotifyTest.GetTestFile(testDir);
                    testFile.Create().Dispose();
                    await Task.Delay(100);
                    testFile.CreationTime = new DateTime(2016, 1, 1);
                    testFile.Refresh();

                    await Task.Delay(100);
                    Assert.IsTrue(creationTimeChanged);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static async Task LastAccessChanged_WhenFileAccessChangedAndEventBound_RaisesEvent()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new EnhancedFileSystemWatcher(watcher))
                {
                    var lastAccessChanged = false;
                    enhancedWatcher.LastAccessChanged += (s, e) => lastAccessChanged = true;
                    enhancedWatcher.EnableRaisingEvents = true;

                    var testFile = FsNotifyTest.GetTestFile(testDir);
                    testFile.Create().Dispose();
                    await Task.Delay(100);
                    testFile.Refresh();

                    testFile.LastAccessTime = new DateTime(2016, 1, 1);
                    testFile.Refresh();

                    await Task.Delay(100);
                    Assert.IsTrue(lastAccessChanged);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static async Task LastWriteChanged_WhenFileLastWriteChangedAndEventBound_RaisesEvent()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new EnhancedFileSystemWatcher(watcher))
                {
                    var lastWriteChanged = false;
                    enhancedWatcher.LastWriteChanged += (s, e) => lastWriteChanged = true;
                    enhancedWatcher.EnableRaisingEvents = true;

                    var testFile = FsNotifyTest.GetTestFile(testDir);
                    testFile.Create().Dispose();
                    await Task.Delay(100);
                    testFile.LastWriteTime = new DateTime(2016, 1, 1);
                    testFile.Refresh();

                    await Task.Delay(100);
                    Assert.IsTrue(lastWriteChanged);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public static async Task SizeChanged_WhenSizeChangedAndEventBound_RaisesEvent()
        {
            var testDir = FsNotifyTest.GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var enhancedWatcher = new EnhancedFileSystemWatcher(watcher))
                {
                    var sizeChanged = false;
                    enhancedWatcher.SizeChanged += (s, e) => sizeChanged = true;
                    enhancedWatcher.EnableRaisingEvents = true;

                    var testFile = FsNotifyTest.GetTestFile(testDir);
                    testFile.Create().Dispose();
                    await Task.Delay(100);
                    using (var writer = new StreamWriter(testFile.OpenWrite()))
                        writer.Write("extra text");

                    await Task.Delay(100);
                    Assert.IsTrue(sizeChanged);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }
        */

        private class FakeEnhancedFileSystemWatcher : EnhancedFileSystemWatcher
        {
            public FakeEnhancedFileSystemWatcher(FileSystemWatcherAdapter watcher, int capacity = int.MaxValue)
                : this(watcher as IFileSystemWatcher, capacity)
            {
            }

            public FakeEnhancedFileSystemWatcher(IFileSystemWatcher watcher, int capacity = int.MaxValue)
                : base(watcher, capacity)
            {
            }

            public bool OnCreatedCalled => _onCreatedCalled;

            public bool OnChangedCalled => _onChangedCalled;

            public bool OnRenamedCalled => _onRenamedCalled;

            public bool OnDeletedCalled => _onDeletedCalled;

            public bool OnAttributeChangedCalled => _onAttributeChangedCalled;

            public bool OnCreationTimeChangedCalled => _onCreationTimeChangedCalled;

            public bool OnLastAccessChangedCalled => _onLastAccessChangedCalled;

            public bool OnLastWriteChangedCalled => _onLastWriteChangedCalled;

            public bool OnSecurityChangedCalled => _onSecurityChangedCalled;

            public bool OnSizeChangedCalled => _onSizeChangedCalled;

            protected override void OnCreated(FileSystemEventArgs e) => _onCreatedCalled = true;

            protected override void OnChanged(FileSystemEventArgs e) => _onChangedCalled = true;

            protected override void OnRenamed(RenamedEventArgs e) => _onRenamedCalled = true;

            protected override void OnDeleted(FileSystemEventArgs e) => _onDeletedCalled = true;

            protected override void OnAttributeChanged(FileSystemEventArgs e) => _onAttributeChangedCalled = true;

            protected override void OnCreationTimeChanged(FileSystemEventArgs e) => _onCreationTimeChangedCalled = true;

            protected override void OnLastAccessChanged(FileSystemEventArgs e) => _onLastAccessChangedCalled = true;

            protected override void OnLastWriteChanged(FileSystemEventArgs e) => _onLastWriteChangedCalled = true;

            protected override void OnSecurityChanged(FileSystemEventArgs e) => _onSecurityChangedCalled = true;

            protected override void OnSizeChanged(FileSystemEventArgs e) => _onSizeChangedCalled = true;

            private bool _onCreatedCalled;
            private bool _onChangedCalled;
            private bool _onRenamedCalled;
            private bool _onDeletedCalled;
            private bool _onAttributeChangedCalled;
            private bool _onCreationTimeChangedCalled;
            private bool _onLastAccessChangedCalled;
            private bool _onLastWriteChangedCalled;
            private bool _onSecurityChangedCalled;
            private bool _onSizeChangedCalled;
        }

        private class ErrorHandlingEnhancedFileSystemWatcher : EnhancedFileSystemWatcher
        {
            public ErrorHandlingEnhancedFileSystemWatcher(FileSystemWatcherAdapter watcher, int capacity = int.MaxValue)
                : this(watcher as IFileSystemWatcher, capacity)
            {
            }

            public ErrorHandlingEnhancedFileSystemWatcher(IFileSystemWatcher watcher, int capacity = int.MaxValue)
                : base(watcher, capacity)
            {
            }

            public bool OnErrorCalled => _onErrorCalled;

            public bool OnBufferExceededCalled => _onBufferExceededCalled;

            protected override void OnError(ErrorEventArgs e) => _onErrorCalled = true;

            protected override void OnBufferExceeded()
            {
                _onBufferExceededCalled = true;
                OnError(new ErrorEventArgs(new Exception()));
            }

            private bool _onErrorCalled;
            private bool _onBufferExceededCalled;
        }
    }
}
