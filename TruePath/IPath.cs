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
#if NET8_0_OR_GREATER
    /// <summary>Appends another path to this one.</summary>
    /// <remarks>
    /// Note that in case path <paramref name="appended"/> is <b>absolute</b>, it will completely take over and the
    /// <paramref name="basePath"/> will be ignored.
    /// </remarks>
    static abstract TPath operator /(TPath basePath, LocalPath appended);

    /// <inheritdoc cref="op_Division(TPath,LocalPath)"/>
    static abstract TPath operator /(TPath basePath, string appended);
#endif

    /// <remarks>
    /// Checks for a non-strict prefix: if the paths are equal, then they are still considered prefixes of each other,
    /// so every path is a prefix of itself.
    /// </remarks>
    /// <remarks>
    /// The comparison is performed over whole path <b>segments</b>: <c>/foo</c> is <b>not</b> a prefix of
    /// <c>/foo1</c>, even though one path string starts with the other.
    /// </remarks>
    /// <remarks>
    /// This is the exact inverse of <see cref="StartsWith"/>: <c>a.IsPrefixOf(b)</c> means the same as
    /// <c>b.StartsWith(a)</c>.
    /// </remarks>
    /// <remarks>Case sensitivity follows the path type's platform-default comparer.</remarks>
    bool IsPrefixOf(TPath other);

    /// <summary>
    /// Determines whether the current path starts with the specified path.
    /// </summary>
    /// <param name="other">The path to compare to the current path.</param>
    /// <remarks>
    /// Checks for a non-strict prefix: if the paths are equal, then each still starts with the other, so every path
    /// starts with itself.
    /// </remarks>
    /// <remarks>
    /// The comparison is performed over whole path <b>segments</b>: <c>/foo1</c> does <b>not</b> start with
    /// <c>/foo</c>, even though one path string starts with the other.
    /// </remarks>
    /// <remarks>
    /// This is the exact inverse of <see cref="IsPrefixOf"/>: <c>a.StartsWith(b)</c> means the same as
    /// <c>b.IsPrefixOf(a)</c>.
    /// </remarks>
    /// <remarks>Case sensitivity follows the path type's platform-default comparer.</remarks>
    bool StartsWith(TPath other);

#if NET8_0_OR_GREATER
    /// <summary>
    /// Creates a new path instance of type <typeparamref name="TPath"/> from the specified string value.
    /// </summary>
    /// <param name="value">The string representation of the path to create.</param>
    /// <returns>A new instance of <typeparamref name="TPath"/> representing the specified path.</returns>
    static abstract TPath Create(string value);
#endif
}
