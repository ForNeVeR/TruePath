// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;

namespace TruePath;

/// <summary>
/// <para>The kind of a <see cref="LocalPath"/>: tells how the path is anchored in the file system.</para>
/// <para>
/// On Unix, a path is either <see cref="Absolute"/> or <see cref="Relative"/>. Windows additionally has two kinds of
/// paths that are only partially anchored: <see cref="DriveRootRelative"/> and
/// <see cref="DriveCurrentDirectoryRelative"/>.
/// </para>
/// </summary>
/// <remarks>
/// This type is <b>experimental</b>: new members may be added, and existing paths may be reclassified, without a major
/// version bump. In particular, UNC and DOS device paths on Windows are not classified yet.
/// </remarks>
// TODO[#24]: UNC and DOS device paths (\\server\share, \\?\..., \\.\...) are not classified yet; add members for them.
[Experimental(ExperimentalDiagnostics.Id, UrlFormat = ExperimentalDiagnostics.UrlFormat)]
public enum PathKind
{
    /// <summary>
    /// A fully qualified path, independent of any current directory: e.g. <c>C:\Windows</c> or <c>C:\</c> on
    /// Windows, <c>/usr/bin</c> or <c>/</c> on Unix.
    /// </summary>
    Absolute,

    /// <summary>
    /// A path relative to the current directory: e.g. <c>Windows</c>, <c>..\Windows</c>, or the empty path (the
    /// current directory itself).
    /// </summary>
    /// <remarks>
    /// On Unix, every path not starting with <c>/</c> is of this kind, including <c>C:foo</c> and <c>\foo</c>.
    /// </remarks>
    Relative,

    /// <summary>
    /// <b>Windows only.</b> A path rooted without a drive letter, relative to the root of the current drive: e.g.
    /// <c>\Windows</c> or <c>\</c>.
    /// </summary>
    DriveRootRelative,

    /// <summary>
    /// <b>Windows only.</b> A path with a drive letter but no root directory, relative to the current directory of
    /// that drive: e.g. <c>C:Windows</c> or <c>C:</c>.
    /// </summary>
    DriveCurrentDirectoryRelative
}
