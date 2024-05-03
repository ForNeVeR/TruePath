// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath;
/// <summary>
/// Extension methods for <see cref="IPath"/> and <see cref="IPath{TPath}"/>.
/// </summary>
public static class PathExtensions
{
    /// <summary>
    /// Gets the extension of the file name of the path with the dot.
    /// </summary>
    /// <param name="path"></param>
    /// <returns>
    /// The extension of the file name of the path with the dot.
    /// </returns>
    public static string? GetExtensionWithDot(this IPath path)
    {
        var fileExtenstion = Path.GetExtension(path.FileName);
        if (string.IsNullOrEmpty(fileExtenstion) && !path.FileName.EndsWith('.'))
            return null;
        return fileExtenstion;
    }
    /// <summary>
    /// Gets the extension of the file name of the path without the dot.
    /// </summary>
    /// <param name="path"></param>
    /// <returns>
    /// The extension of the file name of the path without the dot.
    /// </returns>
    public static string? GetExtensionWithoutDot(this IPath path)
    {
        return GetExtensionWithDot(path)?.TrimStart('.');
    }
}
