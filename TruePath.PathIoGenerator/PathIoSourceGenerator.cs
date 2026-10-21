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
        var markedTypes = context.SyntaxProvider.ForAttributeWithMetadataName<string>(
            PathIoRules.MarkerAttributeName,
            static (node, _) => node is ClassDeclarationSyntax,
            static (attributeContext, _) => attributeContext.TargetSymbol.ToDisplayString());

        var additionalTexts = context.AdditionalTextsProvider.Collect();
        var docIndex = additionalTexts.Select(static (files, _) => BclDocIndex.Build(files));

        var input = markedTypes.Collect()
            .Combine(docIndex)
            .Combine(additionalTexts)
            .Combine(context.CompilationProvider);

        context.RegisterSourceOutput(input, static (productionContext, tuple) =>
        {
            var (((typeNames, docIndex), additionalTexts), compilation) = tuple;

            var exclusions = Exclusions.Parse(additionalTexts
                .Where(file => file.Path.EndsWith(PathIoRules.ExclusionFileName, StringComparison.Ordinal))
                .Select(file => file.GetText()?.ToString() ?? string.Empty)
                .ToImmutableArray());
            foreach (var diagnostic in exclusions.Diagnostics) productionContext.ReportDiagnostic(diagnostic);

            foreach (var typeName in typeNames.Sort(StringComparer.Ordinal))
            {
                var result = PathIoGeneratorCore.Generate(compilation, typeName, exclusions.DocIds, docIndex);
                foreach (var diagnostic in result.Diagnostics) productionContext.ReportDiagnostic(diagnostic);

                if (result.Source is { } source)
                {
                    var hintName = typeName.Substring(typeName.LastIndexOf('.') + 1) + ".Generated.g.cs";
                    productionContext.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
                }
            }
        });
    }
}
