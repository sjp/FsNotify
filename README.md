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
* Handle more fine-grained events, such as an event which is only raised when a file attribute changes, and not just when *something* about a file has changed.
* [Reactively](http://reactivex.io/) observe asynchronous streams of file system events with `ObservableFileSystemWatcher` and `EnhancedObservableFileSystemWatcher`.
* Minimal and focused.

## Installation

**NOTE:** This project is still a work in progress. However, once ready, it will be available by the following methods.

```powershell
Install-Package SJP.FsNotify
```

or

```console
dotnet add package SJP.FsNotify
```

## Usage

**TODO**

## API

**TODO**

## Examples

**TODO**

## Icon

The project icon was created by myself using a combination of two images, in addition to modifying these two images. The folder icon was created by [Madebyoliver](https://www.flaticon.com/authors/madebyoliver), while the signal icon was created by [Freepik](http://www.freepik.com).