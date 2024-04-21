// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

namespace TruePath;

/// <summary>
/// Same as <see cref="AbsolutePath"/>, but also validates that the path is absolute during its creation.
/// </summary>
public readonly struct StrictAbsolutePath
{
    private readonly AbsolutePath _underlying;
    public StrictAbsolutePath(string value)
    {
        if (!Path.IsPathRooted(value))
            throw new ArgumentException($"Path \"{value}\" is not absolute.");
        _underlying = new AbsolutePath(value);
    }

    /// <summary>The normalized path string.</summary>
    public string Value => _underlying.Value;

    public StrictAbsolutePath? Parent => _underlying.Parent is { } path ? new(path.Value) : null;
        // TODO[#36]: Optimize, the strict check here is not necessary.

    public string FileName => _underlying.FileName;

    public static StrictAbsolutePath operator /(StrictAbsolutePath basePath, RelativePath b) =>
        new(Path.Combine(basePath.Value, b.Value));
    public static StrictAbsolutePath operator /(StrictAbsolutePath basePath, string b) => basePath / new RelativePath(b);

    public RelativePath AsRelative() => _underlying.AsRelative();

    public override string ToString()
    {
        return Value;
    }

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
