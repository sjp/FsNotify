using System.IO;
using NUnit.Framework;

namespace SJP.FsNotify.Tests;

[TestFixture]
internal static class FileSystemWatcherAdapterTests
{
    [Test]
    public static void Ctor_GivenNullFsWatcher_ThrowsArgNullException()
    {
        Assert.That(() => new FileSystemWatcherAdapter(default!), Throws.ArgumentNullException);
    }

    [Test]
    public static void ImplicitOp_GivenNullFsWatcher_ThrowsArgNullException()
    {
        FileSystemWatcher watcher = default!;
        Assert.That(() =>
        {
            FileSystemWatcherAdapter adapter = watcher;
        }, Throws.ArgumentNullException);
    }

    [Test]
    public static void Path_WhenSetInCtor_RetrievedFromPropertyUnchanged()
    {
        using var tempDir = new TemporaryDirectory();

        var watcher = new FileSystemWatcher(tempDir.DirectoryPath);
        using var adapter = new FileSystemWatcherAdapter(watcher);
        Assert.Multiple(() =>
        {
            Assert.That(adapter.Path, Is.EqualTo(tempDir.DirectoryPath));
            Assert.That(watcher.Path, Is.EqualTo(tempDir.DirectoryPath));
        });
    }

    [Test]
    public static void Path_WhenSetInProperty_RetrievedFromPropertyCorrectly()
    {
        using var tempDir = new TemporaryDirectory();
        using var tempDir2 = new TemporaryDirectory();

        var watcher = new FileSystemWatcher(tempDir.DirectoryPath);
        using var adapter = new FileSystemWatcherAdapter(watcher) { Path = tempDir2.DirectoryPath };

        Assert.Multiple(() =>
        {
            Assert.That(adapter.Path, Is.EqualTo(tempDir2.DirectoryPath));
            Assert.That(watcher.Path, Is.EqualTo(tempDir2.DirectoryPath));
        });
    }

    [Test]
    public static void IncludeSubdirectories_WhenSetInCtor_RetrievedFromPropertyUnchanged()
    {
        var watcher = new FileSystemWatcher() { IncludeSubdirectories = true };
        using var adapter = new FileSystemWatcherAdapter(watcher);
        Assert.Multiple(() =>
        {
            Assert.That(watcher.IncludeSubdirectories, Is.True);
            Assert.That(adapter.IncludeSubdirectories, Is.True);
        });
    }

    [Test]
    public static void IncludeSubdirectories_WhenSetInProperty_RetrievedFromPropertyCorrectly()
    {
        var watcher = new FileSystemWatcher();
        using var adapter = new FileSystemWatcherAdapter(watcher) { IncludeSubdirectories = true };

        Assert.Multiple(() =>
        {
            Assert.That(watcher.IncludeSubdirectories, Is.True);
            Assert.That(adapter.IncludeSubdirectories, Is.True);
        });
    }

    [Test]
    public static void Filter_WhenSetInCtor_RetrievedFromPropertyUnchanged()
    {
        const string filter = "*.exe";
        var watcher = new FileSystemWatcher() { Filter = filter };
        using var adapter = new FileSystemWatcherAdapter(watcher);

        Assert.Multiple(() =>
        {
            Assert.That(watcher.Filter, Is.EqualTo(filter));
            Assert.That(adapter.Filter, Is.EqualTo(filter));
        });
    }

    [Test]
    public static void Filter_WhenSetInProperty_RetrievedFromPropertyCorrectly()
    {
        const string filter = "*.exe";
        var watcher = new FileSystemWatcher();
        using var adapter = new FileSystemWatcherAdapter(watcher) { Filter = filter };

        Assert.Multiple(() =>
        {
            Assert.That(watcher.Filter, Is.EqualTo(filter));
            Assert.That(adapter.Filter, Is.EqualTo(filter));
        });
    }

    [Test]
    public static void Filters_WhenSetInCtor_RetrievedFromPropertyUnchanged()
    {
        const string filter = "*.exe";
        var watcher = new FileSystemWatcher();
        watcher.Filters.Add(filter);
        using var adapter = new FileSystemWatcherAdapter(watcher);

        Assert.That(adapter.Filters, Is.EquivalentTo(watcher.Filters));
    }

    [Test]
    public static void NotifyFilter_WhenSetInCtor_RetrievedFromPropertyUnchanged()
    {
        const NotifyFilters filter = NotifyFilters.Security | NotifyFilters.Attributes;
        var watcher = new FileSystemWatcher() { NotifyFilter = filter };
        using var adapter = new FileSystemWatcherAdapter(watcher);

        Assert.Multiple(() =>
        {
            Assert.That(watcher.NotifyFilter, Is.EqualTo(filter));
            Assert.That(adapter.NotifyFilter, Is.EqualTo(filter));
        });
    }

    [Test]
    public static void NotifyFilter_WhenSetInProperty_RetrievedFromPropertyCorrectly()
    {
        const NotifyFilters filter = NotifyFilters.Security | NotifyFilters.Attributes;
        var watcher = new FileSystemWatcher();
        using var adapter = new FileSystemWatcherAdapter(watcher) { NotifyFilter = filter };

        Assert.Multiple(() =>
        {
            Assert.That(watcher.NotifyFilter, Is.EqualTo(filter));
            Assert.That(adapter.NotifyFilter, Is.EqualTo(filter));
        });
    }

    [Test]
    public static void EnableRaisingEvents_WhenSetInCtor_RetrievedFromPropertyUnchanged()
    {
        using var tempDir = new TemporaryDirectory();

        var watcher = new FileSystemWatcher(tempDir.DirectoryPath) { EnableRaisingEvents = true };
        using var adapter = new FileSystemWatcherAdapter(watcher);

        Assert.Multiple(() =>
        {
            Assert.That(watcher.EnableRaisingEvents, Is.True);
            Assert.That(adapter.EnableRaisingEvents, Is.True);
        });
    }

    [Test]
    public static void EnableRaisingEvents_WhenSetInProperty_RetrievedFromPropertyCorrectly()
    {
        using var tempDir = new TemporaryDirectory();

        var watcher = new FileSystemWatcher(tempDir.DirectoryPath);
        using var adapter = new FileSystemWatcherAdapter(watcher) { EnableRaisingEvents = true };

        Assert.Multiple(() =>
        {
            Assert.That(watcher.EnableRaisingEvents, Is.True);
            Assert.That(adapter.EnableRaisingEvents, Is.True);
        });
    }

    [Test]
    public static void NotifyFilters_WhenGivenInvalidEnum_ThrowsArgumentException()
    {
        var watcher = new FileSystemWatcher();
        using var adapter = new FileSystemWatcherAdapter(watcher);
        const NotifyFilters filter = (NotifyFilters)32908;

        Assert.That(() => adapter.NotifyFilter = filter, Throws.ArgumentException);
    }

    [Test]
    public static void WaitForChanged_WhenGivenInvalidEnum_ThrowsArgumentException()
    {
        var watcher = new FileSystemWatcher();
        using var adapter = new FileSystemWatcherAdapter(watcher);
        const WatcherChangeTypes changeType = (WatcherChangeTypes)32908;

        Assert.That(() => adapter.WaitForChanged(changeType), Throws.ArgumentException);
    }

    [Test]
    public static void WaitForChanged_WhenGivenInvalidEnumWithTimeout_ThrowsArgumentException()
    {
        var watcher = new FileSystemWatcher();
        using var adapter = new FileSystemWatcherAdapter(watcher);
        const WatcherChangeTypes changeType = (WatcherChangeTypes)32908;

        Assert.That(() => adapter.WaitForChanged(changeType, 123), Throws.ArgumentException);
    }
}
