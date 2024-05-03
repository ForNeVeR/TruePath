<!--
SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

Changelog
=========
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased] (1.2.0)
### Added
- New `IPath` and `IPath<TPath>` interfaces that allow to process any paths (`LocalPath` and `AbsolutePath`) in a polymorphic way.
- [#42](https://github.com/ForNeVeR/TruePath/issues/42): `GetExtensionWithDot` and `GetExtensionWithoutDot` extension methods for `IPath`.

  Thanks to @Komroncube.

### Changed
- [#19](https://github.com/ForNeVeR/TruePath/issues/19): Optimize `PathStrings.Normalize` method.

  Thanks ot @BadRyuner.

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
[Unreleased]: https://github.com/ForNeVeR/TruePath/compare/v1.1.0...HEAD
