// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;

namespace TruePath.PathIoGenerator;

/// <summary>
///     Formats XML documentation IDs ("M:…") for the methods this generator works with. Only covers the shape of the
///     <c>System.IO.File</c>/<c>System.IO.Directory</c> signatures: named types, arrays, and closed generic
///     instantiations; no by-reference, pointer, or generic-method parameters occur there.
/// </summary>
internal static class DocId
{
    public static string ForMethod(IMethodSymbol method)
    {
        var parameters = string.Join(",", method.Parameters.Select(p => TypeReference(p.Type)));
        return $"M:{TypeReference(method.ContainingType)}.{method.Name}({parameters})";
    }

    /// <summary>Strips the "M:" prefix, so the result is usable as a <c>cref</c> attribute value.</summary>
    public static string ForCref(IMethodSymbol method)
    {
        return ForMethod(method).Substring(2);
    }

    private static string TypeReference(ITypeSymbol type)
    {
        switch (type.SpecialType)
        {
            case SpecialType.System_String:
                return "System.String";
            case SpecialType.System_Int32:
                return "System.Int32";
            case SpecialType.System_Int64:
                return "System.Int64";
            case SpecialType.System_Boolean:
                return "System.Boolean";
            case SpecialType.System_Byte:
                return "System.Byte";
            case SpecialType.System_Object:
                return "System.Object";
        }

        switch (type)
        {
            case IArrayTypeSymbol array:
            {
                var suffix = array.Rank == 1 ? "[]" : "[" + new string(',', array.Rank - 1) + "]";
                return TypeReference(array.ElementType) + suffix;
            }
            case INamedTypeSymbol named:
            {
                var name = MetadataName(named);
                if (named.TypeArguments.Length > 0)
                    name += "{" + string.Join(",", named.TypeArguments.Select(TypeReference)) + "}";
                return name;
            }
            default:
                return type.ToDisplayString();
        }
    }

    private static string MetadataName(INamedTypeSymbol type)
    {
        return type.ContainingNamespace is { IsGlobalNamespace: false } namespaceSymbol
            ? namespaceSymbol.ToDisplayString() + "." + type.Name
            : type.Name;
    }
}
