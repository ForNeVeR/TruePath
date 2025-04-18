// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath;

/// <summary>Represents a path in a file system.</summary>
public interface IPath
{
    /// <summary>The normalized path string.</summary>
    string Value { get; }

    /// <summary>The name of this path's last component.</summary>
    string FileName { get; }

    /// <summary>
    /// The parent of this path. Will be <c>null</c> for a rooted absolute path. For a relative path, will always
    /// resolve to its parent directory — by either removing directories from the end of the path, or appending
    /// <code>..</code> to the end.
    /// </summary>
    IPath? Parent { get; }
}

/// <summary>Represents a path in a file system. Allows generic operators to be applied.</summary>
/// <typeparam name="TPath">The type of this path.</typeparam>
public interface IPath<TPath> where TPath : IPath<TPath>
{
    /// <summary>Appends another path to this one.</summary>
    /// <remarks>
    /// Note that in case path <paramref name="appended"/> is <b>absolute</b>, it will completely take over and the
    /// <paramref name="basePath"/> will be ignored.
    /// </remarks>
    static abstract TPath operator /(TPath basePath, LocalPath appended);

    /// <inheritdoc cref="op_Division(TPath,LocalPath)"/>
    static abstract TPath operator /(TPath basePath, string appended);

    /// <remarks>
    /// Checks for a non-strict prefix: if the paths are equal, then they are still considered prefixes of each other.
    /// </remarks>
    /// <remarks>Note that currently this comparison is case-sensitive.</remarks>
    bool IsPrefixOf(TPath other);

    /// <summary>
    /// Determines whether the current path starts with the specified path.
    /// </summary>
    /// <param name="other">The path to compare to the current path.</param>
    /// <remarks>Note that currently this comparison is case-sensitive.</remarks>
    bool StartsWith(TPath other);
}
