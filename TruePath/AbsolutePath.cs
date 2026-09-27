// SPDX-FileCopyrightText: 2024-2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;
using TruePath.Comparers;
#if !NET8_0_OR_GREATER
using TruePath.Polyfills;
#endif

namespace TruePath;

/// <summary>
/// This is a path on the local system that's guaranteed to be <b>absolute</b>: that is, path that is rooted and has a
/// disk letter (on Windows).
/// </summary>
/// <remarks>
/// <para>For a path that's not guaranteed to be absolute, use the <see cref="LocalPath"/> type.</para>
/// <para>
/// Uninitialized values of this structure (e.g. <c>default(AbsolutePath)</c>) will throw
/// <see cref="NullReferenceException"/> from many of the APIs.
/// </para>
/// </remarks>
public readonly struct AbsolutePath : IEquatable<AbsolutePath>, IComparable<AbsolutePath>, IPath, IPath<AbsolutePath>
{
    /// <summary>
    /// <para>Provides a default comparer for comparing file paths, aware of the current platform.</para>
    /// <para>
    /// On <b>Windows</b>, <b>macOS</b>, <b>iOS</b> and <b>tvOS</b>, this will perform <b>case-insensitive</b> string
    /// comparison, since the file systems are case-insensitive on these operating systems by default.
    /// </para>
    /// <para>On <b>Linux</b>, the comparison will be <b>case-sensitive</b>.</para>
    /// </summary>
    /// <remarks>
    /// Note that this comparison <b>does not guarantee correctness</b>: in practice, on any platform to control
    /// case-sensitiveness of either the whole file system or a part of it. This class does not take this into account,
    /// having a benefit of no accessing the file system for any of the comparisons.
    /// </remarks>
    public static readonly IPathComparer<AbsolutePath> PlatformDefaultComparer =
        new PlatformDefaultPathComparer<AbsolutePath>();

    /// <summary>
    /// A strict comparer for comparing file paths using ordinal, case-sensitive comparison of the underlying path
    /// strings.
    /// </summary>
    public static readonly IPathComparer<AbsolutePath> StrictStringComparer =
        new StrictStringPathComparer<AbsolutePath>();

    internal readonly LocalPath Underlying;

    /// <summary>
    /// Creates an <see cref="AbsolutePath"/> instance by normalizing the path from the passed string according to the
    /// rules stated in <see cref="LocalPath"/>.
    /// </summary>
    /// <param name="value">Path string to normalize.</param>
    /// <param name="checkAbsoluteness">Flag indicating whether absoluteness of path should be checked</param>
    /// <exception cref="ArgumentException">Thrown if the passed string does not represent an absolute path.</exception>>
    internal AbsolutePath(string value, bool checkAbsoluteness)
    {
        Underlying = new LocalPath(value);

        if (checkAbsoluteness && Underlying.IsAbsolute is false)
            throw new ArgumentException($"Path \"{value}\" is not absolute.");
    }

    /// <summary>
    /// Creates an <see cref="AbsolutePath"/> instance by normalizing the path from the passed string according to the
    /// rules stated in <see cref="LocalPath"/>.
    /// </summary>
    /// <param name="value">Path string to normalize.</param>
    /// <exception cref="ArgumentException">Thrown if the passed string does not represent an absolute path.</exception>
    public AbsolutePath(string value) : this(value, checkAbsoluteness: true) { }

    /// <summary>
    /// Creates an <see cref="AbsolutePath"/> instance by converting a <paramref name="localPath"/> object.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the passed path is not absolute.</exception>
    public AbsolutePath(LocalPath localPath) : this(localPath.Value, checkAbsoluteness: true) { }

    /// <inheritdoc cref="IPath.Value"/>
    public string Value => Underlying.Value;

    /// <inheritdoc cref="IPath.Parent"/>
    public AbsolutePath? Parent => Underlying.Parent is { } path ? new(path.Value, checkAbsoluteness: false) : null;

    /// <summary>Gets the root of this path: e.g. <c>C:\</c> on Windows or <c>/</c> on Unix.</summary>
    public AbsolutePath PathRoot => Underlying.PathRoot!.Value;

    /// <inheritdoc cref="IPath.Parent"/>
    IPath? IPath.Parent => Parent;

    /// <inheritdoc cref="IPath.FileName"/>
    public string FileName => Underlying.FileName;

    /// <inheritdoc cref="IPath{TPath}.StartsWith(TPath)"/>
    public bool StartsWith(AbsolutePath other) => other.IsPrefixOf(this);

    /// <summary>
    /// Creates a new path instance of type <see cref="AbsolutePath" /> from the specified string value.
    /// </summary>
    /// <param name="value">The string representation of the path to create.</param>
    /// <returns>A new instance of <see cref="AbsolutePath" /> representing the specified path.</returns>
    public static AbsolutePath Create(string value) => new(value);

    /// <inheritdoc cref="IPath{TPath}.IsPrefixOf(TPath)"/>
    public bool IsPrefixOf(AbsolutePath other) => Underlying.IsPrefixOf(other.Underlying);

    /// <summary>Gets or sets the current working directory as an AbsolutePath instance.</summary>
    /// <value>The current working directory.</value>
    public static AbsolutePath CurrentWorkingDirectory
    {
        get => new(Environment.CurrentDirectory);
        set => Directory.SetCurrentDirectory(value.Value);
    }

    /// <summary>
    /// Calculates the relative path from a base path to this path.
    /// </summary>
    /// <param name="basePath">The base path from which to calculate the relative path.</param>
    /// <returns>
    /// The relative path from the base path to this path, or this path itself if the paths have different roots.
    /// </returns>
    /// <remarks>
    /// If the paths have different roots (on Windows, e.g. paths on different drives), there's no relative path
    /// between them, and this path is returned unchanged: <c>D:\x</c> relative to <c>C:\y</c> is <c>D:\x</c>. On
    /// Unix, all paths share the same root, so this never happens.
    /// </remarks>
#if NET8_0_OR_GREATER
    public LocalPath RelativeTo(AbsolutePath basePath) => new(Path.GetRelativePath(basePath.Value, Value));
#else
    public LocalPath RelativeTo(AbsolutePath basePath) => new(PathPolyfills.GetRelativePath(basePath.Value, Value));
#endif
    /// <summary>Corrects the file name case on case-insensitive file systems, resolves symlinks.</summary>
    public AbsolutePath Canonicalize() => new(DiskUtils.GetRealPath(Value));

    /// <summary>
    /// <para>
    /// Works the same way as <see cref="LocalPath.op_Division(LocalPath, LocalPath)"/> (read its documentation for
    /// the details, including the differences from <see cref="Path.Combine(string, string)"/>), except that the
    /// result is always absolute: a path relative to the current directory of another drive gets resolved (see the
    /// remarks).
    /// </para>
    /// <para>
    /// The result designates the same location as changing the current directory first to
    /// <paramref name="basePath"/>, and then to <paramref name="b"/>: <c>a / b</c> means the same as
    /// <c>cd /d a &amp;&amp; cd /d b</c> on Windows, or <c>cd a &amp;&amp; cd b</c> on Unix. This is the algorithm
    /// of C++'s <c>std::filesystem::path::operator/</c>, except that the result is normalized, and that a path
    /// relative to the current directory of another drive gets resolved.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// On Windows, a path relative to the current directory of <b>another</b> drive is resolved against the current
    /// directory of that drive, as tracked by the process (see <see cref="Path.GetFullPath(string)"/>), or against the
    /// root of that drive if the process doesn't track one. E.g. <c>C:\base / D:x</c> is <c>D:\x</c> if the current
    /// directory of drive <c>D:</c> is its root. This is the only case when the result depends on the state of the
    /// process: <see cref="LocalPath.op_Division(LocalPath, LocalPath)"/> returns <c>D:x</c> here.
    /// </para>
    /// <para>
    /// On the same drive, such a path is resolved against the base path: <c>C:\base / C:x</c> is <c>C:\base\x</c>. A
    /// path rooted without a drive letter keeps the drive of the base path: <c>C:\base / \x</c> is <c>C:\x</c>.
    /// </para>
    /// </remarks>
    /// <seealso href="https://eel.is/c++draft/fs.path.append">C++ standard: path appends (fs.path.append)</seealso>
    public static AbsolutePath operator /(AbsolutePath basePath, LocalPath b)
    {
        var result = basePath.Underlying / b;
        // Only a path relative to the current directory of another drive is left non-absolute: C:\base / D:x is D:x.
        return result.Kind == PathKind.DriveCurrentDirectoryRelative
            ? GetDriveCurrentDirectory(result.Value.Substring(0, 2)) / result.Value.Substring(2)
            : new(result.Value, checkAbsoluteness: false);
    }

    private static AbsolutePath GetDriveCurrentDirectory(string drive)
    {
        // TODO[#24]: This will get fancy when we support UNC and DOS Device paths.
        // Only the drive gets resolved: Path.GetFullPath may throw for the rest of the path, or alter it (e.g. trim
        // the trailing dots).
        var directory = new LocalPath(Path.GetFullPath(drive));
        var root = drive + Path.DirectorySeparatorChar;
        return new(directory.IsAbsolute ? directory.Value : root, checkAbsoluteness: false);
    }

    /// <inheritdoc cref="op_Division(AbsolutePath, LocalPath)"/>
    public static AbsolutePath operator /(AbsolutePath basePath, string b) => basePath / new LocalPath(b);

    /// <returns>The normalized path string contained in this object.</returns>
    public override string ToString() => Value;

    /// <summary>Compares the path with another.</summary>
    /// <remarks>Uses <see cref="PlatformDefaultComparer"/> for platform-default case sensitivity.</remarks>
    public bool Equals(AbsolutePath other) => Equals(other, PlatformDefaultComparer);

    /// <summary>
    /// Determines whether the specified <see cref="AbsolutePath"/> is equal to the current <see cref="AbsolutePath"/>
    /// using the specified comparer.
    /// </summary>
    /// <param name="other">The <see cref="AbsolutePath"/> to compare with the current <see cref="AbsolutePath"/>.</param>
    /// <param name="comparer">
    /// The comparer to use for comparing the paths. For example, pass <see cref="PlatformDefaultComparer"/> or
    /// <see cref="StrictStringComparer"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified <see cref="AbsolutePath"/> is equal to the current
    /// <see cref="AbsolutePath"/> using the specified comparer; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(AbsolutePath other, IEqualityComparer<AbsolutePath> comparer) =>
        comparer.Equals(this, other);

    /// <inheritdoc cref="Equals(AbsolutePath)"/>
    public override bool Equals(object? obj)
    {
        return obj is AbsolutePath other && Equals(other);
    }

    /// <inheritdoc cref="Object.GetHashCode"/>
    public override int GetHashCode() => PlatformDefaultComparer.GetHashCode(this);

    /// <inheritdoc cref="Equals(AbsolutePath)"/>
    public static bool operator ==(AbsolutePath left, AbsolutePath right)
    {
        return left.Equals(right);
    }

    /// <inheritdoc cref="Equals(AbsolutePath)"/>
    public static bool operator !=(AbsolutePath left, AbsolutePath right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Determines the type of the file system entry (file, directory, symlink, or junction) for the given path.
    /// </summary>
    /// <returns>
    /// A <see cref="FileEntryKind"/> enumeration value representing the type of the file system entry, or <see langword="null"/> if the entry does not exist.
    /// </returns>
    /// <remarks>
    /// This method checks if the specified path represents a file, directory, symbolic link, or junction. On Windows, it uses <see cref="DiskUtils.IsJunction"/> to identify junctions and checks for the <see cref="FileAttributes.ReparsePoint"/> flag to identify symbolic links.
    /// </remarks>
    public FileEntryKind? ReadKind()
    {
        if (!File.Exists(Value) && !Directory.Exists(Value))
        {
            return null;
        }

        var attributes = File.GetAttributes(Value);

        if (!attributes.HasFlag(FileAttributes.Directory))
        {
            return FileEntryKind.File;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (DiskUtils.IsJunction(Value))
            {
                return FileEntryKind.Junction;
            }

            if (attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                return FileEntryKind.Symlink;
            }

            return FileEntryKind.Directory;
        }

        if (attributes.HasFlag(FileAttributes.ReparsePoint))
        {
            return FileEntryKind.Symlink;
        }

        return FileEntryKind.Directory;
    }

    /// <summary>
    /// Compares the current <see cref="AbsolutePath"/> instance with another <see cref="AbsolutePath"/> instance,
    /// using the default platform-aware comparison rules provided by <see cref="PlatformDefaultPathComparer{TPath}"/>.
    /// </summary>
    /// <param name="other">The <see cref="AbsolutePath"/> instance to compare with the current instance.</param>
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
    public int CompareTo(AbsolutePath other) => PlatformDefaultComparer.Compare(this, other);
}

/// <summary>
/// Specifies the type of file system entry.
/// </summary>
public enum FileEntryKind
{
    /// <summary>
    /// The type of the file system entry is unknown.
    /// </summary>
    Unknown,

    /// <summary>
    /// The file system entry is a regular file.
    /// </summary>
    File,

    /// <summary>
    /// The file system entry is a directory.
    /// </summary>
    Directory,

    /// <summary>
    /// The file system entry is a symbolic link.
    /// </summary>
    Symlink,

    /// <summary>
    /// The file system entry is a junction.
    /// </summary>
    Junction
}
