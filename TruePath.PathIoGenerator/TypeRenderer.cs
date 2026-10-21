// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace TruePath.PathIoGenerator;

/// <summary>
///     Renders type names for the generated source: types from the namespaces imported by the generated file use
///     their short names, and everything else is namespace-qualified. The imported namespace set is also the
///     single source of truth for the `using` directives the generated file emits, so the two cannot diverge.
/// </summary>
internal sealed class TypeRenderer
{
    private readonly ImmutableHashSet<string> _importedNamespaces;

    private TypeRenderer(ImmutableHashSet<string> importedNamespaces)
    {
        _importedNamespaces = importedNamespaces;
        ImportedNamespaces = importedNamespaces;
    }

    public ImmutableHashSet<string> ImportedNamespaces { get; }

    public static TypeRenderer Create(Compilation compilation)
    {
        var namespaces = ImmutableHashSet.Create(
            "System",
            "System.Collections.Generic",
            "System.IO",
            "System.Text",
            "System.Threading",
            "System.Threading.Tasks",
            "TruePath");

        // Conditional namespaces must only be imported when the target framework has their types.
        if (compilation.GetTypeByMetadataName("Microsoft.Win32.SafeHandles.SafeFileHandle") != null)
            namespaces = namespaces.Add("Microsoft.Win32.SafeHandles");
        if (compilation.GetTypeByMetadataName("System.Runtime.Versioning.UnsupportedOSPlatformAttribute") != null)
            namespaces = namespaces.Add("System.Runtime.Versioning");

        return new TypeRenderer(namespaces);
    }

    public string Render(ITypeSymbol type)
    {
        var name = type switch
        {
            IArrayTypeSymbol array => Render(array.ElementType)
                + (array.Rank == 1 ? "[]" : "[" + new string(',', array.Rank - 1) + "]"),
            INamedTypeSymbol named => RenderNamed(named),
            _ => type.ToDisplayString(),
        };

        name = type.SpecialType switch
        {
            SpecialType.System_String => "string",
            SpecialType.System_Char => "char",
            SpecialType.System_Byte => "byte",
            SpecialType.System_SByte => "sbyte",
            SpecialType.System_Int16 => "short",
            SpecialType.System_UInt16 => "ushort",
            SpecialType.System_Int32 => "int",
            SpecialType.System_UInt32 => "uint",
            SpecialType.System_Int64 => "long",
            SpecialType.System_UInt64 => "ulong",
            SpecialType.System_Single => "float",
            SpecialType.System_Double => "double",
            SpecialType.System_Decimal => "decimal",
            SpecialType.System_Boolean => "bool",
            SpecialType.System_Object => "object",
            SpecialType.System_Void => "void",
            _ => name,
        };

        if (type.IsReferenceType && type.NullableAnnotation == NullableAnnotation.Annotated) name += "?";
        return name;
    }

    private string RenderNamed(INamedTypeSymbol named)
    {
        var namespaceName = named.ContainingNamespace is { IsGlobalNamespace: false } namespaceSymbol
            ? namespaceSymbol.ToDisplayString()
            : string.Empty;
        var qualifier = _importedNamespaces.Contains(namespaceName) ? string.Empty : namespaceName + ".";

        var result = qualifier + ContainingTypesPrefix(named) + named.Name;
        if (named.TypeArguments.Length > 0)
            result += "<" + string.Join(", ", named.TypeArguments.Select(Render)) + ">";
        return result;
    }

    private static string ContainingTypesPrefix(INamedTypeSymbol named)
    {
        var names = new List<string>();
        for (var containing = named.ContainingType; containing != null; containing = containing.ContainingType)
            names.Insert(0, containing.Name);
        return names.Count == 0 ? string.Empty : string.Join(".", names) + ".";
    }
}
