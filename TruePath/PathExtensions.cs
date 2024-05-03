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
    /// <para>File name entirely consisting of extension, such as <c>.gitignore</c>, is returned as-is.</para>
    /// </summary>
    /// <returns>The extension of the file name of the path with the dot character (if present).</returns>
    /// <remarks>
    ///     This method will return an empty string for paths without extensions, and will return a dot for paths whose
    ///     names end with a dot (even though it is an unusual path). This behavior allows to distinguish such paths,
    ///     and potentially reconstruct the file name from its part without the extension and the "extension with dot".
    /// </remarks>
    public static string GetExtensionWithDot(this IPath path) =>
        path.FileName.EndsWith('.') ? "." : Path.GetExtension(path.FileName);

    /// <summary>
    /// <para>Gets the extension of the file name of the <paramref name="path"/> without the dot character.</para>
    /// <para>For example, for the path <c>file.txt</c>, this method will return a string <c>txt</c>.</para>
    /// <para>
    ///     File name entirely consisting of extension, such as <c>.gitignore</c>, is returned with its leading dot
    ///     trimmed.
    /// </para>
    /// </summary>
    /// <returns>The extension of the file name of the path without the dot.</returns>
    /// <remarks>
    ///     This method will return an empty string for paths without extensions and with empty extensions (ending with
    ///     dot, which may be unusual). This behavior doesn't allow to distinguish such paths using this method, to
    ///     reconstruct the original name from its name without extension and its extension without dot.
    /// </remarks>
    public static string GetExtensionWithoutDot(this IPath path) => GetExtensionWithDot(path).TrimStart('.');
}
