using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.FsNotify.Tests
{
    [TestFixture]
    public class BufferedFileSystemWatcherTests : FsNotifyTest
    {
        [Test]
        public void Ctor_GivenNullFileSystemWatcher_ThrowsArgNullException()
        {
            FileSystemWatcher watcher = null;
            Assert.Throws<ArgumentNullException>(() => new BufferedFileSystemWatcher(watcher));
        }

        [Test]
        public void Ctor_GivenNullIFileSystemWatcher_ThrowsArgNullException()
        {
            IFileSystemWatcher watcher = null;
            Assert.Throws<ArgumentNullException>(() => new BufferedFileSystemWatcher(watcher));
        }

        [Test]
        public void Ctor_GivenBadCapacity_ThrowsArgOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BufferedFileSystemWatcher(0));
        }

        [Test]
        public void Ctor_GivenWatcherWithBadCapacity_ThrowsArgOutOfRangeException()
        {
            var watcher = new FileSystemWatcher();
            Assert.Throws<ArgumentOutOfRangeException>(() => new BufferedFileSystemWatcher(watcher, 0));
        }

        [Test]
        public void Ctor_GivenNullDirectoryInfo_ThrowsArgNullException()
        {
            DirectoryInfo dir = null;
            Assert.Throws<ArgumentNullException>(() => new BufferedFileSystemWatcher(dir));
        }

        [Test]
        public void Ctor_GivenNullDirectoryInfoAndValidFilter_ThrowsArgNullException()
        {
            DirectoryInfo dir = null;
            const string filter = "*.*";
            Assert.Throws<ArgumentNullException>(() => new BufferedFileSystemWatcher(dir, filter));
        }

        [Test]
        public void Ctor_GivenNullPath_ThrowsArgNullException()
        {
            string path = null;
            Assert.Throws<ArgumentNullException>(() => new BufferedFileSystemWatcher(path));
        }

        [Test]
        public void Ctor_GivenNullPathAndValidFilter_ThrowsArgNullException()
        {
            string path = null;
            const string filter = "*.*";
            Assert.Throws<ArgumentNullException>(() => new BufferedFileSystemWatcher(path, filter));
        }

        [Test]
        public void Path_WhenSetInCtor_RetrievedFromPropertyUnchanged()
        {
            var testDir = GetTestDirectory();

            var watcher = new FileSystemWatcher(testDir.FullName);
            using (var bufferedWatcher = new BufferedFileSystemWatcher(watcher))
            {
                Assert.Multiple(() =>
                {
                    Assert.AreEqual(testDir.FullName, bufferedWatcher.Path);
                    Assert.AreEqual(testDir.FullName, watcher.Path);
                });
            }
        }

        [Test]
        public void Path_WhenSetInProperty_RetrievedFromPropertyCorrectly()
        {
            var testDir = GetTestDirectory();
            var testDir2 = GetTestDirectory();

            var watcher = new FileSystemWatcher(testDir.FullName);
            using (var bufferedWatcher = new BufferedFileSystemWatcher(watcher))
            {
                bufferedWatcher.Path = testDir2.FullName;

                Assert.Multiple(() =>
                {
                    Assert.AreEqual(testDir2.FullName, bufferedWatcher.Path);
                    Assert.AreEqual(testDir2.FullName, watcher.Path);
                });
            }
        }

        [Test]
        public void IncludeSubdirectories_WhenSetInCtor_RetrievedFromPropertyUnchanged()
        {
            var watcher = new FileSystemWatcher() { IncludeSubdirectories = true };
            using (var bufferedWatcher = new BufferedFileSystemWatcher(watcher))
            {
                Assert.Multiple(() =>
                {
                    Assert.IsTrue(watcher.IncludeSubdirectories);
                    Assert.IsTrue(bufferedWatcher.IncludeSubdirectories);
                });
            }
        }

        [Test]
        public void IncludeSubdirectories_WhenSetInProperty_RetrievedFromPropertyCorrectly()
        {
            var watcher = new FileSystemWatcher();
            using (var bufferedWatcher = new BufferedFileSystemWatcher(watcher))
            {
                bufferedWatcher.IncludeSubdirectories = true;

                Assert.Multiple(() =>
                {
                    Assert.IsTrue(watcher.IncludeSubdirectories);
                    Assert.IsTrue(bufferedWatcher.IncludeSubdirectories);
                });
            }
        }

        [Test]
        public void Filter_WhenSetInCtor_RetrievedFromPropertyUnchanged()
        {
            const string filter = "*.exe";
            var watcher = new FileSystemWatcher() { Filter = filter };
            using (var bufferedWatcher = new BufferedFileSystemWatcher(watcher))
            {
                Assert.Multiple(() =>
                {
                    Assert.AreEqual(filter, watcher.Filter);
                    Assert.AreEqual(filter, bufferedWatcher.Filter);
                });
            }
        }

        [Test]
        public void Filter_WhenSetInProperty_RetrievedFromPropertyCorrectly()
        {
            const string filter = "*.exe";
            var watcher = new FileSystemWatcher();
            using (var bufferedWatcher = new BufferedFileSystemWatcher(watcher))
            {
                bufferedWatcher.Filter = filter;

                Assert.Multiple(() =>
                {
                    Assert.AreEqual(filter, watcher.Filter);
                    Assert.AreEqual(filter, bufferedWatcher.Filter);
                });
            }
        }

        [Test]
        public void NotifyFilter_WhenSetInCtor_RetrievedFromPropertyUnchanged()
        {
            const NotifyFilters filter = NotifyFilters.Security | NotifyFilters.Attributes;
            var watcher = new FileSystemWatcher() { NotifyFilter = filter };
            using (var bufferedWatcher = new BufferedFileSystemWatcher(watcher))
            {
                Assert.Multiple(() =>
                {
                    Assert.AreEqual(filter, watcher.NotifyFilter);
                    Assert.AreEqual(filter, bufferedWatcher.NotifyFilter);
                });
            }
        }

        [Test]
        public void NotifyFilter_WhenSetInProperty_RetrievedFromPropertyCorrectly()
        {
            const NotifyFilters filter = NotifyFilters.Security | NotifyFilters.Attributes;
            var watcher = new FileSystemWatcher();
            using (var bufferedWatcher = new BufferedFileSystemWatcher(watcher))
            {
                bufferedWatcher.NotifyFilter = filter;

                Assert.Multiple(() =>
                {
                    Assert.AreEqual(filter, watcher.NotifyFilter);
                    Assert.AreEqual(filter, bufferedWatcher.NotifyFilter);
                });
            }
        }

        [Test]
        public void EnableRaisingEvents_WhenSetInCtor_RetrievedFromPropertyUnchanged()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName) { EnableRaisingEvents = true };
                using (var bufferedWatcher = new BufferedFileSystemWatcher(watcher))
                {
                    Assert.Multiple(() =>
                    {
                        Assert.IsTrue(watcher.EnableRaisingEvents);
                        Assert.IsTrue(bufferedWatcher.EnableRaisingEvents);
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
        public void EnableRaisingEvents_WhenSetInProperty_RetrievedFromPropertyCorrectly()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var bufferedWatcher = new BufferedFileSystemWatcher(watcher))
                {
                    bufferedWatcher.EnableRaisingEvents = true;

                    Assert.Multiple(() =>
                    {
                        Assert.IsTrue(watcher.EnableRaisingEvents);
                        Assert.IsTrue(bufferedWatcher.EnableRaisingEvents);
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
        public void NotifyFilters_WhenGivenInvalidEnum_ThrowsArgumentException()
        {
            var watcher = new FileSystemWatcher();
            using (var bufferedWatcher = new BufferedFileSystemWatcher(watcher))
            {
                const NotifyFilters filter = (NotifyFilters)32908;
                Assert.Throws<ArgumentException>(() => bufferedWatcher.NotifyFilter = filter);
            }
        }

        [Test]
        public void WaitForChanged_WhenGivenInvalidEnum_ThrowsArgumentException()
        {
            var watcher = new FileSystemWatcher();
            using (var bufferedWatcher = new BufferedFileSystemWatcher(watcher))
            {
                const WatcherChangeTypes changeType = (WatcherChangeTypes)32908;
                Assert.Throws<ArgumentException>(() => bufferedWatcher.WaitForChanged(changeType));
            }
        }

        [Test]
        public void WaitForChanged_WhenGivenInvalidEnumWithTimeout_ThrowsArgumentException()
        {
            var watcher = new FileSystemWatcher();
            using (var bufferedWatcher = new BufferedFileSystemWatcher(watcher))
            {
                const WatcherChangeTypes changeType = (WatcherChangeTypes)32908;
                Assert.Throws<ArgumentException>(() => bufferedWatcher.WaitForChanged(changeType, 123));
            }
        }

        [Test]
        public async Task OnCreated_WhenFileCreatedAndEventBound_CallsMethod()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var bufferedWatcher = new FakeBufferedFileSystemWatcher(watcher))
                {
                    bufferedWatcher.Created += (s, e) => { };
                    bufferedWatcher.EnableRaisingEvents = true;
                    await Task.Delay(100);

                    var testFile = GetTestFile(testDir);
                    testFile.Create().Dispose();

                    await Task.Delay(10);
                    Assert.IsTrue(bufferedWatcher.OnCreatedCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public async Task OnChanged_WhenFileChangedAndEventBound_CallsMethod()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var bufferedWatcher = new FakeBufferedFileSystemWatcher(watcher))
                {
                    bufferedWatcher.Changed += (s, e) => { };
                    bufferedWatcher.EnableRaisingEvents = true;
                    await Task.Delay(100);

                    var testFile = GetTestFile(testDir);
                    testFile.Create().Dispose();

                    using (var writer = testFile.AppendText())
                        writer.WriteLine("trigger change");
                    testFile.LastWriteTime = new DateTime(2016, 1, 1);

                    await Task.Delay(10);
                    Assert.IsTrue(bufferedWatcher.OnChangedCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public async Task OnRenamed_WhenFileRenamedAndEventBound_CallsMethod()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var bufferedWatcher = new FakeBufferedFileSystemWatcher(watcher))
                {
                    bufferedWatcher.Renamed += (s, e) => { };
                    bufferedWatcher.EnableRaisingEvents = true;
                    await Task.Delay(100);

                    var testFile = GetTestFile(testDir);
                    var testFile2 = GetTestFile(testDir);
                    testFile.Create().Dispose();
                    File.Move(testFile.FullName, testFile2.FullName);

                    await Task.Delay(10);
                    Assert.IsTrue(bufferedWatcher.OnRenamedCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public async Task OnDeleted_WhenFileDeletedAndEventBound_CallsMethod()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var bufferedWatcher = new FakeBufferedFileSystemWatcher(watcher))
                {
                    bufferedWatcher.Deleted += (s, e) => { };
                    bufferedWatcher.EnableRaisingEvents = true;
                    await Task.Delay(100);

                    var testFile = GetTestFile(testDir);
                    testFile.Create().Dispose();
                    testFile.Delete();

                    await Task.Delay(10);
                    Assert.IsTrue(bufferedWatcher.OnDeletedCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public async Task OnError_WhenErrorOccursAndEventBound_CallsMethod()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var bufferedWatcher = new ErrorHandlingBufferedFileSystemWatcher(watcher, 1))
                {
                    bufferedWatcher.Created += (s, e) => Task.Delay(100).Wait();
                    bufferedWatcher.Error += (s, e) => {};
                    bufferedWatcher.EnableRaisingEvents = true;
                    await Task.Delay(100);

                    var testFile1 = GetTestFile(testDir);
                    var testFile2 = GetTestFile(testDir);
                    var testFile3 = GetTestFile(testDir);

                    testFile1.Create().Dispose();
                    testFile2.Create().Dispose();
                    testFile3.Create().Dispose();

                    await Task.Delay(10);
                    Assert.IsTrue(bufferedWatcher.OnErrorCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public async Task OnBufferExceeded_WhenBufferExceeded_CallsMethod()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var bufferedWatcher = new ErrorHandlingBufferedFileSystemWatcher(watcher, 1))
                {
                    bufferedWatcher.Created += (s, e) => Task.Delay(100).Wait();
                    bufferedWatcher.EnableRaisingEvents = true;
                    await Task.Delay(100);

                    var testFile1 = GetTestFile(testDir);
                    var testFile2 = GetTestFile(testDir);
                    var testFile3 = GetTestFile(testDir);

                    testFile1.Create().Dispose();
                    testFile2.Create().Dispose();
                    testFile3.Create().Dispose();

                    await Task.Delay(10);
                    Assert.IsTrue(bufferedWatcher.OnBufferExceededCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        [Test]
        public async Task Create_WhenFileCreatedAndEventBound_RaisesEvent()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var bufferedWatcher = new BufferedFileSystemWatcher(watcher))
                {
                    var createdCalled = false;
                    bufferedWatcher.Created += (s, e) => createdCalled = true;
                    bufferedWatcher.EnableRaisingEvents = true;
                    await Task.Delay(100);

                    var testFile = GetTestFile(testDir);
                    testFile.Create().Dispose();

                    await Task.Delay(10);
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
        public async Task Change_WhenFileChangedAndEventBound_RaisesEvent()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var bufferedWatcher = new BufferedFileSystemWatcher(watcher))
                {
                    var changedCalled = false;
                    bufferedWatcher.Changed += (s, e) => changedCalled = true;
                    bufferedWatcher.EnableRaisingEvents = true;
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
        public async Task Rename_WhenFileRenamedAndEventBound_RaisesEvent()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var bufferedWatcher = new BufferedFileSystemWatcher(watcher))
                {
                    var renamedCalled = false;
                    bufferedWatcher.Renamed += (s, e) => renamedCalled = true;
                    bufferedWatcher.EnableRaisingEvents = true;
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
        public async Task Delete_WhenFileDeletedAndEventBound_RaisesEvent()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var bufferedWatcher = new BufferedFileSystemWatcher(watcher))
                {
                    var deletedCalled = false;
                    bufferedWatcher.Deleted += (s, e) => deletedCalled = true;
                    bufferedWatcher.EnableRaisingEvents = true;
                    await Task.Delay(100);

                    var testFile = GetTestFile(testDir);
                    testFile.Create().Dispose();
                    testFile.Delete();

                    await Task.Delay(10);
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
        public async Task Error_WhenErrorOccursAndEventBound_RaisesEvent()
        {
            var testDir = GetTestDirectory();
            try
            {
                testDir.Create();
                var watcher = new FileSystemWatcher(testDir.FullName);
                using (var bufferedWatcher = new BufferedFileSystemWatcher(watcher, 1))
                {
                    var errorCalled = false;
                    bufferedWatcher.Created += (s, e) => Task.Delay(10000).Wait();
                    bufferedWatcher.Error += (s, e) => errorCalled = true;
                    bufferedWatcher.EnableRaisingEvents = true;
                    await Task.Delay(100);

                    var testFile1 = GetTestFile(testDir);
                    var testFile2 = GetTestFile(testDir);
                    var testFile3 = GetTestFile(testDir);

                    testFile1.Create().Dispose();
                    testFile2.Create().Dispose();
                    testFile3.Create().Dispose();

                    await Task.Delay(10);
                    Assert.IsTrue(errorCalled);
                }
            }
            finally
            {
                if (testDir.Exists)
                    testDir.Delete(true);
            }
        }

        private class FakeBufferedFileSystemWatcher : BufferedFileSystemWatcher
        {
            public FakeBufferedFileSystemWatcher(FileSystemWatcherAdapter watcher, int capacity = int.MaxValue)
                : this(watcher as IFileSystemWatcher, capacity)
            {
            }

            public FakeBufferedFileSystemWatcher(IFileSystemWatcher watcher, int capacity = int.MaxValue)
                : base(watcher, capacity)
            {
            }

            public bool OnCreatedCalled => _onCreatedCalled;

            public bool OnChangedCalled => _onChangedCalled;

            public bool OnRenamedCalled => _onRenamedCalled;

            public bool OnDeletedCalled => _onDeletedCalled;

            protected override void OnCreated(FileSystemEventArgs e) => _onCreatedCalled = true;

            protected override void OnChanged(FileSystemEventArgs e) => _onChangedCalled = true;

            protected override void OnRenamed(RenamedEventArgs e) => _onRenamedCalled = true;

            protected override void OnDeleted(FileSystemEventArgs e) => _onDeletedCalled = true;

            private bool _onCreatedCalled;
            private bool _onChangedCalled;
            private bool _onRenamedCalled;
            private bool _onDeletedCalled;
        }

        private class ErrorHandlingBufferedFileSystemWatcher : BufferedFileSystemWatcher
        {
            public ErrorHandlingBufferedFileSystemWatcher(FileSystemWatcherAdapter watcher, int capacity = int.MaxValue)
                : this(watcher as IFileSystemWatcher, capacity)
            {
            }

            public ErrorHandlingBufferedFileSystemWatcher(IFileSystemWatcher watcher, int capacity = int.MaxValue)
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
