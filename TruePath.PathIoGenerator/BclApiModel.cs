// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;

namespace TruePath.PathIoGenerator;

// Only values leave the semantic projection: no symbols or compilations survive in the rendering cache.
internal sealed class BclApiModel(
    ImmutableArray<MethodCandidate> methods,
    ImmutableArray<string> importedNamespaces,
    ImmutableArray<string> missingTypes)
{
    public ImmutableArray<MethodCandidate> Methods { get; } = methods;
    public ImmutableArray<string> ImportedNamespaces { get; } = importedNamespaces;
    public ImmutableArray<string> MissingTypes { get; } = missingTypes;

    public override bool Equals(object? obj) => obj is BclApiModel other
        && Methods.SequenceEqual(other.Methods)
        && ImportedNamespaces.SequenceEqual(other.ImportedNamespaces)
        && MissingTypes.SequenceEqual(other.MissingTypes);

    public override int GetHashCode() => Methods.Length;
}
