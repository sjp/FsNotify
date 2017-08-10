using System;
using System.IO;
using SJP.FsNotify;

namespace ConsoleApp1
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var watcher = new BufferedFileSystemWatcher(@"C:\Users\sjp\Downloads")
            {
                IncludeSubdirectories = true
            };
            watcher.Changed += Watcher_Changed;
            watcher.Created += Watcher_Created;
            watcher.Deleted += Watcher_Deleted;
            watcher.Renamed += Watcher_Renamed;
            watcher.EnableRaisingEvents = true;

            Console.ReadKey();
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