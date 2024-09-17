// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath;

/// <summary>
/// This is a path on the local system that's guaranteed to be <b>absolute</b>: that is, path that is rooted and has a
/// disk letter (on Windows).
/// </summary>
/// <remarks>For a path that's not guaranteed to be absolute, use the <see cref="LocalPath"/> type.</remarks>
public readonly struct AbsolutePath : IEquatable<AbsolutePath>, IPath, IPath<AbsolutePath>
{
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

    /// <summary>
    /// Gets the current working directory as an AbsolutePath instance.
    /// </summary>
    /// <value>
    /// The current working directory.
    /// </value>
    public static AbsolutePath CurrentWorkingDirectory => new(Environment.CurrentDirectory);

    /// <summary>
    /// Calculates the relative path from a base path to this path.
    /// </summary>
    /// <param name="basePath">The base path from which to calculate the relative path.</param>
    /// <returns>The relative path from the base path to this path.</returns>
    public LocalPath RelativeTo(AbsolutePath basePath) => new(Path.GetRelativePath(basePath.Value, Value));

    /// <summary>
    /// Converts the path to absolute, corrects the file name case on case-insensitive file systems, resolves symlinks.
    /// </summary>
    public AbsolutePath Canonicalize() => new(DiskUtils.GetRealPath(Value));

    /// <remarks>
    /// Note that in case path <paramref name="b"/> is <b>absolute</b>, it will completely take over and the
    /// <paramref name="basePath"/> will be ignored.
    /// </remarks>
    public static AbsolutePath operator /(AbsolutePath basePath, LocalPath b) =>
        new(Path.Combine(basePath.Value, b.Value));

    /// <remarks>
    /// Note that in case path <paramref name="b"/> is <b>absolute</b>, it will completely take over and the
    /// <paramref name="basePath"/> will be ignored.
    /// </remarks>
    public static AbsolutePath operator /(AbsolutePath basePath, string b) => basePath / new LocalPath(b);

    /// <returns>The normalized path string contained in this object.</returns>
    public override string ToString() => Value;

    /// <summary>Compares the path with another.</summary>
    /// <remarks>Note that currently this comparison is case-sensitive.</remarks>
    public bool Equals(AbsolutePath other)
    {
        var comparer = PlatformDefaultPathComparer.Comparer;
        return comparer.Compare(Underlying.Value, other.Underlying.Value) == 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="AbsolutePath"/> is equal to the current <see cref="AbsolutePath"/> using the specified string comparer.
    /// </summary>
    /// <param name="other">The <see cref="AbsolutePath"/> to compare with the current <see cref="AbsolutePath"/>.</param>
    /// <param name="comparer">The comparer to use for comparing the paths.</param>
    /// <returns>
    /// <see langword="true"/> if the specified <see cref="AbsolutePath"/> is equal to the current <see cref="AbsolutePath"/> using the specified string comparer; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// If the comparer is null, this method returns <see langword="false"/>.
    /// </remarks>
    public bool Equals(AbsolutePath other, IComparer<string> comparer)
    {
        return comparer.Compare(Value, other.Value) == 0;
    }

    /// <inheritdoc cref="Equals(AbsolutePath)"/>
    public override bool Equals(object? obj)
    {
        return obj is AbsolutePath other && Equals(other);
    }

    /// <inheritdoc cref="Object.GetHashCode"/>
    public override int GetHashCode()
    {
        return Underlying.GetHashCode();
    }

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
}
