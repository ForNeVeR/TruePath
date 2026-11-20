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
///     Generates System.IO forwarding extensions for marked partial classes. Semantic projections may rerun on
///     any compilation change; value equality stops unchanged facts before documentation formatting and rendering.
/// </summary>
[Generator]
public sealed class PathIoSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var analyses = context.SyntaxProvider.ForAttributeWithMetadataName(
            PathIoRules.MarkerAttributeName,
            static (node, _) => node is ClassDeclarationSyntax,
            static (attributeContext, cancellationToken) => PathIoGeneratorCore.AnalyzeMarkedType(
                attributeContext.SemanticModel.Compilation,
                (INamedTypeSymbol)attributeContext.TargetSymbol, cancellationToken));
        var markedTypes = analyses.Select(static (analysis, _) => analysis.Type).WithTrackingName("MarkedTypes");

        var parsedExclusions = context.AdditionalTextsProvider
            .Where(static file => Path.GetFileName(file.Path) == PathIoRules.ExclusionFileName)
            .Select(static (file, cancellationToken) => file.GetText(cancellationToken)?.ToString() ?? string.Empty)
            .Collect()
            .Select(static (texts, cancellationToken) => Exclusions.Parse(texts, cancellationToken));
        var exclusions = parsedExclusions.Select(static (parsed, _) => parsed.DocIds)
            .WithComparer(DocIdSetComparer.Instance).WithTrackingName("ExclusionIds");
        context.RegisterSourceOutput(parsedExclusions, static (output, parsed) =>
        {
            foreach (var diagnostic in parsed.Diagnostics) output.ReportDiagnostic(diagnostic);
        });

        var docIndex = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(".xml", StringComparison.Ordinal))
            .Select(static (file, cancellationToken) => BclDocIndex.Build(ImmutableArray.Create(file), cancellationToken))
            .WithTrackingName("DocumentFiles")
            .Collect()
            .Select(static (indexes, cancellationToken) => BclDocIndex.Merge(indexes, cancellationToken))
            .WithTrackingName("DocumentationIndex");
        var api = context.CompilationProvider
            .Select(static (compilation, cancellationToken) => PathIoGeneratorCore.ExtractBclApi(compilation, cancellationToken))
            .WithTrackingName("BclApi");
        var templates = api.Combine(docIndex)
            .Select(static (pair, cancellationToken) => PathIoGeneratorCore.AddDocumentation(pair.Left, pair.Right, cancellationToken))
            .WithTrackingName("MethodTemplates");

        // Diagnostics retain current source locations without invalidating the source rendering cache.
        context.RegisterSourceOutput(analyses.Combine(api), static (output, pair) =>
        {
            foreach (var diagnostic in PathIoGeneratorCore.GetTypeDiagnostics(pair.Left, pair.Right))
                output.ReportDiagnostic(diagnostic);
        });

        var sources = markedTypes.Combine(templates).Combine(exclusions)
            .Select(static (input, cancellationToken) => (
                HintName: input.Left.Left.SimpleName + ".Generated.g.cs",
                Source: PathIoGeneratorCore.Render(input.Left.Left, input.Left.Right, input.Right, cancellationToken)))
            .WithTrackingName("RenderedSource");
        context.RegisterSourceOutput(sources, static (output, source) =>
        {
            output.CancellationToken.ThrowIfCancellationRequested();
            if (source.Source != null)
                output.AddSource(source.HintName, SourceText.From(source.Source, Encoding.UTF8));
        });
    }
}
