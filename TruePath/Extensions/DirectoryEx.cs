// SPDX-FileCopyrightText: 2025 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

#if !NET8_0_OR_GREATER
// ReSharper disable once CheckNamespace
namespace System.IO;

/// <summary>
/// Class that contains custom implementations methods of <see cref="Directory"/> class presented in .NET 8 but missing in .NET Standard 2.0.
/// </summary>
public static class DirectoryEx
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
