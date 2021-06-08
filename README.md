<h1 align="center">
    <br>
    <img width="256" height="144" src="fsnotify.png" alt="FsNotify">
    <br>
    <br>
</h1>

> Improved file system notification events for .NET.

[![License (MIT)](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT) ![Build Status](https://github.com/sjp/FsNotify/workflows/CI/badge.svg?branch=master) [![Code coverage](https://img.shields.io/codecov/c/gh/sjp/FsNotify/master?logo=codecov)](https://codecov.io/gh/sjp/FsNotify)

Avoid some of the pitfalls of [`FileSystemWatcher`](https://docs.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher) and subscribe to more fine-grained events with `SJP.FsNotify`.

## Highlights

* Supports .NET 5.0.
* Avoids many of the pitfalls of `FileSystemWatcher`, in particular when it can exhaust its internal buffer.
* Asynchronously read from a [`Channel`](https://docs.microsoft.com/en-us/dotnet/api/system.threading.channels.channel) to observe file system events.
* Determine *why* a file change event was emitted with an `EnhancedChannelFileSystemWatcher`.

## Installation

```powershell
Install-Package SJP.FsNotify
```

or

```console
dotnet add package SJP.FsNotify
```

## Usage

Avoid exhausting `FileSystemWatcher`'s internal buffer by using the `ChannelFileSystemWatcher`. It avoids relying upon the internal buffer by immediately shifting the reponsiblity of managing the buffer to the consumer.

You can create either a bounded (recommended) or an unbounded channel to process events. The bounded example follows:

```csharp
var channel = Channel.CreateBounded<FileSystemEventArgs>(1024);
var options = new ChannelFileSystemWatcherOptions(@"C:\Temp");
using var watcher = new ChannelFileSystemWatcher(channel.Writer, options);

await foreach (var fsEventArgs in channel.Reader.ReadAllAsync())
{
    Console.WriteLine($"{fsEventArgs.ChangeType} {fsEventArgs.FullPath}");
}
```

## API

### `ChannelFileSystemWatcher`

The `ChannelFileSystemWatcher` can be created with both a channel, and options which define the file watching behaviour.

There are two key methods that are exposed:

* `Start()`
* `Stop()`

These are rather self-explanatory in terms of behaviour. `Start()` begins writing available file system events to the provided channel. This will continue indefinitely until `Stop()` is called. At that point, the watcher cannot be restarted, all writing to the channel is now closed. There is no restart behaviour, a new instance of the watcher is required to perform this type of task.

There is an additional overload for constructing a `ChannelFileSystemWatcher` that also enables writing file system error messages. This is recommended but not required. No other behaviour is impacted.

### `ChannelFileSystemWatcherOptions`

The `ChannelFileSystemWatcherOptions` class is the key component which configures the behaviour of the `ChannelFileSystemWatcher` class.

There is a single constructor parameter `path`, which is a path to a directory that will be monitored. This is required and not optional. All other options can be provided during object initialisation and largely follow those from `FileSystemWatcher`. 

These options are:

* `Filter`: Sets the filter string used to determine what files are monitored in a directory. Default is `*.*`.
* `Filters`: The collection of all the filters used to determine what files are monitored in a directory.
* `IncludeSubdirectories`: Sets a value indicating whether subdirectories within the specified path should be monitored. Defaults to `false`.
* `NotifyFilter`: Sets the type of changes to watch for when a file has changed.
* `ChangedEnabled`: Whether file system change events should be written to the channel. Defaults to `true`.
* `CreatedEnabled`: Whether file system creation events should be written to the channel. Defaults to `true`.
* `DeletedEnabled`: Whether file system deletion events should be written to the channel. Defaults to `true`.
* `RenamedEnabled`: Whether file system rename events should be written to the channel. Defaults to `true`.

There are also two convenience properties that are available which can be used to configure the `NotifyFilter` value. `AllNotifyFilters`, which enables the most verbose triggering of file system change events. Additionally there is `DefaultNotifyFilters`, which simply contains the default value for `NotifyFilter`.

### `EnhancedChannelFileSystemWatcher`

The `EnhancedChannelFileSystemWatcher` can be created with both a channel, and options which define the file watching behaviour. It behaves identically to the `ChannelFileSystemWatcher`, except that when specific `NotifyFilters` values are provided, it also publishes which value in the filter triggered a given change. For example, when the `NotifyFilters.Size` value is provide, it emits when the size has changed *and* informs that the size via a change reason.

There are two key methods that are exposed:

* `Start()`
* `Stop()`

The key difference is in the event arguments provides to the channel. There are both `EnhancedFileSystemEventArgs` and `EnhancedRenamedEventArgs` which only differ from the common `FileSystemEventArgs` and `RenamedEventArgs` in that they provide a new value:

```csharp
/// <summary>
/// The reason that the event has been triggered.
/// </summary>
public FileSystemChangeType ChangeReason { get; set; }
```

This change type is an `enum` containing values such as `Created`, `LastAccessChanged`, `AttributeChanged`, etc. In short, this one property is the key reason why you would prefer an `EnhancedChannelFileSystemWatcher` over a regular `EnhancedChannelFileSystemWatcher`.

Additionally, bear in mind that the change reason will only ever emit values that are enabled by the `ChannelFileSystemWatcherOptions.NotifyFilter` value. In other words, the `ChangeReason` property can only exposes what the provided options enable.

## Icon

The project icon was created by myself using a combination of two images, in addition to modifying these two images. The folder icon was created by [Madebyoliver](https://www.flaticon.com/authors/madebyoliver), while the signal icon was created by [Freepik](http://www.freepik.com).