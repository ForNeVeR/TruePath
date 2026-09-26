// SPDX-FileCopyrightText: 2024-2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath.Tests;

public static class Utils
{
    internal static string ToNonCanonicalCase(this string path)
    {
        var result = new char[path.Length];
        for (var i = 0; i < path.Length; i++)
        {
            result[i] = i % 2 == 0 ? char.ToUpper(path[i]) : char.ToLower(path[i]);
        }

        var nonCanonicalPath = new string(result);
        if (path.Equals(nonCanonicalPath, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The non-canonical path is equal to the original path.");
        }

        return nonCanonicalPath;
    }

    /// <summary>A root path that exists syntactically on the current platform: <c>A:\</c> on Windows, <c>/</c> elsewhere.</summary>
    internal static string SyntheticRootString => OperatingSystem.IsWindows() ? @"A:\" : "/";

    /// <inheritdoc cref="SyntheticRootString"/>
    internal static AbsolutePath SyntheticRoot => new(SyntheticRootString);

    internal static AbsolutePath NonCurrentSyntheticRoot =>
        AbsolutePath.CurrentWorkingDirectory.PathRoot == SyntheticRoot
            ? new AbsolutePath(@"B:\")
            : SyntheticRoot;

    internal static bool IsPlatformCaseInsensitive() =>
        OperatingSystem.IsWindows() || OperatingSystem.IsMacOS();

    internal static bool RunsOnCi()
    {
        return Environment.GetEnvironmentVariable("CI") is not null;
    }
}
