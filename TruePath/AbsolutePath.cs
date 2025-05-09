// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;
using TruePath.Comparers;

namespace TruePath;

/// <summary>
/// This is a path on the local system that's guaranteed to be <b>absolute</b>: that is, path that is rooted and has a
/// disk letter (on Windows).
/// </summary>
/// <remarks>For a path that's not guaranteed to be absolute, use the <see cref="LocalPath"/> type.</remarks>
public readonly struct AbsolutePath : IEquatable<AbsolutePath>, IComparable<AbsolutePath>, IPath, IPath<AbsolutePath>
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
    public static readonly PathComparer<AbsolutePath> PlatformDefaultComparer =
        new PlatformDefaultPathComparer<AbsolutePath>();

    /// <summary>
    /// A strict comparer for comparing file paths using ordinal, case-sensitive comparison of the underlying path
    /// strings.
    /// </summary>
    public static readonly PathComparer<AbsolutePath> StrictStringComparer =
        new StrictStringPathComparer<AbsolutePath>();

    internal readonly LocalPath Underlying;

    /// <summary>
    /// Creates an <see cref="AbsolutePath"/> instance by normalizing the path from the passed string according to the
    /// rules stated in <see cref="LocalPath"/>.
    /// </summary>
    /// <param name="value">Path string to normalize.</param>
    /// <param name="checkAbsoluteness">Flag indicating whether absoluteness of path should be checked</param>
    /// <exception cref="ArgumentException">Thrown if the passed string does not represent an absolute path.</exception>>
    private AbsolutePath(string value, bool checkAbsoluteness)
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

    /// <inheritdoc cref="IPath.Parent"/>
    IPath? IPath.Parent => Parent;

    /// <inheritdoc cref="IPath.FileName"/>
    public string FileName => Underlying.FileName;

    /// <inheritdoc cref="IPath{TPath}.StartsWith(TPath)"/>
    public bool StartsWith(AbsolutePath other) => Value.StartsWith(other.Value);

    /// <inheritdoc cref="IPath{TPath}.IsPrefixOf(TPath)"/>
    public bool IsPrefixOf(AbsolutePath other)
    {
        return Value.Length <= other.Value.Length && other.Value.StartsWith(Value);
    }

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
    /// <returns>The relative path from the base path to this path.</returns>
    public LocalPath RelativeTo(AbsolutePath basePath) => new(Path.GetRelativePath(basePath.Value, Value));

    /// <summary>Corrects the file name case on case-insensitive file systems, resolves symlinks.</summary>
    public AbsolutePath Canonicalize() => new(DiskUtils.GetRealPath(Value));

    /// <remarks>
    /// Note that in case path <paramref name="b"/> is <b>absolute</b>, it will completely take over and the
    /// <paramref name="basePath"/> will be ignored.
    /// </remarks>
    public static AbsolutePath operator /(AbsolutePath basePath, LocalPath b) =>
        new(Path.Combine(basePath.Value, b.Value), false);

    /// <remarks>
    /// Note that in case path <paramref name="b"/> is <b>absolute</b>, it will completely take over and the
    /// <paramref name="basePath"/> will be ignored.
    /// </remarks>
    public static AbsolutePath operator /(AbsolutePath basePath, string b) => basePath / new LocalPath(b);

    /// <returns>The normalized path string contained in this object.</returns>
    public override string ToString() => Value;

    /// <summary>Compares the path with another.</summary>
    /// <remarks>Note that currently this comparison is case-sensitive.</remarks>
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

