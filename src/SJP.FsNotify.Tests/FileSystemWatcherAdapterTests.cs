using System;
using System.IO;
using NUnit.Framework;
using Moq;
using System.Threading.Tasks;

namespace SJP.FsNotify.Tests
{
    [TestFixture]
    internal static class FileSystemWatcherAdapterTests
    {
        [Test]
        public static void Ctor_GivenNullFsWatcher_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FileSystemWatcherAdapter(null));
        }

        [Test]
        public static void ImplicitOp_GivenNullFsWatcher_ThrowsArgNullException()
        {
            FileSystemWatcher watcher = null;
            Assert.Throws<ArgumentNullException>(() =>
            {
                FileSystemWatcherAdapter adapter = watcher;
            });
        }

        [Test]
        public static void Path_WhenSetInCtor_RetrievedFromPropertyUnchanged()
        {
            var testDir = FsNotifyTest.GetTestDirectory();

            var watcher = new FileSystemWatcher(testDir.FullName);
            using (var adapter = new FileSystemWatcherAdapter(watcher))
            {
                Assert.Multiple(() =>
                {
                    Assert.AreEqual(testDir.FullName, adapter.Path);
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
            using (var adapter = new FileSystemWatcherAdapter(watcher))
            {
                adapter.Path = testDir2.FullName;

                Assert.Multiple(() =>
                {
                    Assert.AreEqual(testDir2.FullName, adapter.Path);
                    Assert.AreEqual(testDir2.FullName, watcher.Path);
                });
            }
        }

        [Test]
        public static void IncludeSubdirectories_WhenSetInCtor_RetrievedFromPropertyUnchanged()
        {
            var watcher = new FileSystemWatcher() { IncludeSubdirectories = true };
            using (var adapter = new FileSystemWatcherAdapter(watcher))
            {
                Assert.Multiple(() =>
                {
                    Assert.IsTrue(watcher.IncludeSubdirectories);
                    Assert.IsTrue(adapter.IncludeSubdirectories);
                });
            }
        }

        [Test]
        public static void IncludeSubdirectories_WhenSetInProperty_RetrievedFromPropertyCorrectly()
        {
            var watcher = new FileSystemWatcher();
            using (var adapter = new FileSystemWatcherAdapter(watcher))
            {
                adapter.IncludeSubdirectories = true;

                Assert.Multiple(() =>
                {
                    Assert.IsTrue(watcher.IncludeSubdirectories);
                    Assert.IsTrue(adapter.IncludeSubdirectories);
                });
            }
        }

        [Test]
        public static void Filter_WhenSetInCtor_RetrievedFromPropertyUnchanged()
        {
            const string filter = "*.exe";
            var watcher = new FileSystemWatcher() { Filter = filter };
            using (var adapter = new FileSystemWatcherAdapter(watcher))
            {
                Assert.Multiple(() =>
                {
                    Assert.AreEqual(filter, watcher.Filter);
                    Assert.AreEqual(filter, adapter.Filter);
                });
            }
        }

        [Test]
        public static void Filter_WhenSetInProperty_RetrievedFromPropertyCorrectly()
        {
            const string filter = "*.exe";
            var watcher = new FileSystemWatcher();
            using (var adapter = new FileSystemWatcherAdapter(watcher))
            {
                adapter.Filter = filter;

                Assert.Multiple(() =>
                {
                    Assert.AreEqual(filter, watcher.Filter);
                    Assert.AreEqual(filter, adapter.Filter);
                });
            }
        }

        [Test]
        public static void NotifyFilter_WhenSetInCtor_RetrievedFromPropertyUnchanged()
        {
            const NotifyFilters filter = NotifyFilters.Security | NotifyFilters.Attributes;
            var watcher = new FileSystemWatcher() { NotifyFilter = filter };
            using (var adapter = new FileSystemWatcherAdapter(watcher))
            {
                Assert.Multiple(() =>
                {
                    Assert.AreEqual(filter, watcher.NotifyFilter);
                    Assert.AreEqual(filter, adapter.NotifyFilter);
                });
            }
        }

        [Test]
        public static void NotifyFilter_WhenSetInProperty_RetrievedFromPropertyCorrectly()
        {
            const NotifyFilters filter = NotifyFilters.Security | NotifyFilters.Attributes;
            var watcher = new FileSystemWatcher();
            using (var adapter = new FileSystemWatcherAdapter(watcher))
            {
                adapter.NotifyFilter= filter;

                Assert.Multiple(() =>
                {
                    Assert.AreEqual(filter, watcher.NotifyFilter);
                    Assert.AreEqual(filter, adapter.NotifyFilter);
                });
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
                using (var adapter = new FileSystemWatcherAdapter(watcher))
                {
                    Assert.Multiple(() =>
                    {
                        Assert.IsTrue(watcher.EnableRaisingEvents);
                        Assert.IsTrue(adapter.EnableRaisingEvents);
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
                using (var adapter = new FileSystemWatcherAdapter(watcher))
                {
                    adapter.EnableRaisingEvents = true;

                    Assert.Multiple(() =>
                    {
                        Assert.IsTrue(watcher.EnableRaisingEvents);
                        Assert.IsTrue(adapter.EnableRaisingEvents);
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
            var watcher = new FileSystemWatcher();
            using (var adapter = new FileSystemWatcherAdapter(watcher))
            {
                const NotifyFilters filter = (NotifyFilters)32908;
                Assert.Throws<ArgumentException>(() => adapter.NotifyFilter = filter);
            }
        }

        [Test]
        public static void WaitForChanged_WhenGivenInvalidEnum_ThrowsArgumentException()
        {
            var watcher = new FileSystemWatcher();
            using (var adapter = new FileSystemWatcherAdapter(watcher))
            {
                const WatcherChangeTypes changeType = (WatcherChangeTypes)32908;
                Assert.Throws<ArgumentException>(() => adapter.WaitForChanged(changeType));
            }
        }

        [Test]
        public static void WaitForChanged_WhenGivenInvalidEnumWithTimeout_ThrowsArgumentException()
        {
            var watcher = new FileSystemWatcher();
            using (var adapter = new FileSystemWatcherAdapter(watcher))
            {
                const WatcherChangeTypes changeType = (WatcherChangeTypes)32908;
                Assert.Throws<ArgumentException>(() => adapter.WaitForChanged(changeType, 123));
            }
        }
    }
}
