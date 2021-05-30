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
                Console.Error.WriteLine("A path to a directory to watch must be provided.");
                return ExitFailure;
            }

            var dirPath = args[0];
            if (!Directory.Exists(dirPath))
            {
                Console.Error.WriteLine("A valid path to a directory to watch must be provided.");
                Console.Error.WriteLine("Given path: " + dirPath);
                return ExitFailure;
            }

            var options = new ChannelFileSystemWatcherOptions(dirPath)
            {
                NotifyFilter = ChannelFileSystemWatcherOptions.AllNotifyFilters,
                IncludeSubdirectories = true
            };

            var channel = Channel.CreateBounded<FileSystemEventArgs>(1024);

            using var watcher = new ChannelFileSystemWatcher(channel.Writer, options);
            watcher.Start();

            await foreach (var fsEventArgs in channel.Reader.ReadAllAsync())
            {
                WriteColoredChangeType(fsEventArgs);
                if (fsEventArgs is RenamedEventArgs renamedEventArgs)
                {
                    Console.WriteLine(renamedEventArgs.OldFullPath + " to " + renamedEventArgs.FullPath);
                }
                else
                {
                    Console.WriteLine(fsEventArgs.FullPath);
                }
            }

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

        private const int ExitFailure = 1;
        private const int ExitSuccess = 0;
    }
}