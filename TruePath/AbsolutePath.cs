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
    /// <exception cref="ArgumentException">Thrown if the passed string does not represent an absolute path.</exception>
    public AbsolutePath(string value)
    {
        Underlying = new LocalPath(value);
        if (!Underlying.IsAbsolute)
            throw new ArgumentException($"Path \"{value}\" is not absolute.");
    }

    /// <summary>
    /// Creates an <see cref="AbsolutePath"/> instance by converting a <paramref name="localPath"/> object.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the passed path is not absolute.</exception>
    public AbsolutePath(LocalPath localPath) : this(localPath.Value) {}

    /// <inheritdoc cref="IPath.Value"/>
    public string Value => Underlying.Value;

    /// <inheritdoc cref="IPath.Parent"/>
    public AbsolutePath? Parent => Underlying.Parent is { } path ? new(path.Value) : null;
        // TODO[#17]: Optimize, the strict check here is not necessary.

    /// <inheritdoc cref="IPath.Parent"/>
    IPath? IPath.Parent => Parent;

    /// <inheritdoc cref="IPath.FileName"/>
    public string FileName => Underlying.FileName;

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
        return Underlying.Equals(other.Underlying);
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
