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
        path.FileName.EndsWith(".") ? "." : Path.GetExtension(path.FileName);

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

    /// <summary>
    /// <para>Gets the file name of the <paramref name="path"/> without the extension.</para>
    /// <para>For example, for the path <c>file.txt</c>, this method will return a string <c>file</c>.</para>
    /// </summary>
    /// <returns>
    /// The file name of the path without the extension. If the path has no extension, the file name is returned as-is
    /// (one trailing dot will be stripped, though).
    /// </returns>
    public static string GetFilenameWithoutExtension(this IPath path) =>
        Path.GetFileNameWithoutExtension(path.FileName);

#if NET8_0_OR_GREATER
    /// <summary>
    /// Returns a new path of the same type <typeparamref name="TPath"/> with the extension of its file name
    /// component changed.
    /// </summary>
    /// <typeparam name="TPath">The type of the path, which must implement <see cref="IPath{TPath}"/>.</typeparam>
    /// <param name="path">The original path.</param>
    /// <param name="extension">
    ///     <para>
    ///         The new extension, either with or without the leading dot: both <c>txt</c> and <c>.txt</c> give the
    ///         same result, since the dot is added when missing.
    ///     </para>
    ///     <para>
    ///         Pass <see langword="null"/> to remove the extension entirely (<c>file.txt</c> becomes <c>file</c>), or
    ///         an empty string to remove it but keep the trailing dot (<c>file.txt</c> becomes <c>file.</c>).
    ///     </para>
    ///     <para>
    ///         The extension may not contain a directory separator, and neither <see langword="null"/>, an empty
    ///         string nor a lone dot may be passed for a file name that consists entirely of an extension: see the
    ///         exception below.
    ///     </para>
    /// </param>
    /// <returns>
    /// A new path of type <typeparamref name="TPath"/> with the modified file name component.
    /// The original <paramref name="path"/> object is not modified.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     <para>
    ///         Thrown if the <paramref name="path"/> has no file name to change: it is empty (which designates the
    ///         current directory), it consists of a root or a drive designator alone, such as <c>C:\</c>, <c>/</c>
    ///         or <c>C:</c>, or its last segment is a parent directory reference (<c>..</c>).
    ///     </para>
    ///     <para>
    ///         Also thrown if the requested change would leave something other than a file name in the file name's
    ///         place: <c>C:\foo\.gitignore</c> with <see langword="null"/> would leave nothing at all, and
    ///         <c>file.txt</c> with the extension <c>foo/bar</c> would leave a whole new segment.
    ///     </para>
    /// </exception>
    /// <remarks>
    ///     <para>
    ///         Only the last extension is replaced: for <c>archive.tar.gz</c> and the extension <c>zip</c>, the result
    ///         is <c>archive.tar.zip</c>. The extension itself may contain dots, so <c>.tar.gz</c> applied to
    ///         <c>archive.zip</c> gives <c>archive.tar.gz</c>.
    ///     </para>
    ///     <para>If the file name has no extension, the new one is appended: <c>file</c> becomes <c>file.txt</c>.</para>
    ///     <para>
    ///         A file name consisting entirely of an extension is treated as an extension, consistently with
    ///         <see cref="GetExtensionWithDot"/>: <c>.gitignore</c> with the extension <c>hgignore</c> becomes
    ///         <c>.hgignore</c>. <b>Removing</b> the extension of such a name is impossible, though: nothing would
    ///         be left of the file name.
    ///     </para>
    ///     <para>
    ///         This method changes the extension and nothing else: the path always ends with a file name both
    ///         before and after the call, and the number and the kind of its segments are always the same. Whenever
    ///         the requested change would break that, it throws instead of returning a path of a different shape.
    ///     </para>
    /// </remarks>
    public static TPath WithExtension<TPath>(this TPath path, string? extension) where TPath : IPath<TPath>
    {
        var p = (IPath)path;
        return TPath.Create(ChangeExtension(p.Value, p.FileName, extension));
    }
#else
    /// <summary>
    /// Returns a new path of type <see cref="AbsolutePath"/> with the extension of its file name component changed.
    /// </summary>
    /// <param name="path">The original path.</param>
    /// <param name="extension">
    ///     <para>
    ///         The new extension, either with or without the leading dot: both <c>txt</c> and <c>.txt</c> give the
    ///         same result, since the dot is added when missing.
    ///     </para>
    ///     <para>
    ///         Pass <see langword="null"/> to remove the extension entirely (<c>file.txt</c> becomes <c>file</c>), or
    ///         an empty string to remove it but keep the trailing dot (<c>file.txt</c> becomes <c>file.</c>).
    ///     </para>
    ///     <para>
    ///         The extension may not contain a directory separator, and neither <see langword="null"/>, an empty
    ///         string nor a lone dot may be passed for a file name that consists entirely of an extension: see the
    ///         exception below.
    ///     </para>
    /// </param>
    /// <returns>
    /// A new path of type <see cref="AbsolutePath"/> with the modified file name component.
    /// The original <paramref name="path"/> object is not modified.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     <para>
    ///         Thrown if the <paramref name="path"/> has no file name to change: it is empty (which designates the
    ///         current directory), it consists of a root or a drive designator alone, such as <c>C:\</c>, <c>/</c>
    ///         or <c>C:</c>, or its last segment is a parent directory reference (<c>..</c>).
    ///     </para>
    ///     <para>
    ///         Also thrown if the requested change would leave something other than a file name in the file name's
    ///         place: <c>C:\foo\.gitignore</c> with <see langword="null"/> would leave nothing at all, and
    ///         <c>file.txt</c> with the extension <c>foo/bar</c> would leave a whole new segment.
    ///     </para>
    /// </exception>
    /// <remarks>
    ///     <para>
    ///         Only the last extension is replaced: for <c>archive.tar.gz</c> and the extension <c>zip</c>, the result
    ///         is <c>archive.tar.zip</c>. The extension itself may contain dots, so <c>.tar.gz</c> applied to
    ///         <c>archive.zip</c> gives <c>archive.tar.gz</c>.
    ///     </para>
    ///     <para>If the file name has no extension, the new one is appended: <c>file</c> becomes <c>file.txt</c>.</para>
    ///     <para>
    ///         A file name consisting entirely of an extension is treated as an extension, consistently with
    ///         <see cref="GetExtensionWithDot"/>: <c>.gitignore</c> with the extension <c>hgignore</c> becomes
    ///         <c>.hgignore</c>. <b>Removing</b> the extension of such a name is impossible, though: nothing would
    ///         be left of the file name.
    ///     </para>
    ///     <para>
    ///         This method changes the extension and nothing else: the path always ends with a file name both
    ///         before and after the call, and the number and the kind of its segments are always the same. Whenever
    ///         the requested change would break that, it throws instead of returning a path of a different shape.
    ///     </para>
    /// </remarks>
    public static AbsolutePath WithExtension(this AbsolutePath path, string? extension) =>
        AbsolutePath.Create(ChangeExtension(path.Value, path.FileName, extension));

    /// <summary>
    /// Returns a new path of type <see cref="LocalPath"/> with the extension of its file name component changed.
    /// </summary>
    /// <param name="path">The original path.</param>
    /// <param name="extension">
    ///     <para>
    ///         The new extension, either with or without the leading dot: both <c>txt</c> and <c>.txt</c> give the
    ///         same result, since the dot is added when missing.
    ///     </para>
    ///     <para>
    ///         Pass <see langword="null"/> to remove the extension entirely (<c>file.txt</c> becomes <c>file</c>), or
    ///         an empty string to remove it but keep the trailing dot (<c>file.txt</c> becomes <c>file.</c>).
    ///     </para>
    ///     <para>
    ///         The extension may not contain a directory separator, and neither <see langword="null"/>, an empty
    ///         string nor a lone dot may be passed for a file name that consists entirely of an extension: see the
    ///         exception below.
    ///     </para>
    /// </param>
    /// <returns>
    /// A new path of type <see cref="LocalPath"/> with the modified file name component.
    /// The original <paramref name="path"/> object is not modified.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     <para>
    ///         Thrown if the <paramref name="path"/> has no file name to change: it is empty (which designates the
    ///         current directory), it consists of a root or a drive designator alone, such as <c>C:\</c>, <c>/</c>
    ///         or <c>C:</c>, or its last segment is a parent directory reference (<c>..</c>).
    ///     </para>
    ///     <para>
    ///         Also thrown if the requested change would leave something other than a file name in the file name's
    ///         place: <c>C:\foo\.gitignore</c> with <see langword="null"/> would leave nothing at all, and
    ///         <c>file.txt</c> with the extension <c>foo/bar</c> would leave a whole new segment.
    ///     </para>
    /// </exception>
    /// <remarks>
    ///     <para>
    ///         Only the last extension is replaced: for <c>archive.tar.gz</c> and the extension <c>zip</c>, the result
    ///         is <c>archive.tar.zip</c>. The extension itself may contain dots, so <c>.tar.gz</c> applied to
    ///         <c>archive.zip</c> gives <c>archive.tar.gz</c>.
    ///     </para>
    ///     <para>If the file name has no extension, the new one is appended: <c>file</c> becomes <c>file.txt</c>.</para>
    ///     <para>
    ///         A file name consisting entirely of an extension is treated as an extension, consistently with
    ///         <see cref="GetExtensionWithDot"/>: <c>.gitignore</c> with the extension <c>hgignore</c> becomes
    ///         <c>.hgignore</c>. <b>Removing</b> the extension of such a name is impossible, though: nothing would
    ///         be left of the file name.
    ///     </para>
    ///     <para>
    ///         This method changes the extension and nothing else: the path always ends with a file name both
    ///         before and after the call, and the number and the kind of its segments are always the same. Whenever
    ///         the requested change would break that, it throws instead of returning a path of a different shape.
    ///     </para>
    /// </remarks>
    public static LocalPath WithExtension(this LocalPath path, string? extension) =>
        LocalPath.Create(ChangeExtension(path.Value, path.FileName, extension));
#endif

    private static bool IsFileName(string segment) =>
        segment.Length > 0
        && segment is not ("." or "..")
        && segment.IndexOf(Path.DirectorySeparatorChar) < 0
        && segment.IndexOf(Path.AltDirectorySeparatorChar) < 0;

    private static string ChangeExtension(string value, string fileName, string? extension)
    {
        if (!IsFileName(fileName))
            throw new ArgumentException(
                $"Path \"{value}\" does not end with a file name, so its extension cannot be changed.",
                "path");

        // A file name consisting entirely of an extension, such as ".gitignore", has nothing left once the extension
        // is removed: the new file name would either be empty or a lone dot that the normalization drops, and so the
        // path would lose a segment.
        var newFileName = Path.ChangeExtension(fileName, extension);
        if (!IsFileName(newFileName))
        {
            var extensionText = extension is null ? "null" : $"\"{extension}\"";
            throw new ArgumentException(
                $"Changing the extension of path \"{value}\" to {extensionText} would replace its file name " +
                $"\"{fileName}\" with \"{newFileName}\", which is not a file name.",
                nameof(extension));
        }

        return Path.ChangeExtension(value, extension);
    }
}
