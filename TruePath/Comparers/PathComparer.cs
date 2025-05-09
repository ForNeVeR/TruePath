// SPDX-FileCopyrightText: 2025 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath.Comparers;

/// <summary>
/// Provides an abstract base class for defining custom comparison logic for path types.
/// This class combines both equality (<see cref="IEqualityComparer{T}"/>)
/// and ordering (<see cref="IComparer{T}"/>) comparisons.
/// </summary>
/// <typeparam name="TPath">The type of paths being compared, which must implement <see cref="IPath"/>.</typeparam>
public abstract class PathComparer<TPath> : IEqualityComparer<TPath>, IComparer<TPath> where TPath : IPath
{
    public abstract bool Equals(TPath? x, TPath? y);

    public abstract int GetHashCode(TPath obj);

    public abstract int Compare(TPath? x, TPath? y);
}
