// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

namespace TruePath;

/// <summary>Relative path in the local file system.</summary>
/// <remarks>
/// Note that absolute paths can be used in the most places where the relative paths can, so an absolute path
/// technically may be considered as a valid relative path as well. This type may contain an absolute path as well.
/// </remarks>
public readonly struct RelativePath(string value)
{
    public bool Equals(RelativePath other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is RelativePath other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(RelativePath left, RelativePath right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RelativePath left, RelativePath right)
    {
        return !left.Equals(right);
    }

    /// <summary>The normalized path string.</summary>
    public string Value { get; } = PathStrings.Normalize(value);

    public override string ToString() => Value;
}
