// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath;

/// <summary>
/// Provides methods for working with temporary files, directories and folders.
/// </summary>
public static class Temporary
{

    /// <summary>
    /// Gets the system's temporary directory.
    /// </summary>
    /// <returns>An AbsolutePath representing the system's temporary directory.</returns>
    public static AbsolutePath SystemTempDirectory()
    {
        var tempPath = Path.GetTempPath();
        return new AbsolutePath(tempPath);
    }

    /// <summary>
    /// Creates a temporary file.
    /// </summary>
    /// <returns>An AbsolutePath representing the newly created temporary file.</returns>
    public static AbsolutePath CreateTempFile()
    {
        var tempPath = Path.GetTempFileName();
        return new AbsolutePath(tempPath);
    }

    /// <summary>
    /// Creates a temporary folder with the specified prefix.
    /// </summary>
    /// <param name="prefix">An optional string to add to the beginning of the subdirectory name.</param>
    /// <returns>An AbsolutePath representing newly created temporary folder</returns>
    public static AbsolutePath CreateTempFolder(string? prefix = null)
    {
        var tempDirectoryInfo = Directory.CreateTempSubdirectory(prefix);

        return new AbsolutePath(tempDirectoryInfo.FullName);
    }
}
