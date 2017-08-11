using System;
using System.IO;
using SJP.FsNotify;

namespace ConsoleApp1
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var watcher = new EnhancedFileSystemWatcher(@"C:\Users\sjp\Downloads")
            {
                IncludeSubdirectories = true
            };
            watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size;
            watcher.AttributeChanged += Watcher_AttributeChanged;
            watcher.CreationTimeChanged += Watcher_CreationTimeChanged;
            watcher.DirectoryNameChanged += Watcher_DirectoryNameChanged;
            watcher.FileNameChanged += Watcher_FileNameChanged;
            watcher.LastAccessChanged += Watcher_LastAccessChanged;
            watcher.LastWriteChanged += Watcher_LastWriteChanged;
            watcher.SecurityChanged += Watcher_SecurityChanged;
            watcher.SizeChanged += Watcher_SizeChanged;

            //watcher.Changed += Watcher_Changed;
            //watcher.Created += Watcher_Created;
            //watcher.Deleted += Watcher_Deleted;
            //watcher.Renamed += Watcher_Renamed;
            watcher.EnableRaisingEvents = true;

            Console.ReadKey();
        }

        private static void Watcher_SizeChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Changed size " + e.FullPath);
        }

        private static void Watcher_SecurityChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Changed security " + e.FullPath);
        }

        private static void Watcher_LastWriteChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Changed last write time " + e.FullPath);
        }

        private static void Watcher_LastAccessChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Changed last access time " + e.FullPath);
        }

        private static void Watcher_FileNameChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Changed file name " + e.FullPath);
        }

        private static void Watcher_DirectoryNameChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Changed directory name " + e.FullPath);
        }

        private static void Watcher_CreationTimeChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Changed creation time " + e.FullPath);
        }

        private static void Watcher_AttributeChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Changed attribute " + e.FullPath);
        }

        private static void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine($"Renamed from { e.OldFullPath } to { e.FullPath }");
        }

        private static void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Deleted " + e.FullPath);
        }

        private static void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Created " + e.FullPath);
        }

        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Changed " + e.FullPath);
        }
    }
}