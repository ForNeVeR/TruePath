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

    public static AbsolutePath operator /(AbsolutePath basePath, RelativePath b) =>
        new(Path.Combine(basePath.Value, b.Value));
    public static AbsolutePath operator /(AbsolutePath basePath, string b) => basePath / new RelativePath(b);

    public static implicit operator AbsolutePath(StrictAbsolutePath path) => new(path.Value);
    public RelativePath AsRelative() => new(Value);
}
