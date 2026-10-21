// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace TruePath.PathIoGenerator;

/// <summary>
///     The parsed contents of the <c>path-io-exclusions.txt</c> additional file: XML documentation IDs of the BCL
///     methods that must not be exposed as PathIo extensions. Methods covered by handwritten members (either by
///     rewritten signature or by the forwarding call inside the handwritten body) are excluded automatically and
///     must not be listed there.
/// </summary>
internal sealed class Exclusions
{
    public ImmutableHashSet<string> DocIds { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }

    private Exclusions(ImmutableHashSet<string> docIds, ImmutableArray<Diagnostic> diagnostics)
    {
        DocIds = docIds;
        Diagnostics = diagnostics;
    }

    public static Exclusions Parse(ImmutableArray<string> fileTexts)
    {
        var docIds = ImmutableHashSet.CreateBuilder<string>();
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

        foreach (var text in fileTexts)
        foreach (var rawLine in text.Replace("\r\n", "\n").Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal)) continue;

            if (line.StartsWith("M:", StringComparison.Ordinal))
            {
                docIds.Add(line);
            }
            else
            {
                diagnostics.Add(Diagnostic.Create(GeneratorDiagnostics.InvalidExclusionEntry, Location.None, line));
            }
        }

        return new Exclusions(docIds.ToImmutable(), diagnostics.ToImmutable());
    }
}
