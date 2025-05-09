// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using TruePath.Comparers;

namespace TruePath;

/// <summary>
/// <para>A path pointing to a place in the local file system.</para>
/// <para>It may be either absolute or relative.</para>
/// <para>
///     Always stored in a normalized form. Read the documentation on <see cref="TruePath.PathStrings.Normalize"/> to
///     know what form of normalization the path uses.
/// </para>
/// </summary>
public readonly struct LocalPath(string value) : IEquatable<LocalPath>, IComparable<LocalPath>, IPath, IPath<LocalPath>
{
    /// <summary>
    /// <para>Provides a default comparer for comparing file paths, aware of the current platform.</para>
    /// <para>
    ///     On <b>Windows</b> and <b>macOS</b>, this will perform <b>case-insensitive</b> string comparison, since the
    ///     file systems are case-insensitive on these operating systems by default.
    /// </para>
    /// <para>On <b>Linux</b>, the comparison will be <b>case-sensitive</b>.</para>
    /// </summary>
    /// <remarks>
    /// Note that this comparison <b>does not guarantee correctness</b>: in practice, on any platform to control
    /// case-sensitiveness of either the whole file system or a part of it. This class does not take this into account,
    /// having a benefit of no accessing the file system for any of the comparisons.
    /// </remarks>
    public static readonly PathComparer<LocalPath> PlatformDefaultComparer =
        new PlatformDefaultPathComparer<LocalPath>();

    /// <summary>
    /// A strict comparer for comparing file paths using ordinal, case-sensitive comparison of the underlying path
    /// strings.
    /// </summary>
    public static readonly PathComparer<LocalPath> StrictStringComparer =
        new StrictStringPathComparer<LocalPath>();

    private static char Separator => Path.DirectorySeparatorChar;

    /// <inheritdoc cref="IPath.Value"/>
    public string Value { get; } = PathStrings.Normalize(value);

    /// <summary>
    /// <para>Checks whether the path is absolute.</para>
    /// <para>
    ///     Currently, any rooted paths are considered absolute, but this is subject to change: on Windows, there
    ///     will be an additional requirement for a path to be either a DOS device path or start from a disk letter.
    /// </para>
    /// </summary>
    public bool IsAbsolute => Path.IsPathRooted(Value);

    /// <inheritdoc cref="IPath.Parent"/>
    public LocalPath? Parent
    {
        get
        {
            if (Value == "" || Value == ".." || Value.EndsWith($"{Separator}..")) return this / "..";
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
    public bool Equals(LocalPath other) => Equals(other, PlatformDefaultComparer);

    /// <summary>
    /// Determines whether the specified <see cref="LocalPath"/> is equal to the current <see cref="LocalPath"/> using the specified string comparer.
    /// </summary>
    /// <param name="other">The <see cref="LocalPath"/> to compare with the current <see cref="LocalPath"/>.</param>
    /// <param name="comparer">The comparer to use for comparing the paths.</param>
    /// <returns>
    /// <see langword="true"/> if the specified <see cref="LocalPath"/> is equal to the current <see cref="LocalPath"/> using the specified string comparer; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(LocalPath other, IEqualityComparer<LocalPath> comparer) => comparer.Equals(this, other);

    /// <inheritdoc cref="Equals(LocalPath)"/>
    public override bool Equals(object? obj)
    {
        return obj is LocalPath other && Equals(other);
    }

    /// <inheritdoc cref="Object.GetHashCode"/>
    public override int GetHashCode() => PlatformDefaultComparer.GetHashCode(this);

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
        return other.Value.Length == Value.Length || other.Value[Value.Length] == Separator;
    }

    /// <summary>
    /// Calculates the relative path from a base path to this path.
    /// </summary>
    /// <param name="basePath">The base path from which to calculate the relative path.</param>
    /// <returns>The relative path from the base path to this path.</returns>
    public LocalPath RelativeTo(LocalPath basePath) => new(Path.GetRelativePath(basePath.Value, Value));

    /// <summary>Appends another path to this one.</summary>
    /// <remarks>
    /// Note that in case path <paramref name="b"/> is <b>absolute</b>, it will completely take over and the
    /// <paramref name="basePath"/> will be ignored.
    /// </remarks>
    public static LocalPath operator /(LocalPath basePath, LocalPath b) =>
        new(Path.Combine(basePath.Value, b.Value));

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

    /// <summary>
    /// Resolves this path to an absolute path based on the current working directory.
    /// </summary>
    /// <returns>An <see cref="AbsolutePath"/> that represents this path resolved against the current working directory.</returns>
    /// <remarks>
    /// Note that if this path is already absolute, it will just transform to <see cref="AbsolutePath"/>. The current
    /// directory won't matter for such a case.
    /// </remarks>
    public AbsolutePath ResolveToCurrentDirectory() => AbsolutePath.CurrentWorkingDirectory / this;

    /// <summary>Converts an <see cref="AbsolutePath"/> to a <see cref="LocalPath"/>.</summary>
    public LocalPath(AbsolutePath path) : this(path.Value)
    {
    }

    /// <summary>
    /// Compares the current <see cref="LocalPath"/> instance with another <see cref="LocalPath"/> instance,
    /// using the default platform-aware comparison rules provided by <see cref="PlatformDefaultPathComparer{TPath}"/>.
    /// </summary>
    /// <param name="other">The <see cref="LocalPath"/> instance to compare with the current instance.</param>
    /// <returns>
    /// <para>A signed integer that indicates the relative order of the compared objects.</para>
    /// <list type="table">
    ///     <listheader>
    ///         <term>Value</term>
    ///         <description>Meaning</description>
    ///     </listheader>
    ///     <item>
    ///         <term>Less than zero</term>
    ///         <description>The current instance precedes <paramref name="other"/> in the sort order.</description>
    ///     </item>
    ///     <item>
    ///         <term>Zero</term>
    ///         <description>The current instance occurs in the same position in the sort order as <paramref name="other"/>.</description>
    ///     </item>
    ///     <item>
    ///         <term>Greater than zero</term>
    ///         <description>The current instance follows <paramref name="other"/> in the sort order.</description>
    ///     </item>
    /// </list>
    public int CompareTo(LocalPath other) => PlatformDefaultComparer.Compare(this, other);
}
