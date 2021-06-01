using System;
using System.IO;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace SJP.FsNotify.Tests
{
    [TestFixture]
    internal class ChannelFileSystemWatcherTests
    {
        private const int FsEventTimeout = 50; // ms

        private TemporaryDirectory _tempDir = default!;
        private Channel<FileSystemEventArgs> _channel = default!;
        private Channel<ErrorEventArgs> _errorChannel = default!;
        private Mock<IChannelFileSystemWatcherOptions> _options = default!;
        private ChannelFileSystemWatcherOptions _sourceOptions = default!;

        private ChannelFileSystemWatcher _watcher = default!;

        [SetUp]
        public void Setup()
        {
            _tempDir = new TemporaryDirectory();

            _channel = Channel.CreateUnbounded<FileSystemEventArgs>();
            _errorChannel = Channel.CreateUnbounded<ErrorEventArgs>();
            _sourceOptions = new ChannelFileSystemWatcherOptions(_tempDir.DirectoryPath);

            _options = new Mock<IChannelFileSystemWatcherOptions>(MockBehavior.Strict);
            _options.Setup(x => x.ChangedEnabled).Returns(() => _sourceOptions.ChangedEnabled);
            _options.Setup(x => x.CreatedEnabled).Returns(() => _sourceOptions.CreatedEnabled);
            _options.Setup(x => x.DeletedEnabled).Returns(() => _sourceOptions.DeletedEnabled);
            _options.Setup(x => x.Filter).Returns(() => _sourceOptions.Filter);
            _options.Setup(x => x.Filters).Returns(() => _sourceOptions.Filters);
            _options.Setup(x => x.IncludeSubdirectories).Returns(() => _sourceOptions.IncludeSubdirectories);
            _options.Setup(x => x.NotifyFilter).Returns(() => _sourceOptions.NotifyFilter);
            _options.Setup(x => x.Path).Returns(() => _sourceOptions.Path);
            _options.Setup(x => x.RenamedEnabled).Returns(() => _sourceOptions.RenamedEnabled);

            _watcher = new ChannelFileSystemWatcher(_channel, _errorChannel, _options.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _watcher.Dispose();
            _tempDir.Dispose();
        }

        [Test]
        public static void Ctor_GivenNullChannelWriter_ThrowsArgNullException()
        {
            var options = new ChannelFileSystemWatcherOptions(".");
            Assert.Throws<ArgumentNullException>(() => new ChannelFileSystemWatcher(null!, options));
        }

        [Test]
        public static void Ctor_GivenNullChannelOptions_ThrowsArgNullException()
        {
            var channel = Channel.CreateBounded<FileSystemEventArgs>(1);
            Assert.Throws<ArgumentNullException>(() => new ChannelFileSystemWatcher(channel.Writer, null!));
        }

        [Test]
        public static void Ctor_GivenNullErrorChannelWriter_ThrowsArgNullException()
        {
            var channel = Channel.CreateBounded<FileSystemEventArgs>(1);
            var options = new ChannelFileSystemWatcherOptions(".");
            Assert.Throws<ArgumentNullException>(() => new ChannelFileSystemWatcher(channel, null!, options));
        }

        [Test]
        public async Task OnCreated_WhenFileCreated_PublishesToChannel()
        {
            // Disable everything but create
            _sourceOptions = new ChannelFileSystemWatcherOptions(_tempDir.DirectoryPath)
            {
                CreatedEnabled = true,
                ChangedEnabled = false,
                DeletedEnabled = false,
                RenamedEnabled = false
            };

            var testFile = FsNotifyTest.GetTestFile(new DirectoryInfo(_tempDir.DirectoryPath));

            _watcher.Start();
            testFile.Create().Dispose();
            await Task.Delay(FsEventTimeout).ConfigureAwait(false); // wait for watcher to notify to channel before closing
            _watcher.Stop();

            var containsFile = await _channel.Reader.ReadAllAsync().AnyAsync(evt => evt.Name == testFile.Name).ConfigureAwait(false);
            Assert.That(containsFile, Is.True);
        }

        [Test]
        public async Task OnChanged_WhenFileChanged_PublishesToChannel()
        {
            // Disable everything but create
            _sourceOptions = new ChannelFileSystemWatcherOptions(_tempDir.DirectoryPath)
            {
                CreatedEnabled = false,
                ChangedEnabled = true,
                DeletedEnabled = false,
                RenamedEnabled = false
            };

            var testFile = FsNotifyTest.GetTestFile(new DirectoryInfo(_tempDir.DirectoryPath));
            testFile.Create().Dispose();

            _watcher.Start();
            using (var writer = testFile.AppendText())
                await writer.WriteLineAsync("trigger change").ConfigureAwait(false);
            testFile.LastWriteTime = new DateTime(2016, 1, 1);
            await Task.Delay(FsEventTimeout).ConfigureAwait(false); // wait for watcher to notify to channel before closing
            _watcher.Stop();

            var containsFile = await _channel.Reader.ReadAllAsync().AnyAsync(evt => evt.Name == testFile.Name).ConfigureAwait(false);
            Assert.That(containsFile, Is.True);
        }

        [Test]
        public async Task OnDeleted_WhenFileDeleted_PublishesToChannel()
        {
            // Disable everything but create
            _sourceOptions = new ChannelFileSystemWatcherOptions(_tempDir.DirectoryPath)
            {
                CreatedEnabled = false,
                ChangedEnabled = false,
                DeletedEnabled = true,
                RenamedEnabled = false
            };

            var testFile = FsNotifyTest.GetTestFile(new DirectoryInfo(_tempDir.DirectoryPath));
            testFile.Create().Dispose();

            _watcher.Start();
            testFile.Delete();
            await Task.Delay(FsEventTimeout).ConfigureAwait(false); // wait for watcher to notify to channel before closing
            _watcher.Stop();

            var containsFile = await _channel.Reader.ReadAllAsync().AnyAsync(evt => evt.Name == testFile.Name).ConfigureAwait(false);
            Assert.That(containsFile, Is.True);
        }

        [Test]
        public async Task OnRenamed_WhenFileRenamed_PublishesToChannel()
        {
            // Disable everything but create
            _sourceOptions = new ChannelFileSystemWatcherOptions(_tempDir.DirectoryPath)
            {
                CreatedEnabled = false,
                ChangedEnabled = false,
                DeletedEnabled = false,
                RenamedEnabled = true
            };

            var testFile = FsNotifyTest.GetTestFile(new DirectoryInfo(_tempDir.DirectoryPath));
            var testFile2 = FsNotifyTest.GetTestFile(new DirectoryInfo(_tempDir.DirectoryPath));
            testFile.Create().Dispose();

            _watcher.Start();
            File.Move(testFile.FullName, testFile2.FullName);
            await Task.Delay(FsEventTimeout).ConfigureAwait(false); // wait for watcher to notify to channel before closing
            _watcher.Stop();

            var containsFile = await _channel.Reader
                .ReadAllAsync()
                .OfType<RenamedEventArgs>()
                .AnyAsync(evt => evt.OldFullPath == testFile.FullName && evt.FullPath == testFile2.FullName)
                .ConfigureAwait(false);
            Assert.That(containsFile, Is.True);
        }

        [Test]
        public async Task OnError_WhenErrorOccurs_PublishesToErrorChannel()
        {
            var fsWatcher = new Mock<IFileSystemWatcher>(MockBehavior.Loose);

            _watcher = new ChannelFileSystemWatcher(_channel, _errorChannel, _options.Object, fsWatcher.Object);
            _watcher.Start();

            fsWatcher.Raise(w => w.Error += null, new ErrorEventArgs(new Exception("test_message")));
            await Task.Delay(FsEventTimeout).ConfigureAwait(false); // wait for watcher to notify to channel before closing
            _watcher.Stop();

            var containsError = await _errorChannel.Reader.ReadAllAsync().AnyAsync(e => e.GetException().Message == "test_message").ConfigureAwait(false);
            Assert.That(containsError, Is.True);
        }

        [Test]
        public void Start_WhenRestartingAfterStopping_ThrowsError()
        {
            _watcher.Start();
            _watcher.Stop();

            Assert.That(() => _watcher.Start(), Throws.ArgumentException);
        }
    }
}
