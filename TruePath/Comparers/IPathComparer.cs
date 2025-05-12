// SPDX-FileCopyrightText: 2025 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath.Comparers;

/// <summary>
/// Provides an interface for defining custom comparison logic for path types.
/// This interface combines both equality (<see cref="IEqualityComparer{T}"/>)
/// and ordering (<see cref="IComparer{T}"/>) comparisons.
/// </summary>
/// <typeparam name="TPath">The type of paths being compared, which must implement <see cref="IPath"/>.</typeparam>
public interface IPathComparer<in TPath> : IEqualityComparer<TPath>, IComparer<TPath> where TPath : IPath;
