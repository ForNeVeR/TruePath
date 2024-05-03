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
    /// <para>Gets the extension of the file name of the <paramref name="path"/> with the dot character.</para>
    /// <para>For example, for the path <c>file.txt</c>, this method will return a string <c>.txt</c>.</para>
    /// <para>
    ///     <b>Note</b> that this method will return <c>null</c> for paths without extensions, and will return an empty
    ///     string for paths whose names end with a dot (even though it is an unusual path). This behavior allows to
    ///     distinguish such paths.
    /// </para>
    /// <para>File name entirely consisting of extension, such as <c>.gitignore</c>, is returned as-is.</para>
    /// </summary>
    /// <returns>The extension of the file name of the path with the dot.
    /// </returns>
    public static string? GetExtensionWithDot(this IPath path)
    {
        var fileExtenstion = Path.GetExtension(path.FileName);
        if (string.IsNullOrEmpty(fileExtenstion) && !path.FileName.EndsWith('.'))
            return null;
        return fileExtenstion;
    }

    /// <summary>
    /// <para>Gets the extension of the file name of the <paramref name="path"/> without the dot character.</para>
    /// <para>For example, for the path <c>file.txt</c>, this method will return a string <c>txt</c>.</para>
    /// <para>
    ///     <b>Note</b> that this method will return <c>null</c> for paths without extensions, and will return an empty
    ///     string for paths whose names end with a dot (even though it is an unusual path). This behavior allows to
    ///     distinguish such paths.
    /// </para>
    /// <para>
    ///     File name entirely consisting of extension, such as <c>.gitignore</c>, is returned with its leading dot
    ///     trimmed.
    /// </para>
    /// </summary>
    /// <returns>
    /// The extension of the file name of the path without the dot.
    /// </returns>
    public static string? GetExtensionWithoutDot(this IPath path) => GetExtensionWithDot(path)?.TrimStart('.');
}
