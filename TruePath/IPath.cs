// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath;

/// <summary>Represents a path in a file system.</summary>
public interface IPath
{
    /// <summary>The normalized path string.</summary>
    string Value { get; }

    /// <summary>The name of the last component of this path.</summary>
    string FileName { get; }

    /// <summary>
    /// The parent of this path. Will be <c>null</c> for a rooted absolute path, or relative path pointing to the
    /// current directory.
    /// </summary>
    IPath? Parent { get; }
}

/// <summary>Represents a path in a file system. Allows generic operators to be applied.</summary>
/// <typeparam name="TPath">The type of this path.</typeparam>
public interface IPath<TPath> where TPath : IPath<TPath>
{
    // ReSharper disable once GrammarMistakeInComment // RIDER-111735
    /// <summary>Appends another path to this one.</summary>
    /// <remarks>
    /// Note that in case path <paramref name="appended"/> is <b>absolute</b>, it will completely take over and the
    /// <paramref name="basePath"/> will be ignored.
    /// </remarks>
    static abstract TPath operator /(TPath basePath, LocalPath appended);

    /// <inheritdoc cref="op_Division(TPath,LocalPath)"/>
    static abstract TPath operator /(TPath basePath, string appended);
}
