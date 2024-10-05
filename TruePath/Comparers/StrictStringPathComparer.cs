// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath.Comparers;

/// <summary>A strict comparer for comparing file paths using ordinal, case-sensitive comparison.</summary>
public class StrictStringPathComparer : IComparer<string>
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="StrictStringPathComparer"/> class.
    /// </summary>
    public static readonly StrictStringPathComparer Instance = new();

    /// <inheritdoc cref="IComparer{T}.Compare"/>
    public int Compare(string? x, string? y)
    {
        return StringComparer.Ordinal.Compare(x, y);
    }
}
