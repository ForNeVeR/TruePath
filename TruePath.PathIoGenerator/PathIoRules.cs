// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath.PathIoGenerator;

/// <summary>
///     Naming and matching rules of the PathIo API generation, shared between the generator itself and the tests
///     that verify the completeness of the generated API surface.
/// </summary>
public static class PathIoRules
{
    public const string MarkerAttributeName = "TruePath.SystemIo.GenerateSystemIoExtensionsAttribute";
    public const string AbsolutePathTypeName = "TruePath.AbsolutePath";
    public const string ExclusionFileName = "path-io-exclusions.txt";

    /// <summary>
    ///     Determines whether a <c>System.String</c>-typed parameter of a <c>System.IO.File</c> or
    ///     <c>System.IO.Directory</c> method is a path parameter, i.e. one that becomes a
    ///     <c>TruePath.AbsolutePath</c> in the generated API. This covers the parameter naming conventions actually
    ///     used by the BCL: <c>path</c>/<c>linkPath</c>/<c>pathToTarget</c>, <c>sourceFile</c>/<c>destFile</c>, and
    ///     the .NET Framework-era <c>sourceFileName</c>/<c>destDirName</c> spellings.
    /// </summary>
    public static bool IsPathParameterName(string parameterName)
    {
        return parameterName.Contains("path", StringComparison.OrdinalIgnoreCase)
               || parameterName.EndsWith("file", StringComparison.OrdinalIgnoreCase)
               || parameterName.EndsWith("dir", StringComparison.OrdinalIgnoreCase)
               || parameterName.EndsWith("filename", StringComparison.OrdinalIgnoreCase)
               || parameterName.EndsWith("dirname", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     The parameter name a path parameter gets in the generated API: the BCL `…FileName`/`…DirName` spellings
    ///     lose their `Name` suffix, so that e.g. <c>Copy</c> keeps the <c>sourceFile</c>/<c>destFile</c> parameter
    ///     names the library used when they were still hand-written.
    /// </summary>
    public static string GeneratedParameterName(string bclParameterName)
    {
        return bclParameterName.Length > 4 && bclParameterName.EndsWith("Name", StringComparison.OrdinalIgnoreCase)
            ? bclParameterName.Substring(0, bclParameterName.Length - "Name".Length)
            : bclParameterName;
    }
}
