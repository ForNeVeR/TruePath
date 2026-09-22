// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using TruePath.Comparers;

namespace TruePath;

/// <summary>
/// <para>A path pointing to a place in the local file system.</para>
/// <para>It may be either absolute or relative.</para>
/// <para>
///     An <b>empty</b> path designates the <b>current directory</b>. Normalizing an empty string, <c>.</c> or
///     <c>a/..</c> produces it, and so does <see cref="Parent"/> of a relative path consisting of a single segment
///     (e.g. the parent of <c>foo</c>).
/// </para>
/// <para>
///     Always stored in a normalized form. Read the documentation on <see cref="TruePath.PathStrings.Normalize(string)"/>
///     to know what form of normalization the path uses.
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
    public static readonly IPathComparer<LocalPath> PlatformDefaultComparer =
        new PlatformDefaultPathComparer<LocalPath>();

    /// <summary>
    /// A strict comparer for comparing file paths using ordinal, case-sensitive comparison of the underlying path
    /// strings.
    /// </summary>
    public static readonly IPathComparer<LocalPath> StrictStringComparer =
        new StrictStringPathComparer<LocalPath>();

    private static char Separator => Path.DirectorySeparatorChar;

    private static bool StartsWithParentDirectoryReference(string value) =>
        value.Length >= 2 && value[0] == '.' && value[1] == '.'
        && (value.Length == 2 || value[2] == Separator);

    /// <inheritdoc cref="IPath.Value"/>
    public string Value { get; } = PathStrings.Normalize(value);

    /// <summary>
    /// <para>Checks whether the path is absolute.</para>
    /// <para>
    ///     Currently, any rooted paths are considered absolute, but this is subject to change: on Windows, there
    ///     will be an additional requirement for a path to be either a DOS device path or start from a disk letter.
    /// </para>
    /// </summary>
    // TODO[#224]: narrowing this to true absolute paths (kind 1 in the taxonomy at IsPrefixOf) requires updating
    // IsPrefixOf in the same change: it relies on this property to tell rooted paths from relative ones.
    public bool IsAbsolute => Path.IsPathRooted(Value);

    /// <inheritdoc cref="IPath.Parent"/>
    /// <remarks>
    /// The parent of a relative path consisting of a single segment is the <b>empty</b> path (the current
    /// directory), not <see langword="null"/>.
    /// </remarks>
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
    /// <remarks>Uses <see cref="PlatformDefaultComparer"/> for platform-default case sensitivity.</remarks>
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
    public bool StartsWith(LocalPath other) => other.IsPrefixOf(this);

    /// <summary>
    /// Creates a new <see cref="LocalPath"/> instance from the specified string value.
    /// </summary>
    /// <param name="value">The string representation of the path to create.</param>
    /// <returns>A new normalized <see cref="LocalPath"/> instance representing the specified path.</returns>
    public static LocalPath Create(string value) => new(value);

    /// <inheritdoc cref="IPath{TPath}.IsPrefixOf(TPath)"/>
    /// <remarks>
    /// <para>
    ///     An <b>empty</b> path designates the current directory. It is a prefix of every relative path that stays
    ///     at or below that directory, which means every relative path that does not begin with a <c>..</c>
    ///     reference.
    /// </para>
    /// <para>
    ///     An <b>absolute</b> path is never a prefix of a <b>relative</b> one, and vice versa: such a comparison
    ///     would require resolving the relative path against the current directory, which this type never does. Any
    ///     pair of paths differing in <see cref="IsAbsolute"/> is reported as unrelated.
    /// </para>
    /// </remarks>
    public bool IsPrefixOf(LocalPath other)
    {
        // TODO[#224]: IsAbsolute is Path.IsPathRooted, which is too coarse for this algorithm. On Windows there are
        // really four kinds of path, and no path of one kind should ever be considered a prefix of a path of another:
        //   1. true absolute:   C:\Windows
        //   2. rooted diskless: \Windows
        //   3. current on disk: C: (and C:Windows, relative to the current directory of drive C:)
        //   4. true relative:   Windows, ..\Windows
        // AbsolutePath exists to cover kind 1 only, while LocalPath is applicable to all four. IsPathRooted answers
        // true for kinds 1, 2 and 3 alike, so the check below only separates {1, 2, 3} from {4}. When IsAbsolute is
        // eventually narrowed to kind 1 - as its own documentation anticipates - this comparison must not simply
        // follow it: it needs the full four-way distinction. The path kind should then be extracted into a separate
        // field or property and matched on here.
        if (IsAbsolute != other.IsAbsolute) return false;

        // The empty path is the current directory, so every path at or below it has it as a prefix - but one
        // starting with a ".." reference points outside it. Normalization only ever keeps such references at the
        // very start of a path, so testing the first segment is enough.
        if (Value.Length == 0) return !StartsWithParentDirectoryReference(other.Value);

        if (!(Value.Length <= other.Value.Length &&
              other.Value.StartsWith(Value, PlatformDefaultPathComparer<LocalPath>.DefaultStringComparison)))
            return false;
        return other.Value.Length == Value.Length ||
               Value[Value.Length - 1] == Separator ||
               other.Value[Value.Length] == Separator;
    }

    /// <summary>
    /// Calculates the relative path from a base path to this path.
    /// </summary>
    /// <param name="basePath">The base path from which to calculate the relative path.</param>
    /// <returns>The relative path from the base path to this path.</returns>
#if NET8_0_OR_GREATER
    public LocalPath RelativeTo(LocalPath basePath) => new(Path.GetRelativePath(basePath.Value, Value));
#else
    public LocalPath RelativeTo(LocalPath basePath) => new(PathEx.GetRelativePath(basePath.Value, Value));
#endif
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
    /// </returns>
    public int CompareTo(LocalPath other) => PlatformDefaultComparer.Compare(this, other);
}
