// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;

namespace TruePath.PathIoGenerator;

internal sealed class DocIdSetComparer : IEqualityComparer<ImmutableHashSet<string>>
{
    public static DocIdSetComparer Instance { get; } = new();

    public bool Equals(ImmutableHashSet<string>? x, ImmutableHashSet<string>? y) =>
        ReferenceEquals(x, y) || x != null && y != null && x.SetEquals(y);

    public int GetHashCode(ImmutableHashSet<string> obj) => obj.Count;
}
