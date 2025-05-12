// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath.Comparers;

/// <summary>
/// A strict comparer for comparing file paths using ordinal, case-sensitive comparison of the underlying path strings.
/// </summary>
internal class StrictStringPathComparer<TPath> : IPathComparer<TPath> where TPath : IPath
{
    public bool Equals(TPath? x, TPath? y) => StringComparer.Ordinal.Equals(x?.Value, y?.Value);

    public int GetHashCode(TPath obj) => StringComparer.Ordinal.GetHashCode(obj.Value);

    public int Compare(TPath? x, TPath? y) => StringComparer.Ordinal.Compare(x?.Value, y?.Value);
}
