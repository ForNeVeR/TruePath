// SPDX-FileCopyrightText: 2025 .NET Foundation and Contributors
// SPDX-FileCopyrightText: 2025-2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath.SystemIo;

/// <summary>
/// Extension methods for the <see cref="AbsolutePath"/> type that provide common I/O operations.
/// </summary>
[GenerateSystemIoExtensions]
public static partial class PathIo
{
    /// <summary>
    /// Creates all directories and subdirectories in the specified path unless they already exist.
    /// </summary>
    /// <param name="path">The directory to create.</param>
    public static void CreateDirectory(this AbsolutePath path) => Directory.CreateDirectory(path.Value);
    /// <summary>
    /// Deletes the specified empty directory.
    /// </summary>
    /// <param name="path">The name of the directory to remove. This directory must be writable and empty.</param>
    public static void DeleteEmptyDirectory(this AbsolutePath path) => Directory.Delete(path.Value, recursive: false);
    /// <summary>
    /// Deletes the specified directory and any subdirectories and files in the directory.
    /// </summary>
    /// <param name="path">The name of the directory to remove. This directory must be writable.</param>
    public static void DeleteDirectoryRecursively(this AbsolutePath path) => Directory.Delete(path.Value, recursive: true);
    /// <summary>
    /// Determines whether the specified file or directory exists.
    /// </summary>
    /// <param name="path">The file or directory to check.</param>
    /// <returns><b>true</b> if the caller has the required permissions and <paramref name="path"/> contains the name of an existing file or directory; otherwise, <b>false</b>. This method also returns <b>false</b> if <paramref name="path"/> is null, an invalid path, or a zero-length string. If the caller does not have sufficient permissions to read the specified path, no exception is thrown and the method returns <b>false</b> regardless of the existence of <paramref name="path"/>.</returns>
    public static bool Exists(this AbsolutePath path) => File.Exists(path.Value) || Directory.Exists(path.Value);
    /// <summary>
    /// Determines whether the specified file exists.
    /// </summary>
    /// <param name="path">The file to check.</param>
    /// <returns><b>true</b> if the caller has the required permissions and <paramref name="path"/> contains the name of an existing file; otherwise, <b>false</b>. This method also returns <b>false</b> if <paramref name="path"/> is null, an invalid path, or a zero-length string. If the caller does not have sufficient permissions to read the specified file, no exception is thrown and the method returns <b>false</b> regardless of the existence of <paramref name="path"/>.</returns>
    public static bool ExistsFile(this AbsolutePath path) => File.Exists(path.Value);
    /// <summary>
    /// Determines whether the given path refers to an existing directory on disk.
    /// </summary>
    /// <param name="path">The path to test.</param>
    /// <returns><b>true</b> if path refers to an existing directory; <b>false</b> if the directory does not exist or an error occurs when trying to determine if the specified directory exists.</returns>
    public static bool ExistsDirectory(this AbsolutePath path) => Directory.Exists(path.Value);

    /// <summary>
    /// Returns an enumerable collection of <see cref="AbsolutePath"/> of full file names in a specified path.
    /// </summary>
    /// <param name="path">The directory to search.</param>
    /// <returns>An enumerable collection of <see cref="AbsolutePath"/> of the full names (including paths) for the files in the specified directory.</returns>
    public static IEnumerable<AbsolutePath> EnumerateFiles(this AbsolutePath path)
    {
        return Directory.EnumerateFiles(path.Value)
            .Select(filename => path / filename);
    }

    /// <summary>
    /// Returns an enumerable collection of <see cref="AbsolutePath"/> of full file names in a specified path.
    /// </summary>
    /// <param name="path">The directory to search.</param>
    /// <param name="searchPattern">
    ///     <para>
    ///     The search string to match against the names of files in <paramref name="path"/>. It may combine
    ///     literal characters with the <c>*</c> and <c>?</c> wildcards, but it does not support regular
    ///     expressions. This overload matches with Win32 semantics, where <c>*</c> stands for zero or more
    ///     characters and <c>?</c> for exactly one, except immediately before a period or at the end of the
    ///     name, where it may also match none: <c>a?.txt</c> matches both <c>ab.txt</c> and <c>a.txt</c>,
    ///     while <c>?ello.txt</c> matches only <c>hello.txt</c>.
    ///     </para>
    ///     <para>
    ///     When running on .NET Framework on Windows, the pattern is also matched against the short (8.3) name of
    ///     each file, if it has one. Since a short name keeps at most three characters of the extension,
    ///     <c>*.htm</c> may also return <c>index.html</c> (whose short name is <c>INDEX~1.HTM</c>), and a pattern
    ///     such as <c>*1*</c> may match many files with no <c>1</c> in their long names, through the <c>~1</c>
    ///     suffix of their short names. Whether a file has a short name depends on the volume settings at the time
    ///     the file was created, so the results may differ between machines and even between files in the same
    ///     directory. See
    ///     <see href="https://devblogs.microsoft.com/oldnewthing/20140313-00/?p=1513">Raymond Chen's explanation</see>
    ///     for details.
    ///     </para>
    ///     <para>
    ///     For the full pattern syntax and its caveats, see
    ///     <see href="https://learn.microsoft.com/dotnet/api/system.io.directory.enumeratefiles"/>.
    ///     </para>
    /// </param>
    /// <returns>An enumerable collection of <see cref="AbsolutePath"/> of the full names (including paths) for the files in the specified directory that match the specified search pattern.</returns>
    public static IEnumerable<AbsolutePath> EnumerateFiles(this AbsolutePath path, string searchPattern)
    {
        return Directory.EnumerateFiles(path.Value, searchPattern)
            .Select(filename => path / filename);
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// Returns an enumerable collection of <see cref="AbsolutePath"/> of the names of files (including their paths) that match the specified search pattern and enumeration options in the specified directory.
    /// </summary>
    /// <param name="path">The directory to search.</param>
    /// <param name="searchPattern">
    /// The search string to match against the names of files in <paramref name="path"/>. It may combine literal
    /// characters with the <c>*</c> (zero or more characters) and <c>?</c> wildcards, but it does not support
    /// regular expressions. Under the default <see cref="MatchType.Simple"/>, <c>?</c> matches exactly one
    /// character; under <see cref="MatchType.Win32"/>, selected through <paramref name="enumerationOptions"/>, it
    /// may also match none immediately before a period or at the end of the name. For the full pattern syntax
    /// and its caveats, see
    /// <see href="https://learn.microsoft.com/dotnet/api/system.io.directory.enumeratefiles"/>.
    /// </param>
    /// <param name="enumerationOptions">An object that contains the search options to use.</param>
    /// <returns>An enumerable collection of <see cref="AbsolutePath"/> of the full names (including paths) for the files in the specified directory that match the specified search pattern and enumeration options.</returns>
    public static IEnumerable<AbsolutePath> EnumerateFiles(this AbsolutePath path, string searchPattern, EnumerationOptions enumerationOptions)
    {
        return Directory.EnumerateFiles(path.Value, searchPattern, enumerationOptions)
            .Select(filename => path / filename);
    }
#endif

    /// <summary>
    /// Returns an enumerable collection of <see cref="AbsolutePath"/> with the names of files (including their paths) that match the specified search pattern in the specified directory, using a value to determine whether to search subdirectories.
    /// </summary>
    /// <param name="path">The directory to search.</param>
    /// <param name="searchPattern">
    ///     <para>
    ///     The search string to match against the names of files in <paramref name="path"/>. It may combine
    ///     literal characters with the <c>*</c> and <c>?</c> wildcards, but it does not support regular
    ///     expressions. This overload matches with Win32 semantics, where <c>*</c> stands for zero or more
    ///     characters and <c>?</c> for exactly one, except immediately before a period or at the end of the
    ///     name, where it may also match none: <c>a?.txt</c> matches both <c>ab.txt</c> and <c>a.txt</c>,
    ///     while <c>?ello.txt</c> matches only <c>hello.txt</c>.
    ///     </para>
    ///     <para>
    ///     When running on .NET Framework on Windows, the pattern is also matched against the short (8.3) name of
    ///     each file, if it has one. Since a short name keeps at most three characters of the extension,
    ///     <c>*.htm</c> may also return <c>index.html</c> (whose short name is <c>INDEX~1.HTM</c>), and a pattern
    ///     such as <c>*1*</c> may match many files with no <c>1</c> in their long names, through the <c>~1</c>
    ///     suffix of their short names. Whether a file has a short name depends on the volume settings at the time
    ///     the file was created, so the results may differ between machines and even between files in the same
    ///     directory. See
    ///     <see href="https://devblogs.microsoft.com/oldnewthing/20140313-00/?p=1513">Raymond Chen's explanation</see>
    ///     for details.
    ///     </para>
    ///     <para>
    ///     For the full pattern syntax and its caveats, see
    ///     <see href="https://learn.microsoft.com/dotnet/api/system.io.directory.enumeratefiles"/>.
    ///     </para>
    /// </param>
    /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or should include all subdirectories.</param>
    /// <returns>An enumerable collection of <see cref="AbsolutePath"/> of the full names (including paths) for the files in the specified directory that match the specified search pattern and option.</returns>
    public static IEnumerable<AbsolutePath> EnumerateFiles(this AbsolutePath path, string searchPattern, SearchOption searchOption)
    {
        return Directory.EnumerateFiles(path.Value, searchPattern, searchOption)
            .Select(filename => path / filename);
    }
}
