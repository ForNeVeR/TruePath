<!--
SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

TruePath [![Status Ventis][status-ventis]][andivionian-status-classifier] [![NuGet package][nuget.badge]][nuget.page]
========
This is a library containing a set of types to work with file system paths in .NET.

Motivation
----------
Historically, .NET has been lacking a good set of types to work with file system paths. The `System.IO.Path` class has a variety of methods that operate on path strings, but it doesn't provide any types to represent paths themselves. It's impossible top tell whether a method accepts an absolute path, a relative path, a file name, or something file-related at all, only looking at its signature: all these types are represented by plain strings. Also, comparing different paths is not straightforward.

This library aims to fill this gap by providing a set of types that represent paths in a strongly-typed way. Now, you can require a path in a method's parameters, and it is guaranteed that the passed path will be well-formed and will have certain properties.

Also, the methods in the library provide some qualities that are missing from the `System.IO.Path`: say, we aim to provide several ways of path normalization and comparison, the ones that will and will not perform disk IO to resolve paths on case-insensitive file systems.

If you miss some other operations, do not hesitate to [open an issue][issues] or [go to the discussions section][discussions].

Usage
-----
The library offers several struct (i.e. low to zero memory overhead) types wrapping path strings. The types are designed to not involve any disk IO operations by default, and thus provide excellent performance during common operations. This comes with a drawback, though: **path comparison is only performed as string comparison so far**, which means that the library doesn't provide any means to compare paths in a case-insensitive way.

This is a subject to change in future releases, where we will provide more control over this: better platform-wide defaults (such as case-insensitive comparison on Windows and macOS), and options to enable more IO-intensive comparison (to check sensitivity settings of particular file path components during comparison). See [issue #20][issue.20] on the current progress on this change.

The paths are stored in the **normalized form**.

- All the `Path.AltDirectorySeparatorChar` are converted to `Path.DirectorySeparatorChar` (e.g. `/` to `\` on Windows).
- Any repeated separators in the input are collapsed to only one separator (e.g. `//` to just `/` on Unix).
- Any sequence of current and parent directory marks (subsequently, `.` and `..`) is resolved if possible (meaning they
  will not be replaced if they are in the root position: paths such as `.` or `../..` will not be affected by the
  normalization, while e.g. `foo/../.` will be resolved to just `foo`).

Note that the normalization operation will not perform any file IO, and is purely string manipulation.

### `LocalPath`
This is the type that may either be a relative or an absolute. Small showcase:
```csharp
var myRoot = new LocalPath("foo/bar");
var fooDirectory = myRoot.Parent;

var bazSubdirectory = myRoot / "baz";
var alsoBazSubdirectory = myRoot / new LocalPath("baz");
```

### `AbsolutePath`
This functions basically the same as the `LocalPath`, but it is _always_ an absolute path, which is checked in the constructor.

To convert from `LocalPath` to `AbsolutePath` and vice versa, you can use the constructors of `AbsolutePath` and `LocalPath` respectively. Any `AbsolutePath` constructor (from either a string or a `LocalPath`) has same check for absolute path, and any `LocalPath` constructor (from either a string or an `AbsolutePath`) doesn't have any checks.

### `LocalPathPattern`
This is a marker type that doesn't offer any advanced functionality over the contained string. It is used to mark paths that include wildcards, for further integration with external libraries, such as [Microsoft.Extensions.FileSystemGlobbing][file-system-globbing.nuget].

Documentation
-------------
- [Contributor Guide][docs.contributing]
- [Maintainer Guide][docs.maintaining]

License
-------
This project's licensing follows the [REUSE specification v 3.0][reuse.spec]. Consult each file's headers and the REUSE specification for possible details.

### Contribution Policy
By contributing to this repository, you agree that any new files you contribute will be covered by the MIT license. If you want to contribute a file under a different license, you should clearly mark it in the file's header, according to the REUSE specification.

You are welcome to explicitly state your copyright in the file's header as described in [the contributor guide][docs.contributing], but the project maintainers may do this for you as well.

[andivionian-status-classifier]: https://andivionian.fornever.me/v1/#status-ventis-
[discussions]: https://github.com/ForNeVeR/TruePath/discussions
[docs.contributing]: CONTRIBUTING.md
[docs.maintaining]: MAINTAINING.md
[file-system-globbing.nuget]: https://www.nuget.org/packages/Microsoft.Extensions.FileSystemGlobbing
[issue.20]: https://github.com/ForNeVeR/TruePath/issues/20
[issues]: https://github.com/ForNeVeR/TruePath/issues
[nuget.badge]: https://img.shields.io/nuget/v/TruePath
[nuget.page]: https://www.nuget.org/packages/TruePath
[reuse.spec]: https://reuse.software/spec/
[status-ventis]: https://img.shields.io/badge/status-ventis-yellow.svg
