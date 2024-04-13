// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

namespace TruePath;

/// <summary>
/// Same as <see cref="AbsolutePath"/>, but also validates that the path is absolute during its creation.
/// </summary>
public readonly struct StrictAbsolutePath(string value)
{
    private readonly AbsolutePath _underlying = new(value);

    /// <summary>The normalized path string.</summary>
    public string Value => _underlying.Value;

    public StrictAbsolutePath? Parent => _underlying.Parent is { } path ? new(path.Value) : null;
        // TODO: Optimize, the strict check here is not necessary.

    public string FileName => _underlying.FileName;

    public static StrictAbsolutePath operator /(StrictAbsolutePath basePath, RelativePath b) =>
        new(Path.Combine(basePath.Value, b.Value));
    public static StrictAbsolutePath operator /(StrictAbsolutePath basePath, string b) => basePath / new RelativePath(b);

    public RelativePath AsRelative() => _underlying.AsRelative();
}
