<!--
SPDX-FileCopyrightText: 2024-2025 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

TruePath [![Status Ventis][status-ventis]][andivionian-status-classifier]
========
This is a library containing a set of types to work with file system paths in .NET.

Motivation
----------
Historically, .NET has been lacking a good set of types to work with file system paths. The `System.IO.Path` class has a variety of methods that operate on path strings, but it doesn't provide any types to represent paths themselves. It's impossible to tell whether a method accepts an absolute path, a relative path, a file name, or something file-related at all, only looking at its signature: all these types are represented by plain strings. Also, comparing different paths is not straightforward.

This library aims to fill this gap by providing a set of types that represent paths in a strongly-typed way. Now, you can require a path in a method's parameters, and it is guaranteed that the passed path will be well-formed and will have certain properties.

Also, the methods in the library provide some qualities that are missing from the `System.IO.Path`: say, we aim to provide several ways of path normalization and comparison, the ones that will and will not perform disk IO to resolve paths on case-insensitive file systems.

The library is inspired by the path libraries used in other ecosystems: in particular, Java's [java.nio.file.Path][java.path] and [Kotlin's extensions][kotlin.path].

Read more on [the documentation site][docs].

If you miss some feature or have questions, do not hesitate to [open an issue][issues] or [go to the discussions section][discussions].

Packages
--------
| Name                  | NuGet Package                                                                                   | Documentation                             |
|-----------------------|-------------------------------------------------------------------------------------------------|-------------------------------------------|
| **TruePath**          | [![TruePath on nuget.org][nuget.badge.true-path]][nuget.true-path]                              | [API Reference][docs.true-path]           |
| **TruePath.SystemIo** | [![TruePath.SystemIo on nuget.org][nuget.badge.true-path.system-io]][nuget.true-path.system-io] | [API Reference][docs.true-path.system-io] |

TruePath provides two NuGet packages:
- [**TruePath**][nuget.true-path] for the main path abstractions,
- [**TruePath.SystemIo**][nuget.true-path.system-io] for the `System.IO` integration.

Documentation
-------------
- [Project Documentation Site][docs]
- [Changelog][docs.changelog]
- [Contributor Guide][docs.contributing]
- [Maintainer Guide][docs.maintaining]

License
-------
The project is distributed under the terms of [the MIT license][docs.license].

The license indication in the project's sources is compliant with the [REUSE specification v3.3][reuse.spec].

[andivionian-status-classifier]: https://andivionian.fornever.me/v1/#status-ventis-
[discussions]: https://github.com/ForNeVeR/TruePath/discussions
[docs.changelog]: CHANGELOG.md
[docs.contributing]: CONTRIBUTING.md
[docs.license]: LICENSE.txt
[docs.maintaining]: MAINTAINING.md
[docs.true-path.system-io]: https://fornever.github.io/TruePath/api/TruePath.SystemIo.html
[docs.true-path]: https://fornever.github.io/TruePath/api/TruePath.html
[docs]: https://fornever.github.io/TruePath
[issues]: https://github.com/ForNeVeR/TruePath/issues
[java.path]: https://docs.oracle.com/en%2Fjava%2Fjavase%2F21%2Fdocs%2Fapi%2F%2F/java.base/java/nio/file/Path.html
[kotlin.path]: https://kotlinlang.org/api/latest/jvm/stdlib/kotlin.io.path/java.nio.file.-path/
[nuget.badge.true-path.system-io]: https://img.shields.io/nuget/v/TruePath.SystemIo
[nuget.badge.true-path]: https://img.shields.io/nuget/v/TruePath
[nuget.true-path.system-io]: https://www.nuget.org/packages/TruePath.SystemIo
[nuget.true-path]: https://www.nuget.org/packages/TruePath
[reuse.spec]: https://reuse.software/spec-3.3/
[status-ventis]: https://img.shields.io/badge/status-ventis-yellow.svg
