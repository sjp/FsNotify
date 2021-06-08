using System;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SJP.FsNotify.ConsoleTest
{
    internal static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            args ??= Array.Empty<string>();
            if (args.Length == 0)
            {
                await Console.Error.WriteLineAsync("A path to a directory to watch must be provided.").ConfigureAwait(false);
                return ExitFailure;
            }

            var dirPath = args[0];
            if (!Directory.Exists(dirPath))
            {
                await Console.Error.WriteLineAsync("A valid path to a directory to watch must be provided.").ConfigureAwait(false);
                await Console.Error.WriteLineAsync("Given path: " + dirPath).ConfigureAwait(false);
                return ExitFailure;
            }

            await Console.Out.WriteLineAsync("Press any key to exit.").ConfigureAwait(false);

            // Uncomment to run the standard watcher
            // return await RunStandardFsWatcher(dirPath);
            return await RunEnhancedFsWatcher(dirPath);
        }

        private static async Task<int> RunStandardFsWatcher(string dirPath)
        {
            var channel = Channel.CreateBounded<FileSystemEventArgs>(1024);
            var options = new ChannelFileSystemWatcherOptions(dirPath)
            {
                NotifyFilter = ChannelFileSystemWatcherOptions.AllNotifyFilters,
                IncludeSubdirectories = true
            };

            using var watcher = new ChannelFileSystemWatcher(channel.Writer, options);
            watcher.Start();

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    if (Console.KeyAvailable)
                        watcher.Stop();

                    await Task.Delay(100).ConfigureAwait(false);
                }
            });

            await foreach (var fsEventArgs in channel.Reader.ReadAllAsync())
            {
                WriteColoredChangeType(fsEventArgs);
                if (fsEventArgs is RenamedEventArgs renamedEventArgs)
                {
                    await Console.Out.WriteLineAsync(renamedEventArgs.OldFullPath + " to " + renamedEventArgs.FullPath).ConfigureAwait(false);
                }
                else
                {
                    await Console.Out.WriteLineAsync(fsEventArgs.FullPath).ConfigureAwait(false);
                }
            }

            await Console.Out.WriteLineAsync("Exited...").ConfigureAwait(false);
            return ExitSuccess;
        }

        private static async Task<int> RunEnhancedFsWatcher(string dirPath)
        {
            var channel = Channel.CreateBounded<EnhancedFileSystemEventArgs>(1024);
            var options = new ChannelFileSystemWatcherOptions(dirPath)
            {
                NotifyFilter = ChannelFileSystemWatcherOptions.AllNotifyFilters,
                IncludeSubdirectories = true
            };

            using var watcher = new EnhancedChannelFileSystemWatcher(channel.Writer, options);
            watcher.Start();

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    if (Console.KeyAvailable)
                        watcher.Stop();

                    await Task.Delay(100).ConfigureAwait(false);
                }
            });

            await foreach (var fsEventArgs in channel.Reader.ReadAllAsync())
            {
                WriteColoredChangeReason(fsEventArgs);
                if (fsEventArgs is EnhancedRenamedEventArgs renamedEventArgs)
                {
                    await Console.Out.WriteLineAsync(renamedEventArgs.OldFullPath + " to " + renamedEventArgs.FullPath).ConfigureAwait(false);
                }
                else
                {
                    await Console.Out.WriteLineAsync(fsEventArgs.FullPath).ConfigureAwait(false);
                }
            }

            await Console.Out.WriteLineAsync("Exited...").ConfigureAwait(false);
            return ExitSuccess;
        }

        private static void WriteColoredChangeType(FileSystemEventArgs fsEventArgs)
        {
            Console.Write("[");

            var originalColor = Console.ForegroundColor;

            var color = fsEventArgs.ChangeType switch
            {
                WatcherChangeTypes.Changed => ConsoleColor.Yellow,
                WatcherChangeTypes.Created => ConsoleColor.Green,
                WatcherChangeTypes.Deleted => ConsoleColor.Red,
                WatcherChangeTypes.Renamed => ConsoleColor.White,
                _ => ConsoleColor.White
            };

            Console.ForegroundColor = color;
            Console.Write(fsEventArgs.ChangeType.ToString().ToUpperInvariant());
            Console.ForegroundColor = originalColor;

            Console.Write("] ");
        }

        private static void WriteColoredChangeReason(EnhancedFileSystemEventArgs fsEventArgs)
        {
            Console.Write("[");

            var originalColor = Console.ForegroundColor;

            var color = fsEventArgs.ChangeType switch
            {
                WatcherChangeTypes.Changed => ConsoleColor.Yellow,
                WatcherChangeTypes.Created => ConsoleColor.Green,
                WatcherChangeTypes.Deleted => ConsoleColor.Red,
                WatcherChangeTypes.Renamed => ConsoleColor.White,
                _ => ConsoleColor.White
            };

            Console.ForegroundColor = color;
            Console.Write(fsEventArgs.ChangeReason.ToString().ToUpperInvariant());
            Console.ForegroundColor = originalColor;

            Console.Write("] ");
        }

        private const int ExitFailure = 1;
        private const int ExitSuccess = 0;
    }
}