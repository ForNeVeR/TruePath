// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TruePath.PathIoGenerator;

public sealed class GenerationResult(string? source, ImmutableArray<Diagnostic> diagnostics)
{
    public string? Source { get; } = source;
    public ImmutableArray<Diagnostic> Diagnostics { get; } = diagnostics;
}

/// <summary>
///     Generates the forwarding part of a class marked with <c>GenerateSystemIoExtensionsAttribute</c>: for every
///     public static method of <c>System.IO.File</c> and <c>System.IO.Directory</c> present in the current
///     compilation, a public static extension method on <c>TruePath.AbsolutePath</c> that forwards to it, with the
///     BCL XML documentation copied verbatim.
///
///     A BCL method is not generated when:
///     <list type="bullet">
///         <item>its ID is listed in path-io-exclusions.txt;</item>
///         <item>a handwritten member of the marked class already occupies the same rewritten signature;</item>
///         <item>a handwritten member forwards to it (detected by the forwarding call in its body);</item>
///         <item>another BCL method (File wins over Directory) already produced the same signature.</item>
///     </list>
/// </summary>
public static class PathIoGeneratorCore
{
    private const string GeneratorName = "TruePath.PathIoGenerator";
    private const string GeneratorVersion = "1.0.0";

    private static readonly ImmutableHashSet<string> ForwardedAttributes = ImmutableHashSet.Create(
        "System.Runtime.Versioning.SupportedOSPlatformAttribute",
        "System.Runtime.Versioning.UnsupportedOSPlatformAttribute",
        "System.ObsoleteAttribute");

    public static GenerationResult Generate(
        Compilation compilation,
        string markedTypeMetadataName,
        bool markedTypeIsNested,
        HandWrittenInfo handWritten,
        ImmutableHashSet<string> excludedDocIds,
        BclDocIndex docIndex)
    {
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

        if (markedTypeIsNested)
        {
            diagnostics.Add(Diagnostic.Create(GeneratorDiagnostics.MarkedClassIsNested, Location.None, markedTypeMetadataName));
            return new GenerationResult(null, diagnostics.ToImmutable());
        }

        var fileType = compilation.GetTypeByMetadataName("System.IO.File");
        var directoryType = compilation.GetTypeByMetadataName("System.IO.Directory");
        var absolutePathType = compilation.GetTypeByMetadataName(PathIoRules.AbsolutePathTypeName);
        var markedType = compilation.GetTypeByMetadataName(markedTypeMetadataName);

        foreach (var (name, symbol) in new[]
                 {
                     ("System.IO.File", fileType),
                     ("System.IO.Directory", directoryType),
                     (PathIoRules.AbsolutePathTypeName, absolutePathType),
                     (markedTypeMetadataName, markedType),
                 })
            if (symbol == null)
                diagnostics.Add(Diagnostic.Create(GeneratorDiagnostics.RequiredTypeNotFound, Location.None, name));
        if (diagnostics.Count > 0)
            return new GenerationResult(null, diagnostics.ToImmutable());

        if (!IsDeclaredPartial(markedType!))
        {
            diagnostics.Add(Diagnostic.Create(
                GeneratorDiagnostics.MarkedClassNotPartial,
                markedType!.Locations.FirstOrDefault() ?? Location.None,
                markedType.Name));
            return new GenerationResult(null, diagnostics.ToImmutable());
        }

        var renderer = TypeRenderer.Create(compilation);
        var generatedMethods = new List<GeneratedMethod>();
        var generatedKeys = new HashSet<string>();
        var skippedWithoutPath = new SortedSet<string>();

        foreach (var owner in new[] { fileType!, directoryType! }) // File first, so File wins on duplicate signatures
        foreach (var method in owner.GetMembers().OfType<IMethodSymbol>())
        {
            if (method.MethodKind != MethodKind.Ordinary
                || !method.IsStatic
                || method.DeclaredAccessibility != Accessibility.Public
                || method.IsGenericMethod
                || method.Parameters.Any(p => p.RefKind != RefKind.None))
                continue;

            var docId = DocId.ForMethod(method);
            if (excludedDocIds.Contains(docId) || handWritten.ForwardedDocIds.Contains(docId)) continue;

            var pathOrdinals = method.Parameters
                .Where(p => p.Type.SpecialType == SpecialType.System_String && PathIoRules.IsPathParameterName(p.Name))
                .Select(p => p.Ordinal)
                .ToImmutableHashSet();
            if (pathOrdinals.Count == 0 || !pathOrdinals.Contains(0))
            {
                skippedWithoutPath.Add(docId);
                continue;
            }

            var key = SignatureKey(method.Name, method.Parameters.Select(p => TypeDisplay(p, pathOrdinals, absolutePathType!, renderer)));
            if (handWritten.Signatures.Contains(key)) continue;
            if (!generatedKeys.Add(key)) continue;

            generatedMethods.Add(BuildMethod(owner, method, pathOrdinals, absolutePathType!, docIndex, renderer));
        }

        var source = RenderFile(markedType!, generatedMethods, skippedWithoutPath, renderer);
        return new GenerationResult(source, diagnostics.ToImmutable());
    }

    private static bool IsDeclaredPartial(INamedTypeSymbol type)
    {
        return type.DeclaringSyntaxReferences
            .Select(reference => reference.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
            .Any(syntax => syntax.Modifiers.Any(SyntaxKind.PartialKeyword));
    }

    /// <summary>
    ///     Collects the exclusion-relevant facts about the hand-written members of the marked type: their rewritten
    ///     signatures, and the IDs of the BCL overloads they forward to. Called from the syntax-anchored part of
    ///     the incremental pipeline, so it only re-runs when the marked type's syntax changes.
    /// </summary>
    public static HandWrittenInfo CollectHandWrittenInfo(Compilation compilation, INamedTypeSymbol markedType)
    {
        var fileType = compilation.GetTypeByMetadataName("System.IO.File");
        var directoryType = compilation.GetTypeByMetadataName("System.IO.Directory");
        var absolutePathType = compilation.GetTypeByMetadataName(PathIoRules.AbsolutePathTypeName);
        if (fileType == null || directoryType == null || absolutePathType == null) return HandWrittenInfo.Empty;

        var renderer = TypeRenderer.Create(compilation);
        var signatures = new HashSet<string>();
        var forwardedDocIds = new HashSet<string>();
        var comparer = SymbolEqualityComparer.Default;

        foreach (var member in markedType.GetMembers().OfType<IMethodSymbol>())
        {
            signatures.Add(SignatureKey(member.Name, member.Parameters.Select(p => TypeDisplay(p, null, absolutePathType, renderer))));

            foreach (var reference in member.DeclaringSyntaxReferences)
            {
                var syntax = reference.GetSyntax();
                var model = compilation.GetSemanticModel(syntax.SyntaxTree);
                foreach (var invocation in syntax.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    if (invocation.ArgumentList.Arguments.Count == 0) continue;
                    if (model.GetSymbolInfo(invocation).Symbol is not IMethodSymbol target) continue;

                    var owner = target.ContainingType?.OriginalDefinition;
                    if (!comparer.Equals(owner, fileType.OriginalDefinition)
                        && !comparer.Equals(owner, directoryType.OriginalDefinition)) continue;

                    if (!FirstArgumentDerivesFromPathParameter(
                            invocation.ArgumentList.Arguments[0], member, absolutePathType)) continue;

                    forwardedDocIds.Add(DocId.ForMethod(target));
                }
            }
        }

        return new HandWrittenInfo(
            signatures.OrderBy(signature => signature, StringComparer.Ordinal).ToImmutableArray(),
            forwardedDocIds.OrderBy(docId => docId, StringComparer.Ordinal).ToImmutableArray());
    }

    /// <summary>
    ///     Matches the forwarding pattern `SomeBclCall(path.Value, …)` where `path` is a parameter of the handwritten
    ///     method typed as <c>AbsolutePath</c>. Anything less direct is deliberately not treated as a forwarding call,
    ///     so that incidental BCL calls inside custom logic cannot silently suppress generation.
    /// </summary>
    private static bool FirstArgumentDerivesFromPathParameter(
        ArgumentSyntax argument,
        IMethodSymbol enclosingMethod,
        INamedTypeSymbol absolutePathType)
    {
        if (argument.Expression is not MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Value" } access) return false;
        if (access.Expression is not IdentifierNameSyntax identifier) return false;

        var parameter = enclosingMethod.Parameters.FirstOrDefault(p => p.Name == identifier.Identifier.ValueText);
        return parameter != null
               && SymbolEqualityComparer.Default.Equals(parameter.Type.OriginalDefinition, absolutePathType.OriginalDefinition);
    }

    private static GeneratedMethod BuildMethod(
        INamedTypeSymbol owner,
        IMethodSymbol method,
        ImmutableHashSet<int> pathOrdinals,
        INamedTypeSymbol absolutePathType,
        BclDocIndex docIndex,
        TypeRenderer renderer)
    {
        var parameterNames = method.Parameters
            .Select(p => pathOrdinals.Contains(p.Ordinal)
                ? PathIoRules.GeneratedParameterName(p.Name)
                : p.Name)
            .ToImmutableArray();
        return new GeneratedMethod(
            method.Name,
            renderer.Render(method.ReturnType),
            method.Parameters
                .Select(p => new GeneratedParameter(
                    TypeDisplay(p, pathOrdinals, absolutePathType, renderer),
                    parameterNames[p.Ordinal],
                    pathOrdinals.Contains(p.Ordinal),
                    RenderDefaultValue(p, renderer)))
                .ToImmutableArray(),
            renderer.Render(owner) + "." + method.Name,
            method.GetAttributes()
                .Where(a => a.AttributeClass != null && ForwardedAttributes.Contains(a.AttributeClass.ToDisplayString()))
                .Select(attribute => RenderAttribute(attribute, renderer))
                .ToImmutableArray(),
            BuildDocLines(method, docIndex, parameterNames));
    }

    private static string? RenderDefaultValue(IParameterSymbol parameter, TypeRenderer renderer)
    {
        if (!parameter.IsOptional) return null;
        if (!parameter.HasExplicitDefaultValue) return "default";

        var value = parameter.ExplicitDefaultValue;
        switch (value)
        {
            // A null explicit default on an optional parameter means default(T): true null for reference types,
            // and a null-valued default for value types such as CancellationToken.
            case null:
                return parameter.Type.IsValueType ? "default" : "null";
            case bool b:
                return b ? "true" : "false";
            case string s:
                return SymbolDisplay.FormatLiteral(s, quote: true);
        }

        if (parameter.Type.TypeKind != TypeKind.Enum || parameter.Type is not INamedTypeSymbol enumType)
            return "default";

        var field = enumType.GetMembers().OfType<IFieldSymbol>()
            .FirstOrDefault(f => f.IsConst && Equals(f.ConstantValue, value));
        if (field != null) return renderer.Render(enumType) + "." + field.Name;

        var number = Convert.ToString(
            Convert.ToInt64(value, CultureInfo.InvariantCulture),
            CultureInfo.InvariantCulture);
        return "(" + renderer.Render(enumType) + ")" + number;

    }

    private static string RenderAttribute(AttributeData attribute, TypeRenderer renderer)
    {
        var arguments = string.Join(", ", attribute.ConstructorArguments
            .Select(a => a.Value switch
            {
                string s => SymbolDisplay.FormatLiteral(s, quote: true),
                bool b => b ? "true" : "false",
                _ => null,
            })
            .Where(rendered => rendered != null));

        var name = renderer.Render(attribute.AttributeClass!);
        return "[" + name + (arguments.Length == 0 ? "" : "(" + arguments + ")") + "]";
    }
    private static ImmutableArray<string> BuildDocLines(
        IMethodSymbol method,
        BclDocIndex docIndex,
        ImmutableArray<string> parameterNames)
    {
        if (docIndex.TryGetMethodMember(DocId.ForMethod(method), out var cachedMember))
            return DocFormatting.ToDocLines(cachedMember, parameterNames);

        var xml = method.GetDocumentationCommentXml();
        if (!string.IsNullOrWhiteSpace(xml))
            try
            {
                var root = XElement.Parse("<root>" + xml!.Trim() + "</root>");
                if (root.Element("member") is { } member)
                    return DocFormatting.ToDocLines(member, parameterNames);
            }
            catch (Exception)
            {
                // Malformed documentation falls back to the generated summary below.
            }

        var cref = DocId.ForCref(method);
        return
        [
            $"<summary>Equivalent to <see cref=\"{cref}\"/>.</summary>",
            .. parameterNames.Select(name =>
                $"<param name=\"{name}\">See the documentation of <see cref=\"{cref}\"/>.</param>")
        ];
    }

    private static string TypeDisplay(
        IParameterSymbol parameter,
        ImmutableHashSet<int>? pathOrdinals,
        INamedTypeSymbol absolutePathType,
        TypeRenderer renderer)
    {
        var isPath = parameter.Type.SpecialType == SpecialType.System_String
                     && pathOrdinals != null
                     && pathOrdinals.Contains(parameter.Ordinal);
        return isPath ? renderer.Render(absolutePathType) : renderer.Render(parameter.Type);
    }

    private static string SignatureKey(string name, IEnumerable<string> parameterTypes)
    {
        return name + "(" + string.Join(", ", parameterTypes) + ")";
    }

    private static string RenderFile(INamedTypeSymbol markedType,
        List<GeneratedMethod> methods,
        SortedSet<string> skippedWithoutPath,
        TypeRenderer renderer)
    {
        var orderedMethods = methods
            .OrderBy(m => m.Name, StringComparer.Ordinal)
            .ThenBy(m => m.Parameters.Length)
            .ThenBy(m => string.Join(", ", m.Parameters.Select(p => p.Type)), StringComparer.Ordinal)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated>");
        sb.AppendLine("//     Generated by the TruePath.PathIoGenerator source generator. Manual changes will be lost.");
        sb.AppendLine("// </auto-generated>");
        sb.AppendLine();
        // REUSE-IgnoreStart: these are string literals for the generated file's header, not this file's tags.
        sb.AppendLine("// SPDX-FileCopyrightText: 2025 .NET Foundation and Contributors");
        sb.AppendLine("// SPDX-FileCopyrightText: 2025-2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>");
        sb.AppendLine("//");
        sb.AppendLine("// SPDX-License-Identifier: MIT");
        // REUSE-IgnoreEnd
        sb.AppendLine();
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        foreach (var importedNamespace in renderer.ImportedNamespaces.OrderBy(namespaceName => namespaceName, StringComparer.Ordinal))
            sb.AppendLine("using " + importedNamespace + ";");
        sb.AppendLine();
        sb.AppendLine("namespace " + markedType.ContainingNamespace.ToDisplayString() + ";");
        sb.AppendLine();
        sb.AppendLine("[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"" + GeneratorName + "\", \"" + GeneratorVersion + "\")]");
        sb.AppendLine("public static partial class " + markedType.Name);
        sb.AppendLine("{");

        for (var memberIndex = 0; memberIndex < orderedMethods.Count; memberIndex++)
        {
            var method = orderedMethods[memberIndex];
            if (memberIndex > 0) sb.AppendLine();

            foreach (var line in method.DocLines) sb.AppendLine("    /// " + line);
            foreach (var attribute in method.Attributes) sb.AppendLine("    " + attribute);

            var parameters = string.Join(", ", method.Parameters.Select((p, parameterIndex) =>
                (parameterIndex == 0 ? "this " : "") + p.Type + " " + p.Name
                + (p.DefaultValue == null ? "" : " = " + p.DefaultValue)));
            var arguments = string.Join(", ", method.Parameters.Select(p => p.IsPath ? p.Name + ".Value" : p.Name));
            sb.Append("    public static ").Append(method.ReturnType).Append(' ').Append(method.Name)
                .Append('(').Append(parameters).Append(')')
                .Append(" => ").Append(method.BclCall).Append('(').Append(arguments).Append(");");
            sb.AppendLine();
        }

        if (skippedWithoutPath.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("    // BCL methods without a path parameter in position zero are not generated:");
            foreach (var docId in skippedWithoutPath) sb.AppendLine("    //     " + docId);
        }

        sb.AppendLine("}");
        return sb.ToString();
    }
}

public sealed class HandWrittenInfo(
    ImmutableArray<string> signatures,
    ImmutableArray<string> forwardedDocIds)
{
    public static HandWrittenInfo Empty { get; } = new(ImmutableArray<string>.Empty, ImmutableArray<string>.Empty);

    public ImmutableArray<string> Signatures { get; } = signatures;
    public ImmutableArray<string> ForwardedDocIds { get; } = forwardedDocIds;

    public override bool Equals(object? obj)
    {
        return obj is HandWrittenInfo other
               && Signatures.SequenceEqual(other.Signatures)
               && ForwardedDocIds.SequenceEqual(other.ForwardedDocIds);
    }

    public override int GetHashCode()
    {
        return Signatures.Length;
    }
}

internal sealed class GeneratedMethod(
    string name,
    string returnType,
    ImmutableArray<GeneratedParameter> parameters,
    string bclCall,
    ImmutableArray<string> attributes,
    ImmutableArray<string> docLines)
{
    public string Name { get; } = name;
    public string ReturnType { get; } = returnType;
    public ImmutableArray<GeneratedParameter> Parameters { get; } = parameters;
    public string BclCall { get; } = bclCall;
    public ImmutableArray<string> Attributes { get; } = attributes;
    public ImmutableArray<string> DocLines { get; } = docLines;
}

internal sealed class GeneratedParameter(string type, string name, bool isPath, string? defaultValue)
{
    public string Type { get; } = type;
    public string Name { get; } = name;
    public bool IsPath { get; } = isPath;
    public string? DefaultValue { get; } = defaultValue;
}
