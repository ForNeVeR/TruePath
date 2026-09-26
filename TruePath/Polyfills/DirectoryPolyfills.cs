// SPDX-FileCopyrightText: 2025-2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

#if !NET8_0_OR_GREATER

namespace TruePath.Polyfills;

/// <summary>
/// Class that contains custom implementations methods of <see cref="Directory"/> class presented in .NET 8 but missing in .NET Standard 2.0.
/// </summary>
internal static class DirectoryPolyfills
{
    public static DirectoryInfo CreateTempSubdirectory(string? prefix = null)
    {
        var tempPath = Path.GetTempPath();
        var directoryName = BuildDirectoryName(prefix);
        var fullPath = Path.Combine(tempPath, directoryName);

        return Directory.CreateDirectory(fullPath);
    }

    private static string BuildDirectoryName(string? prefix)
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss");
        var randomPart = Path.GetRandomFileName().Replace(".", "");

        return prefix != null
            ? $"{prefix}_{timestamp}_{randomPart}"
            : $"tmp_{timestamp}_{randomPart}";
    }
}
#endif
