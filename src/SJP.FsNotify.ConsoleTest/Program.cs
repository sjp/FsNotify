using System;
using System.IO;

namespace SJP.FsNotify.ConsoleTest
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            args = args ?? Array.Empty<string>();
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

            using (var watcher = new EnhancedFileSystemWatcher(dirPath) { IncludeSubdirectories = true })
            {
                watcher.AttributeChanged += OnAttributeChanged;
                watcher.CreationTimeChanged += OnCreationTimeChanged;
                watcher.LastAccessChanged += OnLastAccessChanged;
                watcher.LastWriteChanged += OnLastWriteChanged;
                watcher.SecurityChanged += OnSecurityChanged;
                watcher.SizeChanged += OnSizeChanged;
                watcher.Changed += OnChanged;
                watcher.Created += OnCreated;
                watcher.Deleted += OnDeleted;
                watcher.Renamed += OnRenamed;
                watcher.Error += OnError;

                watcher.EnableRaisingEvents = true;

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }

            return ExitSuccess;
        }

        private static void OnSizeChanged(object _, FileSystemEventArgs e) => Console.WriteLine("Changed size " + e.FullPath);

        private static void OnSecurityChanged(object _, FileSystemEventArgs e) => Console.WriteLine("Changed security " + e.FullPath);

        private static void OnLastWriteChanged(object _, FileSystemEventArgs e) => Console.WriteLine("Changed last write time " + e.FullPath);

        private static void OnLastAccessChanged(object _, FileSystemEventArgs e) => Console.WriteLine("Changed last access time " + e.FullPath);

        private static void OnCreationTimeChanged(object _, FileSystemEventArgs e) => Console.WriteLine("Changed creation time " + e.FullPath);

        private static void OnAttributeChanged(object _, FileSystemEventArgs e) => Console.WriteLine("Changed attribute " + e.FullPath);

        private static void OnRenamed(object _, RenamedEventArgs e) => Console.WriteLine($"Renamed from { e.OldFullPath } to { e.FullPath }");

        private static void OnDeleted(object _, FileSystemEventArgs e) => Console.WriteLine("Deleted " + e.FullPath);

        private static void OnCreated(object _, FileSystemEventArgs e) => Console.WriteLine("Created " + e.FullPath);

        private static void OnChanged(object _, FileSystemEventArgs e) => Console.WriteLine("Changed " + e.FullPath);

        private static void OnError(object _, ErrorEventArgs e)
        {
            var exception = e.GetException();
            Console.WriteLine("An error has occurred.");
            Console.WriteLine("Message: " + exception.Message);
            Console.WriteLine("Stack trace: " + exception.StackTrace);
        }

        private const int ExitFailure = 1;
        private const int ExitSuccess = 0;
    }
}