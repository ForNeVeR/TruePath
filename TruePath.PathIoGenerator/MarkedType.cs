// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath.PathIoGenerator;

/// <summary>
///     The value-equatable facts needed to render one marked class. Semantic analysis may run after any edit;
///     equal facts stop the downstream rendering pipeline, including after edits in another partial declaration.
/// </summary>
internal sealed class MarkedType(
    string typeName, string simpleName, string namespaceName,
    bool isNested, bool isPartial, bool resolves, HandWrittenInfo handWritten)
{
    public string TypeName { get; } = typeName;
    public string SimpleName { get; } = simpleName;
    public string NamespaceName { get; } = namespaceName;
    public bool IsNested { get; } = isNested;
    public bool IsPartial { get; } = isPartial;
    public bool Resolves { get; } = resolves;
    public HandWrittenInfo HandWritten { get; } = handWritten;

    public bool CanGenerate => !IsNested && IsPartial && Resolves;

    public override bool Equals(object? obj) => obj is MarkedType other
        && TypeName == other.TypeName && SimpleName == other.SimpleName && NamespaceName == other.NamespaceName
        && IsNested == other.IsNested && IsPartial == other.IsPartial && Resolves == other.Resolves
        && HandWritten.Equals(other.HandWritten);

    public override int GetHashCode() => TypeName.GetHashCode();
}
