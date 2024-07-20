// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath;

/// <summary>
/// <para>A path pointing to a place in the local file system.</para>
/// <para>It may be either absolute or relative.</para>
/// <para>
///     Always stored in a normalized form. Read the documentation on <see cref="TruePath.PathStrings.Normalize"/> to
///     know what form of normalization does the path use.
/// </para>
/// </summary>
public readonly struct LocalPath(string value) : IEquatable<LocalPath>, IPath, IPath<LocalPath>
{
    /// <inheritdoc cref="IPath.Value"/>
    public string Value { get; } = PathStrings.Normalize(value);

    /// <summary>
    /// <para>Checks whether th path is absolute.</para>
    /// <para>
    ///     Currently, any rooted paths are considered absolute, but this is a subject to change: on Windows, there
    ///     will be an additional requirement for a path to be either a DOS device path or start from a disk letter.
    /// </para>
    /// </summary>
    public bool IsAbsolute => Path.IsPathRooted(Value);

    /// <inheritdoc cref="IPath.Parent"/>
    public LocalPath? Parent {
        get
        {
            var value = PathStrings.ResolveRelativePaths(Value);
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return Path.GetDirectoryName(Value) is { } parent ? new(parent) : null;
        }
    } 

    /// <inheritdoc cref="IPath.Parent"/>
    IPath? IPath.Parent => Parent;

    /// <inheritdoc cref="IPath.FileName"/>
    public string FileName => Path.GetFileName(Value);

    /// <returns>The normalized path string contained in this object.</returns>
    public override string ToString() => Value;

    /// <summary>Compares the path with another.</summary>
    /// <remarks>Note that currently this comparison is case-sensitive.</remarks>
    public bool Equals(LocalPath other)
    {
        return Value == other.Value;
    }

    /// <inheritdoc cref="Equals(LocalPath)"/>
    public override bool Equals(object? obj)
    {
        return obj is LocalPath other && Equals(other);
    }

    /// <inheritdoc cref="Object.GetHashCode"/>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    /// <inheritdoc cref="Equals(LocalPath)"/>
    public static bool operator ==(LocalPath left, LocalPath right)
    {
        return left.Equals(right);
    }

    /// <inheritdoc cref="Equals(LocalPath)"/>
    public static bool operator !=(LocalPath left, LocalPath right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc cref="IPath{TPath}.StartsWith(TPath)"/>
    public bool StartsWith(LocalPath other) => Value.StartsWith(other.Value);

    /// <inheritdoc cref="IPath{TPath}.IsPrefixOf(TPath)"/>
    public bool IsPrefixOf(LocalPath other)
    {
        if (!(Value.Length <= other.Value.Length && other.Value.StartsWith(Value))) return false;
        return other.Value.Length == Value.Length || other.Value[Value.Length] == Path.DirectorySeparatorChar;
    }

    /// <summary>
    /// Calculates the relative path from a base path to this path.
    /// </summary>
    /// <param name="basePath">The base path from which to calculate the relative path.</param>
    /// <returns>The relative path from the base path to this path.</returns>
    public LocalPath RelativeTo(LocalPath basePath) => new(Path.GetRelativePath(basePath.Value, Value));

    // ReSharper disable once GrammarMistakeInComment // RIDER-111735
    /// <summary>Appends another path to this one.</summary>
    /// <remarks>
    /// Note that in case path <paramref name="b"/> is <b>absolute</b>, it will completely take over and the
    /// <paramref name="basePath"/> will be ignored.
    /// </remarks>
    public static LocalPath operator /(LocalPath basePath, LocalPath b) =>
        new(Path.Combine(basePath.Value, b.Value));

    // ReSharper disable once GrammarMistakeInComment // RIDER-111735
    /// <summary>Appends another path to this one.</summary>
    /// <remarks>
    /// Note that in case path <paramref name="b"/> is <b>absolute</b>, it will completely take over and the
    /// <paramref name="basePath"/> will be ignored.
    /// </remarks>
    public static LocalPath operator /(LocalPath basePath, string b) => basePath / new LocalPath(b);

    /// <summary>
    /// Implicitly converts an <see cref="AbsolutePath"/> to a <see cref="LocalPath"/>.
    /// </summary>
    /// <remarks>Note that this conversion doesn't lose any information.</remarks>
    public static implicit operator LocalPath(AbsolutePath path) => path.Underlying;

    // ReSharper disable once GrammarMistakeInComment // RIDER-111735
    /// <summary>Converts an <see cref="AbsolutePath"/> to a <see cref="LocalPath"/>.</summary>
    public LocalPath(AbsolutePath path) : this(path.Value)
    {
    }
}
