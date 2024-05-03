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

The library is inspired by the path libraries used in other ecosystems: in particular, Java's [java.nio.file.Path][java.path] and [Kotlin's extensions][kotlin.path].

If you miss some other operations, do not hesitate to [open an issue][issues] or [go to the discussions section][discussions].

Project Summary
---------------
TruePath allows the user to employ two main approaches to work with paths.

For cases when you want the path kinds to be checked at compile time, you can use the `AbsolutePath` and (WIP) <!-- TODO[#23] --> `RelativePath` types. This guarantees that you will work with proper absolute or relative paths, and it's impossible to mix them in an unsupported way that sometimes leads to surprising results (such as `Path.Combine("/usr", "/bin")` being equals to `"/bin"`).

If you just need a more convenient API to work with paths, and it's not important to use strict path kinds, just rely on the functionality provided by `LocalPath`: it is opaque in a sense it wraps both absolute and relative paths, and you can use it in a more flexible way. It _may_ still cause surprising behavior, though: `new LocalPath("/usr") / "/bin"` is still an equivalent of `"/bin"` (which is not the case for `AbsolutePath` and `RelativePath`).

The latter approach will cost a bit of performance, as the library will have to check the path kind at runtime.

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

### `IPath`
This is an interface that is implemented by both `LocalPath` and `AbsolutePath`. It allows to process any paths in a polymorphic way.

### `LocalPathPattern`
This is a marker type that doesn't offer any advanced functionality over the contained string. It is used to mark paths that include wildcards, for further integration with external libraries, such as [Microsoft.Extensions.FileSystemGlobbing][file-system-globbing.nuget].

### Path Features
Aside from the strict types, the following features are supported for the paths:
- `IPath::Value` returns the normalized path string;
- `IPath::FileName` returns the last component of the path;
- `IPath::Parent` returns the parent path item (`null` for the root path or top-level relative path);
- `IPath<T>` supports operators to join it with `LocalPath` or a `string` (note that in both cases appending an absolute path to path of another kind will take over: the last absolute path in chain will win and destroy all the previous ones; this is the standard behavior of path-combining methods — use `AbsolutePath` in combination with `RelativePath` if you want to avoid this behavior);
- `LocalPath::IsAbsolute` to check the path kind (since it supports both kinds);
- `LocalPath::IsPrefixOf` to check path prefixes;
- `LocalPath::RelativeTo` to get a relative part between two paths, if possible;
- extension methods on `IPath`: `GetExtensionWithDot` and `GetExtensionWithoutDot` to get the file extension with or without the leading dot (note that `GetExtensionWithDot` will behave differently for paths ending with dots and paths without dot at all, which allows to reconstruct such a file name from its part without extension and the "extension with dot").

Documentation
-------------
- [Contributor Guide][docs.contributing]
- [Maintainer Guide][docs.maintaining]

License
-------
The project is distributed under the terms of [the MIT license][docs.license].

The license indication in the project's sources is compliant with the [REUSE specification v3.0][reuse.spec].

[andivionian-status-classifier]: https://andivionian.fornever.me/v1/#status-ventis-
[discussions]: https://github.com/ForNeVeR/TruePath/discussions
[docs.contributing]: CONTRIBUTING.md
[docs.license]: LICENSE.txt
[docs.maintaining]: MAINTAINING.md
[file-system-globbing.nuget]: https://www.nuget.org/packages/Microsoft.Extensions.FileSystemGlobbing
[issue.20]: https://github.com/ForNeVeR/TruePath/issues/20
[issues]: https://github.com/ForNeVeR/TruePath/issues
[java.path]: https://docs.oracle.com/en%2Fjava%2Fjavase%2F21%2Fdocs%2Fapi%2F%2F/java.base/java/nio/file/Path.html
[kotlin.path]: https://kotlinlang.org/api/latest/jvm/stdlib/kotlin.io.path/java.nio.file.-path/
[nuget.badge]: https://img.shields.io/nuget/v/TruePath
[nuget.page]: https://www.nuget.org/packages/TruePath
[reuse.spec]: https://reuse.software/spec/
[status-ventis]: https://img.shields.io/badge/status-ventis-yellow.svg
