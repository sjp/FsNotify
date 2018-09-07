<h1 align="center">
	<br>
	<img width="256" height="144" src="fsnotify.png" alt="FsNotify">
	<br>
	<br>
</h1>

> Improved file system notification events for .NET.

[![License (MIT)](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT) [![Build status](https://ci.appveyor.com/api/projects/status/pp70s691wpbxqidp?svg=true)](https://ci.appveyor.com/project/sjp/fsnotify)

Avoid some of the pitfalls of [`FileSystemWatcher`](https://docs.microsoft.com/en-gb/dotnet/api/system.io.filesystemwatcher) and subscribe to more fine-grained events with `SJP.FsNotify`.

## Highlights

* Supports .NET Framework 4.5.1, .NET Core 1.1, .NET Standard 1.3.
* Avoids many of the pitfalls of `FileSystemWatcher`, in particular when it can exhaust its internal buffer.
* Easy migration from `FileSystemWatcher`.
* Handle more fine-grained events, such as an event which is only raised when a file attribute changes, and not just when *something* about a file has changed. See: `EnhancedFileSystemWatcher` and `EnhancedObservableFileSystemWatcher`.
* [Reactively](http://reactivex.io/) observe asynchronous streams of file system events with `ObservableFileSystemWatcher` and `EnhancedObservableFileSystemWatcher`.

## Installation

```powershell
Install-Package SJP.FsNotify
```

or

```console
dotnet add package SJP.FsNotify
```

## Usage

Avoid exhausting `FileSystemWatcher`'s internal buffer by using the `BufferedFileSystemWatcher`. It enables for more events to be buffered in memory.

```csharp
using (var watcher = new BufferedFileSystemWatcher(@"C:\Temp"))
{
    watcher.Created += (s, e) => Console.WriteLine(e.FullPath + " was created.");
    watcher.EnableRaisingEvents = true;
    Console.ReadKey();
}
```

You can also subscribe to specific events that are normally tracked as part of `FileSystemWatcher`'s `Changed` event. Use `EnhancedFileSystemWatcher` to subscribe to these extra events.

```csharp
using (var watcher = new EnhancedFileSystemWatcher(@"C:\Temp"))
{
    watcher.AttributeChanged += (s, e) => Console.WriteLine(e.FullPath + " has had an attribute change.");
    watcher.EnableRaisingEvents = true;

    var testFile = new FileInfo("C:\Temp\TestFile.txt"); // assume the file exists
    testFile.IsReadOnly = !testFile.IsReadOnly; // toggle read-only attribute

    Console.ReadKey();
}
```

Finally, for those familiar with [reactive](http://reactivex.io/) asynchronous programming, the `ObservableFileSystemWatcher` and `EnhancedObservableFileSystemWatcher` classes are available. The former wraps the events that `FileSystemWatcher` and `BufferedFileSystemWatcher` provide, while the latter wraps the events that `EnhancedFileSystemWatcher` provides.

```csharp
using (var watcher = new EnhancedObservableFileSystemWatcher(@"C:\Temp"))
{
    watcher.LastWriteChanged.Subscribe(e => Console.WriteLine(e.FullPath + " has a new last write time."));
    watcher.Start();

    var testFile = new FileInfo(@"C:\Temp\TestFile.txt"); // assume the file exists
    testFile.LastWriteTime = new DateTime(2017, 1, 1); // update last write time

    Console.ReadKey();
}
```

## API

### `BufferedFileSystemWatcher`

The `BufferedFileSystemWatcher` can be created with an optional argument that specifies the size of the buffer. Most of the time this can be left as the default.

There are five events that are exposed:

* `Changed`
* `Created`
* `Deleted`
* `Renamed`
* `Error`

These events are rather self-explanatory and further information can be obtained at [`FileSystemWatcher`](https://docs.microsoft.com/en-gb/dotnet/api/system.io.filesystemwatcher). Be aware that the file system watcher will not throw exceptions when it is unable to process file system events, it will raise an `Error` event. This means that if you are not subscribing to the `Error` event you will not find out if file system events are not processing correctly.

### `EnhancedFileSystemWatcher`

Like `BufferedFileSystemWatcher`, `EnhancedFileSystemWatcher` can be created with an optional argument that specifies the size of the buffer.

In addition to the events that `BufferedFileSystemWatcher` exposes, `EnhancedFileSystemWatcher` provides six further events:

* `AttributeChanged`
* `CreationTimeChanged`
* `LastAccessChanged`
* `LastWriteChanged`
* `SecurityChanged`
* `SizeChanged`

### `ObservableFileSystemWatcher`

This watcher wraps the behavior of the `BufferedFileSystemWatcher` as observable collections. This means that each event is exposed as an `IObservable<T>` instead of a regular .NET event.

The observables that are provided are:

* `Changed`
* `Created`
* `Deleted`
* `Renamed`
* `Errors`

Additionally, rather than the watcher being started and stopped by an `EnableRaisingEvents` property, this is instead done by `Start()` and `Stop()` methods.

### `EnhancedObservableFileSystemWatcher`

Like `EnhancedFileSystemWatcher` to `BufferedFileSystemWatcher`, `EnhancedObservableFileSystemWatcher` provides further observable collections on top of what `ObservableFileSystemWatcher` provides.

The additional observables that are provided are:

* `AttributeChanged`
* `CreationTimeChanged`
* `LastAccessChanged`
* `LastWriteChanged`
* `SecurityChanged`
* `SizeChanged`

These additional observables are analogues of the additional events that `EnhancedFileSystemWatcher` provides.

## Icon

The project icon was created by myself using a combination of two images, in addition to modifying these two images. The folder icon was created by [Madebyoliver](https://www.flaticon.com/authors/madebyoliver), while the signal icon was created by [Freepik](http://www.freepik.com).