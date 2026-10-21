// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TruePath.PathIoGenerator;

/// <summary>
///     Fills the other partial part of every class marked with <c>TruePath.SystemIo.GenerateSystemIoExtensionsAttribute</c>
///     with the BCL-forwarding extension methods described in <see cref="PathIoGeneratorCore"/>.
/// </summary>
[Generator]
public sealed class PathIoSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // The expensive per-type analysis runs in the syntax-anchored transform, so it is cached across edits in
        // unrelated files; everything else that is independent of the compilation (exclusion texts, the XML
        // documentation index) is also computed in nodes of its own instead of inside the output stage.
        var markedTypes = context.SyntaxProvider.ForAttributeWithMetadataName<MarkedType>(
            PathIoRules.MarkerAttributeName,
            static (node, _) => node is ClassDeclarationSyntax,
            static (attributeContext, _) => new MarkedType(
                attributeContext.TargetSymbol.ToDisplayString(),
                attributeContext.TargetSymbol.ContainingType != null,
                PathIoGeneratorCore.CollectHandWrittenInfo(
                    attributeContext.SemanticModel.Compilation,
                    (INamedTypeSymbol)attributeContext.TargetSymbol)));

        var exclusionTexts = context.AdditionalTextsProvider
            .Where(static file => Path.GetFileName(file.Path) == PathIoRules.ExclusionFileName)
            .Select(static (file, _) => file.GetText()?.ToString() ?? string.Empty)
            .Collect();

        var docIndex = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(".xml", StringComparison.Ordinal))
            .Collect()
            .Select(static (files, _) => BclDocIndex.Build(files));

        var input = markedTypes.Collect()
            .Combine(exclusionTexts)
            .Combine(docIndex)
            .Combine(context.CompilationProvider);

        context.RegisterSourceOutput(input, static (productionContext, tuple) =>
        {
            var (((typeInfos, exclusionTexts), docIndex), compilation) = tuple;

            var exclusions = Exclusions.Parse(exclusionTexts);
            foreach (var diagnostic in exclusions.Diagnostics) productionContext.ReportDiagnostic(diagnostic);

            foreach (var typeInfo in typeInfos.Sort((left, right) => string.CompareOrdinal(left.TypeName, right.TypeName)))
            {
                var result = PathIoGeneratorCore.Generate(
                    compilation, typeInfo.TypeName, typeInfo.IsNested, typeInfo.HandWritten, exclusions.DocIds, docIndex);
                foreach (var diagnostic in result.Diagnostics) productionContext.ReportDiagnostic(diagnostic);

                if (result.Source is { } source)
                {
                    // Nested marked types are rejected with TPG004, so the simple type name is unambiguous here.
                    // Should two same-named top-level types ever be marked, AddSource fails loudly.
                    var hintName = typeInfo.TypeName.Substring(typeInfo.TypeName.LastIndexOf('.') + 1) + ".Generated.g.cs";
                    productionContext.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
                }
            }
        });
    }
}
