<!--
SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

Changelog
=========
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Fixed
- Incorrect path normalization: last ellipsis (`...`) in a path was treated as a `..` entry.

### Changed
- [#18](https://github.com/ForNeVeR/TruePath/issues/18): update to behavior of `.Parent` on relative paths.

  Now, it works for relative paths by either removing the last part or adding `..` as necessary to lead to a parent directory.

  Thanks to @Kataane for help on this one.

## [1.4.0] - 2024-08-12
### Changed
- [#16: Support Windows disk drives in the normalization algorithm](https://github.com/ForNeVeR/TruePath/issues/16).

  Thanks to @Kataane.
- More optimizations for `AbsolutePath`'s `/` operator: it will avoid the unnecessary check for absolute path.
- [#85: Minor performance improvements for absolute path checking on Windows](https://github.com/ForNeVeR/TruePath/pull/85).

  Thanks to @Kataane.

### Added
- `AbsolutePath::Canonicalize` to convert the path to absolute, convert to correct case on case-insensitive file systems, resolve symlinks.

  Thanks to @Kataane.
- `LocalPath::ResolveToCurrentDirectory`: effectively calculates `currentDirectory / this`. No-op for paths that are already absolute (aside from converting to the `AbsolutePath` type).

  Thanks to @Illusion4.
- `AbsolutePath::ReadKind` to check the file system object kind (file, directory, or something else) and whether it exists at all.

  Thanks to @Kataane.
- [#76](https://github.com/ForNeVeR/TruePath/issues/76): a new `Temporary` class for creating a temp file or folder.

  Thanks to @Illusion4.

## [1.3.0] - 2024-06-21
### Added
- [#39: Add `AbsolutePath::RelativeTo`](https://github.com/ForNeVeR/TruePath/issues/39).

  Thanks to @ronimizy.
- `IPath::IsPrefixOf` to check path prefixes.

  Thanks to @babaruh.
- `IPath::StartsWith` to check if the current path starts with a specified path.

  Thanks to @babaruh.
- [#38: Introduce `AbsolutePath::CurrentWorkingDirectory`](https://github.com/ForNeVeR/TruePath/issues/38).

  Thanks to @babaruh.

### Changed
- [#17: `AbsolutePath::Parent` should not re-check the path's absoluteness](https://github.com/ForNeVeR/TruePath/issues/17) (a performance optimization).

  Thanks to @ronimizy.

## [1.2.1] - 2024-05-25
### Fixed
- [#60: `PathStrings.Normalize("../../foo")` is broken](https://github.com/ForNeVeR/TruePath/issues/60).

  Thanks to @ronimizy for report.

## [1.2.0] - 2024-05-05
### Added
- New `IPath` and `IPath<TPath>` interfaces that allow to process any paths (`LocalPath` and `AbsolutePath`) in a polymorphic way.
- [#41](https://github.com/ForNeVeR/TruePath/issues/41): `GetFileNameWithoutExtension` extension method for `IPath`.

  Thanks to @Komroncube.
- [#42](https://github.com/ForNeVeR/TruePath/issues/42): `GetExtensionWithDot` and `GetExtensionWithoutDot` extension methods for `IPath`.

  Thanks to @Komroncube and @y0ung3r.

### Changed
- [#19](https://github.com/ForNeVeR/TruePath/issues/19): Optimize `PathStrings.Normalize` method.

  Thanks to @BadRyuner.
- Improve the project documentation.

## [1.1.0] - 2024-04-27
### Added
- [#26: Publish PDB files to NuGet](https://github.com/ForNeVeR/TruePath/issues/26) (in form of `.snupkg` for now).
- Update and publish XML documentation with the package.
- Enable the Source Link.
- [#29](https://github.com/ForNeVeR/TruePath/issues/29): add converting constructors for `LocalPath` and `AbsolutePath`.
- `AbsolutePath` and `LocalPath` now support `IEquatable<T>` interface.

## [1.0.0] - 2024-04-21
### Added
- New types:
  - `LocalPath` for paths that may be either absolute or relative, and stored in a normalized way;
  - `AbsolutePath` for paths that are guaranteed (checked) to be absolute;
  - `LocalPathPattern` (for paths including wildcards; note this is a marker type that doesn't offer any advanced functionality over the contained string).
- New static classes:
  - `PathStrings` for path normalization (see the type's documentation on what exactly we consider as **normalization**).
- Currently supported features:
  - path normalization (in-memory only, no disk IO performed),
  - path concatenation via `/` operator,
  - check for absolute path (work in progress; doesn't completely work for Windows paths yet),
  - get path parent,
  - get the last path component's name,
  - check for path prefix,
  - get a relative part between two paths,
  - check paths for equality (case-insensitive only, yet).

## [0.0.0] - 2024-04-20
This is the first published version of the package. It doesn't contain any features, and serves the purpose of kickstarting the publication system, and to be an anchor for further additions to the package.

[docs.readme]: README.md

[0.0.0]: https://github.com/ForNeVeR/TruePath/releases/tag/v0.0.0
[1.0.0]: https://github.com/ForNeVeR/TruePath/compare/v0.0.0...v1.0.0
[1.1.0]: https://github.com/ForNeVeR/TruePath/compare/v1.0.0...v1.1.0
[1.2.0]: https://github.com/ForNeVeR/TruePath/compare/v1.1.0...v1.2.0
[1.2.1]: https://github.com/ForNeVeR/TruePath/compare/v1.2.0...v1.2.1
[1.3.0]: https://github.com/ForNeVeR/TruePath/compare/v1.2.1...v1.3.0
[1.4.0]: https://github.com/ForNeVeR/TruePath/compare/v1.3.0...v1.4.0
[Unreleased]: https://github.com/ForNeVeR/TruePath/compare/v1.4.0...HEAD
