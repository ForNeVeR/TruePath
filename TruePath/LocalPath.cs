// SPDX-FileCopyrightText: 2024-2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;
using TruePath.Comparers;

namespace TruePath;

/// <summary>
/// <para>A path pointing to a place in the local file system.</para>
/// <para>It may be either absolute or relative.</para>
/// <para>
/// An <b>empty</b> path designates the <b>current directory</b>. Normalizing an empty string, <c>.</c> or
/// <c>a/..</c> produces it, and so does <see cref="Parent"/> of a relative path consisting of a single segment
/// (e.g. the parent of <c>foo</c>).
/// </para>
/// <para>
/// Always stored in a normalized form. Read the documentation on <see cref="TruePath.PathStrings.Normalize(string)"/>
/// to know what form of normalization the path uses.
/// </para>
/// </summary>
public readonly struct LocalPath(string value) : IEquatable<LocalPath>, IComparable<LocalPath>, IPath, IPath<LocalPath>
{
    /// <summary>
    /// <para>Provides a default comparer for comparing file paths, aware of the current platform.</para>
    /// <para>
    /// On <b>Windows</b> and <b>macOS</b>, this will perform <b>case-insensitive</b> string comparison, since the
    /// file systems are case-insensitive on these operating systems by default.
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

    private static bool StartsWithParentDirectoryReference(ReadOnlySpan<char> value) =>
        value.Length >= 2 && value[0] == '.' && value[1] == '.'
        && (value.Length == 2 || value[2] == Separator);

    /// <inheritdoc cref="IPath.Value"/>
    public string Value { get; } = PathStrings.Normalize(value);

    /// <summary>Determines the kind of this path. See <see cref="PathKind"/> for the details.</summary>
    public PathKind Kind
    {
        get
        {
            if (Value.Length == 0) return PathKind.Relative;
            if (!IsDriveBasedSystem) return Value[0] == Separator ? PathKind.Absolute : PathKind.Relative;

            if (HasDriveLetter)
            {
                return Value.Length > 2 && Value[2] == Separator
                    ? PathKind.Absolute
                    : PathKind.DriveCurrentDirectoryRelative;
            }

            return Value[0] == Separator ? PathKind.DriveRootRelative : PathKind.Relative;
        }
    }

    /// <summary>
    /// <para>
    /// Checks whether the path is absolute, i.e. it is fully qualified, and doesn't depend on the current directory,
    /// or current directory on any drive (Windows-specific).
    /// </para>
    /// <para>
    /// On Windows, this requires a drive letter and a root directory: <c>C:\Windows</c> is absolute, while
    /// <c>\Windows</c> and <c>C:Windows</c> are not.
    /// </para>
    /// </summary>
    /// <remarks>Equivalent to checking that <see cref="Kind"/> is <see cref="PathKind.Absolute"/>.</remarks>
    public bool IsAbsolute => Kind == PathKind.Absolute;

    private static readonly bool IsDriveBasedSystem = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    private bool HasDriveLetter => IsDriveBasedSystem && PathStrings.SourceContainsDriveLetter(Value.AsSpan());

    /// <summary>
    /// <para>Gets the root of this path, if it can be determined without resolving against the current directory.</para>
    /// <list type="bullet">
    ///     <item>
    ///         For an absolute path, this is its root: <c>C:\</c> for <c>C:\foo</c> on Windows, <c>/</c> for
    ///         <c>/foo</c> on Unix.
    ///     </item>
    ///     <item>
    ///         For a drive-relative path on Windows (e.g. <c>C:foo</c> or <c>C:</c>), this is the root of that drive
    ///         (<c>C:\</c>).
    ///     </item>
    ///     <item>
    ///         For a relative path, or a path rooted without a drive letter on Windows (e.g. <c>\foo</c>), this is
    ///         <see langword="null"/>.
    ///     </item>
    /// </list>
    /// </summary>
    public AbsolutePath? PathRoot => Kind switch
    {
        PathKind.Absolute => new AbsolutePath(Path.GetPathRoot(Value)!, checkAbsoluteness: false),
        // The root of a drive-relative path (C:foo) is still the drive root.
        PathKind.DriveCurrentDirectoryRelative => new AbsolutePath(Value.Substring(0, 2) + Separator, checkAbsoluteness: false),
        // A path rooted without a drive (\foo) has its root depending on the current drive.
        _ => null
    };

    /// <inheritdoc cref="IPath.Parent"/>
    /// <remarks>
    /// The parent of a relative path consisting of a single segment is the <b>empty</b> path (the current
    /// directory), not <see langword="null"/>. Similarly, on Windows, the parent of a bare drive (<c>C:</c>, the
    /// current directory of that drive) is <c>C:..</c>.
    /// </remarks>
    public LocalPath? Parent
    {
        get
        {
            // For C:foo, the rest after the drive letter is a relative path, and follows the same rules.
            var relativePart = Kind == PathKind.DriveCurrentDirectoryRelative ? Value[2..] : Value;
            if (relativePart == "" || relativePart == ".." || relativePart.EndsWith($"{Separator}.."))
                return this / "..";
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
    /// An <b>empty</b> path designates the current directory. It is a prefix of every relative path that stays
    /// at or below that directory, which means every relative path that does not begin with a <c>..</c>
    /// reference.
    /// </para>
    /// <para>
    /// Paths of different <see cref="Kind"/>s are never prefixes of each other: e.g. an absolute path is never a
    /// prefix of a relative one, and vice versa. Such a comparison would require resolving a path against the
    /// current directory, which this type never does.
    /// </para>
    /// <para>
    /// On Windows, paths relative to the current directory of a drive (such as <c>C:Windows</c>) are only related
    /// if they have the same drive letter. A bare drive (<c>C:</c>) designates that drive's current directory, and
    /// behaves the same way as the empty path does for relative paths.
    /// </para>
    /// <para>
    /// Note that <c>C:\</c> will <i>not</i> be considered as a prefix of <c>C:Folder</c>, even though they
    /// might be considered as related.
    /// </para>
    /// </remarks>
    public bool IsPrefixOf(LocalPath other)
    {
        var kind = Kind;
        if (kind != other.Kind) return false;

        var prefix = Value.AsSpan();
        var path = other.Value.AsSpan();
        if (kind == PathKind.DriveCurrentDirectoryRelative)
        {
            // The rest of the paths after the drive letter are relative to the drive's current directory, and
            // compare the same way the relative paths do.
            if (!IsSameDrive(Value, other.Value)) return false;
            prefix = prefix.Slice(2);
            path = path.Slice(2);
        }

        return IsSegmentPrefix(prefix, path);
    }

    private static bool IsSegmentPrefix(ReadOnlySpan<char> prefix, ReadOnlySpan<char> path)
    {
        // The empty path is the current directory, so every path at or below it has it as a prefix - but one
        // starting with a ".." reference points outside it. Normalization only ever keeps such references at the
        // very start of a path, so testing the first segment is enough.
        if (prefix.Length == 0) return !StartsWithParentDirectoryReference(path);

        if (!path.StartsWith(prefix, PlatformDefaultPathComparer<LocalPath>.DefaultStringComparison))
            return false;
        return path.Length == prefix.Length ||
               prefix[^1] == Separator ||
               path[prefix.Length] == Separator;
    }

    private static bool IsSameDrive(string a, string b) => char.ToUpperInvariant(a[0]) == char.ToUpperInvariant(b[0]);

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
    /// <param name="basePath">The path to append to.</param>
    /// <param name="b">The path to append.</param>
    /// <returns>The combined path, normalized.</returns>
    /// <remarks>
    /// <para>
    /// The result designates the same location as changing the current directory first to
    /// <paramref name="basePath"/>, and then to <paramref name="b"/>: <c>a / b</c> means the same as
    /// <c>cd /d a &amp;&amp; cd /d b</c> on Windows, or <c>cd a &amp;&amp; cd b</c> on Unix. A base path without a
    /// drive letter is considered to be on a different drive than any drive letter in <paramref name="b"/>.
    /// </para>
    /// <para>Depending on the <see cref="Kind"/> of <paramref name="b"/>:</para>
    /// <list type="bullet">
    ///     <item>
    ///         <see cref="PathKind.Absolute"/> (<c>D:\x</c>, <c>/x</c>): <paramref name="b"/> replaces the base path.
    ///     </item>
    ///     <item>
    ///         <see cref="PathKind.Relative"/> (<c>x</c>): <paramref name="b"/> is appended after a separator, so
    ///         <c>C:\base / x</c> is <c>C:\base\x</c>. No separator is added after a bare drive: <c>C: / x</c> is
    ///         <c>C:x</c>. Appending an empty path returns the base path unchanged, and appending to an empty path
    ///         returns <paramref name="b"/>.
    ///     </item>
    ///     <item>
    ///         <see cref="PathKind.DriveRootRelative"/> (<c>\x</c>): the drive of the base path is kept, if there is
    ///         one. <c>C:\base / \x</c> and <c>C:base / \x</c> are both <c>C:\x</c>, while <c>base / \x</c> is
    ///         <c>\x</c>.
    ///     </item>
    ///     <item>
    ///         <see cref="PathKind.DriveCurrentDirectoryRelative"/> (<c>C:x</c>): if the base path is on the same
    ///         drive, the rest of <paramref name="b"/> is appended as a relative path, so <c>C:\base / C:x</c> is
    ///         <c>C:\base\x</c>. Otherwise, <paramref name="b"/> replaces the base path: <c>C:\base / D:x</c> is
    ///         <c>D:x</c>, and <c>base / C:x</c> is <c>C:x</c>. Drive letters are compared case-insensitively.
    ///     </item>
    /// </list>
    /// <para>
    /// This is the algorithm also defined in C++'s <c>std::filesystem::path::operator/</c>, and that the result is
    /// normalized (e.g. <c>C:\base / ""</c> gets no trailing separator after normalization).
    /// </para>
    /// <para>
    /// <b>Important:</b> this operator differs from <see cref="Path.Combine(string, string)"/>, which returns its
    /// second argument if that is rooted, and otherwise joins the arguments with a separator:
    /// </para>
    /// <list type="bullet">
    ///     <item><c>C:\base / \x</c> is <c>C:\x</c>, while <c>Path.Combine</c> returns <c>\x</c>;</item>
    ///     <item><c>C:\base / C:x</c> is <c>C:\base\x</c>, while <c>Path.Combine</c> returns <c>C:x</c>;</item>
    ///     <item>
    ///     <c>C: / x</c> is <c>C:x</c>, while <c>Path.Combine</c> returns <c>C:\x</c> on .NET 10 (.NET Framework
    ///     4.8.1, though, will return <c>C:x</c>);
    ///     </item>
    ///     <item>the result is normalized.</item>
    /// </list>
    /// <para>On Unix, the result is the same as the one of <c>Path.Combine</c>, up to normalization.</para>
    /// </remarks>
    /// <seealso href="https://eel.is/c++draft/fs.path.append">C++ standard: path appends (fs.path.append)</seealso>
    public static LocalPath operator /(LocalPath basePath, LocalPath b) => new(Append(basePath, b));

    /// <inheritdoc cref="op_Division(LocalPath, LocalPath)"/>
    public static LocalPath operator /(LocalPath basePath, string b) => basePath / new LocalPath(b);

    private static string Append(LocalPath basePath, LocalPath b)
    {
        switch (b.Kind)
        {
            case PathKind.Absolute:
                return b.Value;
            case PathKind.DriveCurrentDirectoryRelative:
                return basePath.HasDriveLetter && IsSameDrive(basePath.Value, b.Value)
                    ? Join(basePath.Value, b.Value.Substring(2))
                    : b.Value;
            case PathKind.DriveRootRelative:
                return basePath.HasDriveLetter ? basePath.Value.Substring(0, 2) + b.Value : b.Value;
            default:
                return Join(basePath.Value, b.Value);
        }
    }

    private static string Join(string basePath, string relativePath)
    {
        if (relativePath.Length == 0) return basePath;
        if (basePath.Length == 0) return relativePath;

        // A bare drive (C:) designates the current directory of the drive, and C:x is a path relative to it.
        var isBareDrive = basePath.Length == 2 && IsDriveBasedSystem
                          && PathStrings.SourceContainsDriveLetter(basePath.AsSpan());
        return isBareDrive || basePath[^1] == Separator
            ? basePath + relativePath
            : basePath + Separator + relativePath;
    }

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
    /// <para>
    /// Note that if this path is already absolute, it will just transform to <see cref="AbsolutePath"/>. The current
    /// directory won't matter for such a case.
    /// </para>
    /// <para>
    /// On Windows, a path rooted without a drive letter (<c>\x</c>) is resolved against the drive of the current
    /// directory, and a path relative to the current directory of a drive (<c>D:x</c>) is resolved against the
    /// current directory of that drive, as tracked by the process (see <see cref="Path.GetFullPath(string)"/>).
    /// </para>
    /// </remarks>
    public AbsolutePath ResolveToCurrentDirectory() => Kind == PathKind.DriveCurrentDirectoryRelative
        ? new AbsolutePath(Path.GetFullPath(Value))
        : AbsolutePath.CurrentWorkingDirectory / this;

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
