// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

#if !NET8_0_OR_GREATER
// ReSharper disable once CheckNamespace
namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// A polyfill of the attribute available since .NET 8. The marked API is considered experimental and can be changed in
/// new releases of the library.
/// </summary>
[AttributeUsage(
    AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct
    | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property
    | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate,
    Inherited = false)]
internal sealed class ExperimentalAttribute(string diagnosticId) : Attribute
{
    public string DiagnosticId { get; } = diagnosticId;
    public string? UrlFormat { get; set; }
}
#endif
