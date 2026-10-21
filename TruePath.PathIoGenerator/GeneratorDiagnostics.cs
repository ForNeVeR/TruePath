// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;

namespace TruePath.PathIoGenerator;

internal static class GeneratorDiagnostics
{
    private const string Category = "PathIoGenerator";

    public static readonly DiagnosticDescriptor RequiredTypeNotFound = new(
        id: "TPG001",
        title: "Required type not found",
        messageFormat: "The type '{0}' was not found in the compilation, so the PathIo source generator cannot run",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MarkedClassNotPartial = new(
        id: "TPG002",
        title: "Marked class is not partial",
        messageFormat: "The class '{0}' is marked with GenerateSystemIoExtensionsAttribute, but it is not declared as partial",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MarkedClassIsNested = new(
        id: "TPG004",
        title: "Marked class is nested",
        messageFormat: "The class '{0}' is marked with GenerateSystemIoExtensionsAttribute, but it is nested inside another type, which is not supported",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidExclusionEntry = new(
        id: "TPG003",
        title: "Invalid exclusion list entry",
        messageFormat: "The exclusion list line '{0}' is ignored: it is not a method documentation ID starting with 'M:'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
