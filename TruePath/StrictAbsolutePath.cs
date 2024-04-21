// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

namespace TruePath;

/// <summary>
/// This is a path on the local system that's guaranteed to be <b>absolute</b>: that is, path that is rooted and has a
/// disk letter (on Windows).
/// </summary>
public readonly struct StrictAbsolutePath
{
    private readonly LocalPath _underlying;
    public StrictAbsolutePath(string value)
    {
        if (!Path.IsPathRooted(value))
            throw new ArgumentException($"Path \"{value}\" is not absolute.");
        _underlying = new LocalPath(value);
    }

    /// <summary>The normalized path string.</summary>
    public string Value => _underlying.Value;

    /// <summary>The parent of this path. Will be <c>null</c> for a rooted absolute path.</summary>
    public StrictAbsolutePath? Parent => _underlying.Parent is { } path ? new(path.Value) : null;
        // TODO[#36]: Optimize, the strict check here is not necessary.

    /// <summary>The full name of the last component of this path.</summary>
    public string FileName => _underlying.FileName;

    /// <remarks>
    /// Note that in case path <paramref name="b"/> is <b>absolute</b>, it will completely take over and the
    /// <paramref name="basePath"/> will be ignored.
    /// </remarks>
    public static StrictAbsolutePath operator /(StrictAbsolutePath basePath, LocalPath b) =>
        new(Path.Combine(basePath.Value, b.Value));

    /// <remarks>
    /// Note that in case path <paramref name="b"/> is <b>absolute</b>, it will completely take over and the
    /// <paramref name="basePath"/> will be ignored.
    /// </remarks>
    public static StrictAbsolutePath operator /(StrictAbsolutePath basePath, string b) => basePath / new LocalPath(b);

    public override string ToString() => Value;

    public bool Equals(StrictAbsolutePath other)
    {
        return _underlying.Equals(other._underlying);
    }

    public override bool Equals(object? obj)
    {
        return obj is StrictAbsolutePath other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _underlying.GetHashCode();
    }

    public static bool operator ==(StrictAbsolutePath left, StrictAbsolutePath right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(StrictAbsolutePath left, StrictAbsolutePath right)
    {
        return !left.Equals(right);
    }
}
