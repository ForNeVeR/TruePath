// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

namespace TruePath;

/// <summary>Absolute path in the local file system.</summary>
public readonly struct AbsolutePath(string value)
{
    /// <summary>The normalized path string.</summary>
    public string Value { get; } = PathStrings.Normalize(value);

    public AbsolutePath? Parent => Path.GetDirectoryName(Value) is { } parent ? new(parent) : null;
    public string FileName => Path.GetFileName(Value);

    public override string ToString() => Value;

    public bool Equals(AbsolutePath other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is AbsolutePath other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    /// <remarks>
    /// Checks for a non-strict prefix: if the paths are equal then they are still considered prefixes of each other. </remarks>
    public bool IsPrefixOf(AbsolutePath other)
    {
        if (!(Value.Length <= other.Value.Length && other.Value.StartsWith(Value))) return false;
        return other.Value.Length == Value.Length || other.Value[Value.Length] == Path.DirectorySeparatorChar;
    }

    public static AbsolutePath operator /(AbsolutePath basePath, RelativePath b) =>
        new(Path.Combine(basePath.Value, b.Value));
    public static AbsolutePath operator /(AbsolutePath basePath, string b) => basePath / new RelativePath(b);

    public static implicit operator AbsolutePath(StrictAbsolutePath path) => new(path.Value);
    public RelativePath AsRelative() => new(Value);
}
