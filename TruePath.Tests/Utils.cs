// SPDX-FileCopyrightText: 2024-2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using Xunit.v3;

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
        OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsIOS() || OperatingSystem.IsTvOS();

    internal static bool RunsOnCi()
    {
        return Environment.GetEnvironmentVariable("CI") is not null;
    }

    /// <summary>
    /// Changes the current directory to <paramref name="newDirectory"/>, and restores the previous one on disposal.
    /// </summary>
    /// <remarks>Only allowed in tests of a collection with parallelization disabled, such as
    /// <see cref="CurrentDirectoryCollection"/>.</remarks>
    internal static IDisposable ChangeCurrentDirectory(AbsolutePath newDirectory)
    {
        if (TestContext.Current.TestCollection is not IXunitTestCollection { DisableParallelization: true })
            throw new InvalidOperationException(
                "Changing the current directory is only allowed in tests of a collection with parallelization " +
                $"disabled, such as {nameof(CurrentDirectoryCollection)}.");

        var previousDirectory = AbsolutePath.CurrentWorkingDirectory;
        AbsolutePath.CurrentWorkingDirectory = newDirectory;
        return new CurrentDirectoryRestorer(previousDirectory);
    }

    private sealed class CurrentDirectoryRestorer(AbsolutePath previousDirectory) : IDisposable
    {
        public void Dispose() => AbsolutePath.CurrentWorkingDirectory = previousDirectory;
    }
}
