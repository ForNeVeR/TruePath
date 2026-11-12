// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath.PathIoGenerator;

// A null method records an API without a leading path parameter, for the generated skipped-method list.
internal sealed class MethodCandidate(string docId, string signature, GeneratedMethod? method, string? fallbackXml)
{
    public string DocId { get; } = docId;
    public string Signature { get; } = signature;
    public GeneratedMethod? Method { get; } = method;
    public string? FallbackXml { get; } = fallbackXml;

    public override bool Equals(object? obj) => obj is MethodCandidate other
        && DocId == other.DocId && Signature == other.Signature
        && Equals(Method, other.Method) && FallbackXml == other.FallbackXml;

    public override int GetHashCode() => DocId.GetHashCode();
}
