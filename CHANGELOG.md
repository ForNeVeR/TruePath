<!--
SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

Changelog
=========
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-04-21
### Added
- New types:
  - `LocalPath` for paths that may be either absolute or relative, and stored in a normalized way;
  - `AbsolutePath` for paths that are guaranteed (checked) to be absolute;
  - `LocalPathPattern` (for paths including wildcards; note this is a marker type that doesn't offer any advanced functionality over the contained string).
- New static classes:
  - `PathStrings` for path normalization (see the type's documentation on what exactly we consider as **normalization**).

## [0.0.0] - 2024-04-20
This is the first published version of the package. It doesn't contain any features, and serves the purpose of kickstarting the publication system, and to be an anchor for further additions to the package.

[docs.readme]: README.md

[0.0.0]: https://github.com/ForNeVeR/TruePath/releases/tag/v0.0.0
[Unreleased]: https://github.com/ForNeVeR/TruePath/compare/v0.0.0...HEAD
