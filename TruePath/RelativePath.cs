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
    /// <summary>The normalized path string.</summary>
    public string Value { get; } = PathStrings.Normalize(value);
}
