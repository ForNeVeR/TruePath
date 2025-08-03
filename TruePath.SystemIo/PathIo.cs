// SPDX-FileCopyrightText: 2025 .NET Foundation and Contributors
// SPDX-FileCopyrightText: 2025 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Runtime.Versioning;
using System.Text;

namespace TruePath.SystemIo;

/// <summary>
/// Extension methods for the <see cref="AbsolutePath"/> type that provide common I/O operations.
/// </summary>
public static class PathIo
{
    /// <summary>
    /// Appends the specified lines to the file, creating the file if it does not already exist.
    /// </summary>
    /// <param name="path">The file to append to.</param>
    /// <param name="contents">The lines to write to the file.</param>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> is empty.</exception>
    /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
    /// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is read-only.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is hidden.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a directory.</exception>
    /// <exception cref="UnauthorizedAccessException">This operation is not supported on the current platform.</exception>
    /// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="NotSupportedException"><paramref name="path"/> is in an invalid format.</exception>
    /// <remarks>
    /// Given a sequence of strings and a file path, this method opens the specified file, appends the each string as a line to the end of the file using the specified encoding,
    /// and then closes the file. The file handle is guaranteed to be closed by this method, even if exceptions are raised. The method creates the file
    /// if it doesn't exist, but it doesn't create new directories. Therefore, the value of the path parameter must contain existing directories.
    /// </remarks>
    public static void AppendAllLines(this AbsolutePath path, IEnumerable<string> contents) => File.AppendAllLines(path.Value, contents);

    /// <summary>
    /// Appends the specified lines to the file, creating the file if it does not already exist.
    /// </summary>
    /// <param name="path">The file to append to.</param>
    /// <param name="contents">The lines to write to the file.</param>
    /// <param name="encoding">The encoding to apply to the each line.</param>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> is empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is <see langword="null"/>.</exception>
    /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
    /// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is read-only.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is hidden.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a directory.</exception>
    /// <exception cref="UnauthorizedAccessException">This operation is not supported on the current platform.</exception>
    /// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="NotSupportedException"><paramref name="path"/> is in an invalid format.</exception>
    /// <remarks>
    /// Given a sequence of strings and a file path, this method opens the specified file, appends the each string as a line to the end of the file using the specified encoding,
    /// and then closes the file. The file handle is guaranteed to be closed by this method, even if exceptions are raised. The method creates the file
    /// if it doesn't exist, but it doesn't create new directories. Therefore, the value of the path parameter must contain existing directories.
    /// </remarks>
    public static void AppendAllLines(this AbsolutePath path, IEnumerable<string> contents, Encoding encoding) => File.AppendAllLines(path.Value, contents, encoding);

#if NET8_0_OR_GREATER
    /// <summary>
    /// Asynchronously appends the specified lines to the file, creating the file if it does not already exist.
    /// </summary>
    /// <param name="path">The file to append to.</param>
    /// <param name="contents">The lines to write to the file.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous append operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> is empty.</exception>
    /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
    /// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is read-only.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is hidden.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a directory.</exception>
    /// <exception cref="UnauthorizedAccessException">This operation is not supported on the current platform.</exception>
    /// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="NotSupportedException"><paramref name="path"/> is in an invalid format.</exception>
    /// <remarks>
    /// Given a sequence of strings and a file path, this method opens the specified file, appends the each string as a line to the end of the file using the specified encoding,
    /// and then closes the file. The file handle is guaranteed to be closed by this method, even if exceptions are raised. The method creates the file
    /// if it doesn't exist, but it doesn't create new directories. Therefore, the value of the path parameter must contain existing directories.
    /// </remarks>
    public static Task AppendAllLinesAsync(this AbsolutePath path, IEnumerable<string> contents, CancellationToken cancellationToken = default) => File.AppendAllLinesAsync(path.Value, contents, cancellationToken);

    /// <summary>
    /// Asynchronously appends the specified lines to the file, creating the file if it does not already exist.
    /// </summary>
    /// <param name="path">The file to append to.</param>
    /// <param name="contents">The lines to write to the file.</param>
    /// <param name="encoding">The encoding to apply to the each line.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous append operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> is empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is <see langword="null"/>.</exception>
    /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
    /// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is read-only.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is hidden.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a directory.</exception>
    /// <exception cref="UnauthorizedAccessException">This operation is not supported on the current platform.</exception>
    /// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="NotSupportedException"><paramref name="path"/> is in an invalid format.</exception>
    /// <remarks>
    /// Given a sequence of strings and a file path, this method opens the specified file, appends the each string as a line to the end of the file using the specified encoding,
    /// and then closes the file. The file handle is guaranteed to be closed by this method, even if exceptions are raised. The method creates the file
    /// if it doesn't exist, but it doesn't create new directories. Therefore, the value of the path parameter must contain existing directories.
    /// </remarks>
    public static Task AppendAllLinesAsync(this AbsolutePath path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default) => File.AppendAllLinesAsync(path.Value, contents, encoding, cancellationToken);
#endif

    /// <summary>
    /// Appends the specified string to the file, creating the file if it does not already exist.
    /// </summary>
    /// <param name="path">The file to append to.</param>
    /// <param name="contents">The characters to write to the file.</param>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> is empty.</exception>
    /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
    /// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is read-only.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is hidden.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a directory.</exception>
    /// <exception cref="UnauthorizedAccessException">This operation is not supported on the current platform.</exception>
    /// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="NotSupportedException"><paramref name="path"/> is in an invalid format.</exception>
    /// <remarks>
    /// Given a string and a file path, this method opens the specified file, appends the string to the end of the file using the specified encoding,
    /// and then closes the file. The file handle is guaranteed to be closed by this method, even if exceptions are raised. The method creates the file
    /// if it doesn't exist, but it doesn't create new directories. Therefore, the value of the path parameter must contain existing directories.
    /// </remarks>
    public static void AppendAllText(this AbsolutePath path, string? contents) => File.AppendAllText(path.Value, contents);

    /// <summary>
    /// Appends the specified string to the file, creating the file if it does not already exist.
    /// </summary>
    /// <param name="path">The file to append to.</param>
    /// <param name="contents">The characters to write to the file.</param>
    /// <param name="encoding">The encoding to apply to the string.</param>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> is empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is <see langword="null"/>.</exception>
    /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
    /// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is read-only.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is hidden.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a directory.</exception>
    /// <exception cref="UnauthorizedAccessException">This operation is not supported on the current platform.</exception>
    /// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="NotSupportedException"><paramref name="path"/> is in an invalid format.</exception>
    /// <remarks>
    /// Given a string and a file path, this method opens the specified file, appends the string to the end of the file using the specified encoding,
    /// and then closes the file. The file handle is guaranteed to be closed by this method, even if exceptions are raised. The method creates the file
    /// if it doesn't exist, but it doesn't create new directories. Therefore, the value of the path parameter must contain existing directories.
    /// </remarks>
    public static void AppendAllText(this AbsolutePath path, string? contents, Encoding encoding) => File.AppendAllText(path.Value, contents, encoding);

#if NET8_0_OR_GREATER
    /// <summary>
    /// Asynchronously appends the specified string to the file, creating the file if it does not already exist.
    /// </summary>
    /// <param name="path">The file to append to.</param>
    /// <param name="contents">The characters to write to the file.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous append operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> is empty.</exception>
    /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
    /// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is read-only.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is hidden.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a directory.</exception>
    /// <exception cref="UnauthorizedAccessException">This operation is not supported on the current platform.</exception>
    /// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="NotSupportedException"><paramref name="path"/> is in an invalid format.</exception>
    /// <remarks>
    /// Given a string and a file path, this method opens the specified file, appends the string to the end of the file using the specified encoding,
    /// and then closes the file. The file handle is guaranteed to be closed by this method, even if exceptions are raised. The method creates the file
    /// if it doesn't exist, but it doesn't create new directories. Therefore, the value of the path parameter must contain existing directories.
    /// </remarks>
    public static Task AppendAllTextAsync(this AbsolutePath path, string? contents, CancellationToken cancellationToken = default) => File.AppendAllTextAsync(path.Value, contents, cancellationToken);

    /// <summary>
    /// Asynchronously appends the specified string to the file, creating the file if it does not already exist.
    /// </summary>
    /// <param name="path">The file to append to.</param>
    /// <param name="contents">The characters to write to the file.</param>
    /// <param name="encoding">The encoding to apply to the string.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous append operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> is empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is <see langword="null"/>.</exception>
    /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
    /// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is read-only.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is hidden.</exception>
    /// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a directory.</exception>
    /// <exception cref="UnauthorizedAccessException">This operation is not supported on the current platform.</exception>
    /// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="NotSupportedException"><paramref name="path"/> is in an invalid format.</exception>
    /// <remarks>
    /// Given a string and a file path, this method opens the specified file, appends the string to the end of the file using the specified encoding,
    /// and then closes the file. The file handle is guaranteed to be closed by this method, even if exceptions are raised. The method creates the file
    /// if it doesn't exist, but it doesn't create new directories. Therefore, the value of the path parameter must contain existing directories.
    /// </remarks>
    public static Task AppendAllTextAsync(this AbsolutePath path, string? contents, Encoding encoding, CancellationToken cancellationToken = default) => File.AppendAllTextAsync(path.Value, contents, encoding, cancellationToken);
#endif

    /// <summary>
    /// Creates a <see cref="StreamWriter" /> that appends UTF-8 encoded text to an existing file, or to a new file if the specified file does not exist.
    /// </summary>
    /// <param name="path">The path to the file to append to.</param>
    /// <returns>A stream writer that appends UTF-8 encoded text to the specified file or to a new file.</returns>
    public static StreamWriter AppendText(this AbsolutePath path) => File.AppendText(path.Value);

    /// <summary>
    /// Creates or opens a file for writing UTF-8 encoded text. If the file already exists, its contents are replaced.
    /// </summary>
    /// <param name="path">The file to be opened for writing.</param>
    /// <returns>A <see cref="StreamWriter" /> that writes to the specified file using UTF-8 encoding.</returns>
    public static StreamWriter CreateText(this AbsolutePath path) => File.CreateText(path.Value);

    /// <summary>
    /// Copies an existing file to a new file. Overwriting a file of the same name is not allowed.
    /// </summary>
    /// <param name="sourceFile">The file to copy.</param>
    /// <param name="destFile">The name of the destination file. This cannot be a directory or an existing file.</param>
    public static void Copy(this AbsolutePath sourceFile, AbsolutePath destFile) => File.Copy(sourceFile.Value, destFile.Value);

    /// <summary>
    /// Copies an existing file to a new file. Overwriting a file of the same name is allowed.
    /// </summary>
    /// <param name="sourceFile">The file to copy.</param>
    /// <param name="destFile">The name of the destination file. This cannot be a directory.</param>
    /// <param name="overwrite"><b>true</b> if the destination file should be replaced if it already exists; otherwise, <b>false</b>.</param>
    public static void Copy(this AbsolutePath sourceFile, AbsolutePath destFile, bool overwrite) => File.Copy(sourceFile.Value, destFile.Value, overwrite);

    /// <summary>
    /// Creates, or truncates and overwrites, a file in the specified path.
    /// </summary>
    /// <param name="path">The path and name of the file to create.</param>
    /// <returns>A <see cref="FileStream"/> that provides read/write access to the file specified in path.</returns>
    public static FileStream Create(this AbsolutePath path) => File.Create(path.Value);
    /// <summary>
    /// Creates, or truncates and overwrites, a file in the specified path, specifying a buffer size.
    /// </summary>
    /// <param name="path">The path and name of the file to create.</param>
    /// <param name="bufferSize">The number of bytes buffered for reads and writes to the file.</param>
    /// <returns>A <see cref="FileStream"/> that provides read/write access to the file specified in path.</returns>
    public static FileStream Create(this AbsolutePath path, int bufferSize) => File.Create(path.Value, bufferSize);
    /// <summary>
    /// Creates or overwrites a file in the specified path, specifying a buffer size and options that describe how to create or overwrite the file.
    /// </summary>
    /// <param name="path">The path and name of the file to create.</param>
    /// <param name="bufferSize">The number of bytes buffered for reads and writes to the file.</param>
    /// <param name="options">One of the <see cref="FileOptions"/> values that describes how to create or overwrite the file.</param>
    /// <returns>A <see cref="FileStream"/> that provides read/write access to the file specified in path.</returns>
    public static FileStream Create(this AbsolutePath path, int bufferSize, FileOptions options) => File.Create(path.Value, bufferSize, options);
    /// <summary>
    /// Creates all directories and subdirectories in the specified path unless they already exist.
    /// </summary>
    /// <param name="path">The directory to create.</param>
    public static void CreateDirectory(this AbsolutePath path) => Directory.CreateDirectory(path.Value);
    /// <summary>
    /// Deletes the specified file or directory.
    /// </summary>
    /// <param name="path">The name of the file to be deleted. Wildcard characters are not supported.</param>
    public static void Delete(this AbsolutePath path) => File.Delete(path.Value);
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
    /// Determines whether the specified file exists.
    /// </summary>
    /// <param name="path">The file to check.</param>
    /// <returns><b>true</b> if the caller has the required permissions and <paramref name="path"/> contains the name of an existing file; otherwise, <b>false</b>. This method also returns <b>false</b> if <paramref name="path"/> is null, an invalid path, or a zero-length string. If the caller does not have sufficient permissions to read the specified file, no exception is thrown and the method returns <b>false</b> regardless of the existence of <paramref name="path"/>.</returns>
    public static bool Exists(this AbsolutePath path) => File.Exists(path.Value);
    /// <summary>
    /// Determines whether the given path refers to an existing directory on disk.
    /// </summary>
    /// <param name="path">The path to test.</param>
    /// <returns><b>true</b> if path refers to an existing directory; <b>false</b> if the directory does not exist or an error occurs when trying to determine if the specified directory exists.</returns>
    public static bool ExistsDirectory(this AbsolutePath path) => Directory.Exists(path.Value);

    /// <summary>
    /// Gets the <see cref="FileAttributes"/> of the file on the path.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>The <see cref="FileAttributes"/> of the file on the path.</returns>
    public static FileAttributes GetAttributes(this AbsolutePath path) => File.GetAttributes(path.Value);

    /// <summary>
    /// Returns the creation date and time of the specified file or directory.
    /// </summary>
    /// <param name="path">The file or directory for which to obtain creation date and time information.</param>
    /// <returns>A <see cref="DateTime"/> structure set to the creation date and time for the specified file or directory. This value is expressed in local time.</returns>
    public static DateTime GetCreationTime(this AbsolutePath path) => File.GetCreationTime(path.Value);

    /// <summary>
    /// Returns the creation date and time, in Coordinated Universal Time (UTC), of the specified file or directory.
    /// </summary>
    /// <param name="path">The file or directory for which to obtain creation date and time information.</param>
    /// <returns>A <see cref="DateTime"/> structure set to the creation date and time for the specified file or directory. This value is expressed in UTC time.</returns>
    public static DateTime GetCreationTimeUtc(this AbsolutePath path) => File.GetCreationTimeUtc(path.Value);

    /// <summary>
    /// Returns the date and time the specified file or directory was last accessed.
    /// </summary>
    /// <param name="path">The file or directory for which to obtain access date and time information.</param>
    /// <returns>A <see cref="DateTime"/> structure set to the date and time that the specified file or directory was last accessed. This value is expressed in local time.</returns>
    public static DateTime GetLastAccessTime(this AbsolutePath path) => File.GetLastAccessTime(path.Value);

    /// <summary>
    /// Returns the date and time, in Coordinated Universal Time (UTC), that the specified file or directory was last accessed.
    /// </summary>
    /// <param name="path">The file or directory for which to obtain access date and time information.</param>
    /// <returns>A <see cref="DateTime"/> structure set to the date and time that the specified file or directory was last accessed. This value is expressed in UTC time.</returns>
    public static DateTime GetLastAccessTimeUtc(this AbsolutePath path) => File.GetLastAccessTimeUtc(path.Value);

    /// <summary>
    /// Returns the date and time the specified file or directory was last written to.
    /// </summary>
    /// <param name="path">The file or directory for which to obtain write date and time information.</param>
    /// <returns>A <see cref="DateTime"/> structure set to the date and time that the specified file or directory was last written to. This value is expressed in local time.</returns>
    public static DateTime GetLastWriteTime(this AbsolutePath path) => File.GetLastWriteTime(path.Value);

    /// <summary>
    /// Returns the date and time, in Coordinated Universal Time (UTC), that the specified file or directory was last written to.
    /// </summary>
    /// <param name="path">The file or directory for which to obtain write date and time information.</param>
    /// <returns>A <see cref="DateTime"/> structure set to the date and time that the specified file or directory was last written to. This value is expressed in UTC time.</returns>
    public static DateTime GetLastWriteTimeUtc(this AbsolutePath path) => File.GetLastWriteTimeUtc(path.Value);

#if NET8_0_OR_GREATER
    /// <summary>
    /// Gets the <see cref="UnixFileMode"/> of the file on the path.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>The <see cref="UnixFileMode"/> of the file on the path.</returns>
    [UnsupportedOSPlatform("windows")]
    public static UnixFileMode GetUnixFileMode(this AbsolutePath path) => File.GetUnixFileMode(path.Value);
#endif

    /// <summary>
    /// Moves a specified file to a new location, providing the option to specify a new file name.
    /// </summary>
    /// <param name="sourceFile">The name of the file to move. Can include a relative or absolute path.</param>
    /// <param name="destFile">The new path and name for the file.</param>
    public static void Move(this AbsolutePath sourceFile, AbsolutePath destFile) => File.Move(sourceFile.Value, destFile.Value);

#if NET8_0_OR_GREATER
    /// <summary>
    /// Moves a specified file to a new location, providing the options to specify a new file name and to replace the destination file if it already exists.
    /// </summary>
    /// <param name="sourceFile">The name of the file to move. Can include a relative or absolute path.</param>
    /// <param name="destFile">The new path and name for the file.</param>
    /// <param name="overwrite"><b>true</b> to replace the destination file if it already exists; <b>false</b> otherwise.</param>
    public static void Move(this AbsolutePath sourceFile, AbsolutePath destFile, bool overwrite) => File.Move(sourceFile.Value, destFile.Value, overwrite);
#endif

    /// <summary>
    /// Opens a <see cref="FileStream"/> on the specified path with read/write access with no sharing.
    /// </summary>
    /// <param name="path">The file to open.</param>
    /// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
    /// <returns>A <see cref="FileStream"/> opened in the specified mode and path, with read/write access and not shared.</returns>
    public static FileStream Open(this AbsolutePath path, FileMode mode) => File.Open(path.Value, mode);

    /// <summary>
    /// Opens a <see cref="FileStream"/> on the specified path with the specified mode and access with no sharing.
    /// </summary>
    /// <param name="path">The file to open.</param>
    /// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
    /// <param name="access">A <see cref="FileAccess"/> value that specifies the operations that can be performed on the file.</param>
    /// <returns>A <see cref="FileStream"/> opened in the specified mode and path, with the specified mode and access with no sharing.</returns>
    public static FileStream Open(this AbsolutePath path, FileMode mode, FileAccess access) => File.Open(path.Value, mode, access);
    /// <summary>
    /// Opens a <see cref="FileStream"/> on the specified path, having the specified mode with read, write, or read/write access and the specified sharing option.
    /// </summary>
    /// <param name="path">The file to open.</param>
    /// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
    /// <param name="access">A <see cref="FileAccess"/> value that specifies the operations that can be performed on the file.</param>
    /// <param name="share">A <see cref="FileShare"/> value specifying the type of access other threads have to the file.</param>
    /// <returns>A <see cref="FileStream"/> on the specified path, having the specified mode with read, write, or read/write access and the specified sharing option.</returns>
    public static FileStream Open(this AbsolutePath path, FileMode mode, FileAccess access, FileShare share) => File.Open(path.Value, mode, access, share);

    /// <summary>
    /// Opens an existing file for reading.
    /// </summary>
    /// <param name="path">The file to be opened for reading.</param>
    /// <returns>A read-only <see cref="FileStream"/> on the specified path.</returns>
    public static FileStream OpenRead(this AbsolutePath path) => File.OpenRead(path.Value);

    /// <summary>
    /// Opens an existing UTF-8 encoded text file for reading.
    /// </summary>
    /// <param name="path">The file to be opened for reading.</param>
    /// <returns>A <see cref="StreamReader"/> on the specified path.</returns>
    public static StreamReader OpenText(this AbsolutePath path) => File.OpenText(path.Value);

    /// <summary>
    /// Opens an existing file or creates a new file for writing.
    /// </summary>
    /// <param name="path">The file to be opened for writing.</param>
    /// <returns>An unshared <see cref="FileStream"/> object on the specified path with <see cref="FileAccess.Write"/> access.</returns>
    public static FileStream OpenWrite(this AbsolutePath path) => File.OpenWrite(path.Value);

    /// <summary>
    /// Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
    /// </summary>
    /// <param name="path">The file to open for reading.</param>
    /// <returns>A byte array containing the contents of the file.</returns>
    public static byte[] ReadAllBytes(this AbsolutePath path) => File.ReadAllBytes(path.Value);

#if NET8_0_OR_GREATER
    /// <summary>
    /// Asynchronously opens a binary file, reads the contents of the file into a byte array, and then closes the file.
    /// </summary>
    /// <param name="path">The file to open for reading.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous read operation, which wraps the byte array containing the contents of the file.</returns>
    public static Task<byte[]> ReadAllBytesAsync(this AbsolutePath path, CancellationToken cancellationToken = default) => File.ReadAllBytesAsync(path.Value, cancellationToken);
#endif

    /// <summary>
    /// Opens a text file, reads all lines of the file, and then closes the file.
    /// </summary>
    /// <param name="path">The file to open for reading.</param>
    /// <returns>A string array containing all lines of the file.</returns>
    public static string[] ReadAllLines(this AbsolutePath path) => File.ReadAllLines(path.Value);

    /// <summary>
    /// Opens a file, reads all lines of the file with the specified encoding, and then closes the file.
    /// </summary>
    /// <param name="path">The file to open for reading.</param>
    /// <param name="encoding">The encoding applied to the contents of the file.</param>
    /// <returns>A string array containing all lines of the file.</returns>
    public static string[] ReadAllLines(this AbsolutePath path, Encoding encoding) => File.ReadAllLines(path.Value, encoding);

#if NET8_0_OR_GREATER
    /// <summary>
    /// Asynchronously opens a text file, reads all lines of the file, and then closes the file.
    /// </summary>
    /// <param name="path">The file to open for reading.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous read operation, which wraps the string array containing all lines of the file.</returns>
    public static Task<string[]> ReadAllLinesAsync(this AbsolutePath path, CancellationToken cancellationToken = default) => File.ReadAllLinesAsync(path.Value, cancellationToken);

    /// <summary>
    /// Asynchronously opens a text file, reads all lines of the file with the specified encoding, and then closes the file.
    /// </summary>
    /// <param name="path">The file to open for reading.</param>
    /// <param name="encoding">The encoding applied to the contents of the file.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous read operation, which wraps the string array containing all lines of the file.</returns>
    public static Task<string[]> ReadAllLinesAsync(this AbsolutePath path, Encoding encoding, CancellationToken cancellationToken = default) => File.ReadAllLinesAsync(path.Value, encoding, cancellationToken);
#endif

    /// <summary>
    /// Reads the lines of a file.
    /// </summary>
    /// <param name="path">The file to read.</param>
    /// <returns>All the lines of the file, or the lines that are the result of a query.</returns>
    public static IEnumerable<string> ReadLines(this AbsolutePath path) => File.ReadLines(path.Value);

    /// <summary>
    /// Read the lines of a file that has a specified encoding.
    /// </summary>
    /// <param name="path">The file to read.</param>
    /// <param name="encoding">The encoding that is applied to the contents of the file.</param>
    /// <returns>All the lines of the file, or the lines that are the result of a query.</returns>
    public static IEnumerable<string> ReadLines(this AbsolutePath path, Encoding encoding) => File.ReadLines(path.Value, encoding);

#if NET8_0_OR_GREATER
    /// <summary>
    /// Asynchronously reads the lines of a file.
    /// </summary>
    /// <param name="path">The file to read.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>The async enumerable that represents all the lines of the file, or the lines that are the result of a query.</returns>
    public static IAsyncEnumerable<string> ReadLinesAsync(this AbsolutePath path, CancellationToken cancellationToken = default) => File.ReadLinesAsync(path.Value, cancellationToken);

    /// <summary>
    /// Asynchronously reads the lines of a file that has a specified encoding.
    /// </summary>
    /// <param name="path">The file to read.</param>
    /// <param name="encoding">The encoding that is applied to the contents of the file.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>The async enumerable that represents all the lines of the file, or the lines that are the result of a query.</returns>
    public static IAsyncEnumerable<string> ReadLinesAsync(this AbsolutePath path, Encoding encoding, CancellationToken cancellationToken = default) => File.ReadLinesAsync(path.Value, encoding, cancellationToken);
#endif

    /// <summary>
    /// Opens a text file, reads all the text in the file, and then closes the file.
    /// </summary>
    /// <param name="path">The file to open for reading.</param>
    /// <returns>A string containing all the text in the file.</returns>
    public static string ReadAllText(this AbsolutePath path) => File.ReadAllText(path.Value);

    /// <summary>
    /// Opens a text file, reads all the text in the file, and then closes the file.
    /// </summary>
    /// <param name="path">The file to open for reading.</param>
    /// <param name="encoding">The encoding that is applied to the contents of the file.</param>
    /// <returns>A string containing all the text in the file.</returns>
    public static string ReadAllText(this AbsolutePath path, Encoding encoding) => File.ReadAllText(path.Value, encoding);

#if NET8_0_OR_GREATER
    /// <summary>
    /// Asynchronously opens a text file, reads all the text in the file, and then closes the file.
    /// </summary>
    /// <param name="path">The file to open for reading.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous read operation, which wraps the string containing all text in the file.</returns>
    public static Task<string> ReadAllTextAsync(this AbsolutePath path, CancellationToken cancellationToken = default) => File.ReadAllTextAsync(path.Value, cancellationToken);

    /// <summary>
    /// Asynchronously opens a text file, reads all text in the file with the specified encoding, and then closes the file.
    /// </summary>
    /// <param name="path">The file to open for reading.</param>
    /// <param name="encoding">The encoding that is applied to the contents of the file.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous read operation, which wraps the string containing all text in the file.</returns>
    public static Task<string> ReadAllTextAsync(this AbsolutePath path, Encoding encoding, CancellationToken cancellationToken = default) => File.ReadAllTextAsync(path.Value, encoding, cancellationToken);
#endif

    /// <summary>
    /// Sets the specified <see cref="FileAttributes"/> of the file on the specified path.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <param name="fileAttributes">A bitwise combination of the enumeration values.</param>
    public static void SetAttributes(this AbsolutePath path, FileAttributes fileAttributes) => File.SetAttributes(path.Value, fileAttributes);

    /// <summary>
    /// Sets the date and time the file was created.
    /// </summary>
    /// <param name="path">The file for which to set the creation date and time information.</param>
    /// <param name="creationTime">A <see cref="DateTime"/> containing the value to set for the creation date and time of <paramref name="path"/>. This value is expressed in local time.</param>
    public static void SetCreationTime(this AbsolutePath path, DateTime creationTime) => File.SetCreationTime(path.Value, creationTime);

    /// <summary>
    /// Sets the date and time, in Coordinated Universal Time (UTC), that the file was created.
    /// </summary>
    /// <param name="path">The file for which to set the creation date and time information.</param>
    /// <param name="creationTimeUtc">The value to set for the creation date and time of <paramref name="path"/>. This value is expressed in UTC time.</param>
    public static void SetCreationTimeUtc(this AbsolutePath path, DateTime creationTimeUtc) => File.SetCreationTimeUtc(path.Value, creationTimeUtc);

    /// <summary>
    /// Sets the date and time the specified file was last accessed.
    /// </summary>
    /// <param name="path">The file for which to set the access date and time information.</param>
    /// <param name="lastAccessTime">A <see cref="DateTime"/> containing the value to set for the last access date and time of <paramref name="path"/>. This value is expressed in local time.</param>
    public static void SetLastAccessTime(this AbsolutePath path, DateTime lastAccessTime) => File.SetLastAccessTime(path.Value, lastAccessTime);

    /// <summary>
    /// Sets the date and time, in Coordinated Universal Time (UTC), that the specified file was last accessed.
    /// </summary>
    /// <param name="path">The file for which to set the access date and time information.</param>
    /// <param name="lastAccessTimeUtc">A <see cref="DateTime"/> containing the value to set for the last access date and time of <paramref name="path"/>. This value is expressed in UTC time.</param>
    public static void SetLastAccessTimeUtc(this AbsolutePath path, DateTime lastAccessTimeUtc) => File.SetLastAccessTimeUtc(path.Value, lastAccessTimeUtc);

    /// <summary>
    /// Sets the date and time that the specified file was last written to.
    /// </summary>
    /// <param name="path">The file for which to set the date and time information.</param>
    /// <param name="lastWriteTime">A <see cref="DateTime"/> containing the value to set for the last write date and time of <paramref name="path"/>. This value is expressed in local time.</param>
    public static void SetLastWriteTime(this AbsolutePath path, DateTime lastWriteTime) => File.SetLastWriteTime(path.Value, lastWriteTime);

    /// <summary>
    /// Sets the date and time, in Coordinated Universal Time (UTC), that the specified file was last written to.
    /// </summary>
    /// <param name="path">The file for which to set the date and time information.</param>
    /// <param name="lastWriteTimeUtc">A <see cref="DateTime"/> containing the value to set for the last write date and time of <paramref name="path"/>. This value is expressed in UTC time.</param>
    public static void SetLastWriteTimeUtc(this AbsolutePath path, DateTime lastWriteTimeUtc) => File.SetLastWriteTimeUtc(path.Value, lastWriteTimeUtc);

    /// <summary>
    /// Creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is truncated and overwritten.
    /// </summary>
    /// <param name="path">The file to write to.</param>
    /// <param name="bytes">The bytes to write to the file.</param>
    public static void WriteAllBytes(this AbsolutePath path, byte[] bytes) => File.WriteAllBytes(path.Value, bytes);

#if NET8_0_OR_GREATER
    /// <summary>
    /// Asynchronously creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is truncated and overwritten.
    /// </summary>
    /// <param name="path">The file to write to.</param>
    /// <param name="bytes">The bytes to write to the file.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public static Task WriteAllBytesAsync(this AbsolutePath path, byte[] bytes, CancellationToken cancellationToken = default) => File.WriteAllBytesAsync(path.Value, bytes, cancellationToken);
#endif

    /// <summary>
    /// Creates a new file, writes a collection of strings to the file, and then closes the file.
    /// </summary>
    /// <param name="path">The file to write to.</param>
    /// <param name="contents">The lines to write to the file.</param>
    public static void WriteAllLines(this AbsolutePath path, IEnumerable<string> contents) => File.WriteAllLines(path.Value, contents);

    /// <summary>
    /// Creates a new file by using the specified encoding, writes a collection of strings to the file, and then closes the file.
    /// </summary>
    /// <param name="path">The file to write to.</param>
    /// <param name="contents">The lines to write to the file.</param>
    /// <param name="encoding">The character encoding to use.</param>
    public static void WriteAllLines(this AbsolutePath path, IEnumerable<string> contents, Encoding encoding) => File.WriteAllLines(path.Value, contents, encoding);

    /// <summary>
    /// Creates a new file, write the specified string array to the file, and then closes the file.
    /// </summary>
    /// <param name="path">The file to write to.</param>
    /// <param name="contents">The string array to write to the file.</param>
    public static void WriteAllLines(this AbsolutePath path, string[] contents) => File.WriteAllLines(path.Value, contents);

    /// <summary>
    /// Creates a new file, writes the specified string array to the file by using the specified encoding, and then closes the file.
    /// </summary>
    /// <param name="path">The file to write to.</param>
    /// <param name="contents">The string array to write to the file.</param>
    /// <param name="encoding">An <see cref="Encoding"/> object that represents the character encoding applied to the string array.</param>
    public static void WriteAllLines(this AbsolutePath path, string[] contents, Encoding encoding) => File.WriteAllLines(path.Value, contents, encoding);

#if NET8_0_OR_GREATER
    /// <summary>
    /// Asynchronously creates a new file, writes the specified lines to the file, and then closes the file.
    /// </summary>
    /// <param name="path">The file to write to.</param>
    /// <param name="contents">The lines to write to the file.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public static Task WriteAllLinesAsync(this AbsolutePath path, IEnumerable<string> contents, CancellationToken cancellationToken = default) => File.WriteAllLinesAsync(path.Value, contents, cancellationToken);

    /// <summary>
    /// Asynchronously creates a new file, write the specified lines to the file by using the specified encoding, and then closes the file.
    /// </summary>
    /// <param name="path">The file to write to.</param>
    /// <param name="contents">The lines to write to the file.</param>
    /// <param name="encoding">The character encoding to use.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public static Task WriteAllLinesAsync(this AbsolutePath path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default) => File.WriteAllLinesAsync(path.Value, contents, encoding, cancellationToken);
#endif

    /// <summary>
    /// Creates a new file, writes the specified string to the file, and then closes the file. If the target file already exists, it is truncated and overwritten.
    /// </summary>
    /// <param name="path">The file to write to.</param>
    /// <param name="contents">The string to write to the file.</param>
    public static void WriteAllText(this AbsolutePath path, string? contents) => File.WriteAllText(path.Value, contents);

    /// <summary>
    /// Creates a new file, writes the specified string to the file using the specified encoding, and then closes the file. If the target file already exists, it is truncated and overwritten.
    /// </summary>
    /// <param name="path">The file to write to.</param>
    /// <param name="contents">The string to write to the file.</param>
    /// <param name="encoding">The character encoding to use.</param>
    public static void WriteAllText(this AbsolutePath path, string? contents, Encoding encoding) => File.WriteAllText(path.Value, contents, encoding);

#if NET8_0_OR_GREATER
    /// <summary>
    /// Asynchronously creates a new file, writes the specified string to the file, and then closes the file. If the target file already exists, it is truncated and overwritten.
    /// </summary>
    /// <param name="path">The file to write to.</param>
    /// <param name="contents">The string to write to the file.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public static Task WriteAllTextAsync(this AbsolutePath path, string? contents, CancellationToken cancellationToken = default) => File.WriteAllTextAsync(path.Value, contents, cancellationToken);

    /// <summary>
    /// Asynchronously creates a new file, writes the specified string to the file using the specified encoding, and then closes the file. If the target file already exists, it is truncated and overwritten.
    /// </summary>
    /// <param name="path">The file to write to.</param>
    /// <param name="contents">The string to write to the file.</param>
    /// <param name="encoding">The character encoding to use.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public static Task WriteAllTextAsync(this AbsolutePath path, string? contents, Encoding encoding, CancellationToken cancellationToken = default) => File.WriteAllTextAsync(path.Value, contents, encoding, cancellationToken);
#endif

    /// <summary>
    /// Returns the names of files (including their paths) that match the specified search pattern in the specified directory.
    /// </summary>
    /// <param name="path">The directory to search.</param>
    /// <returns>An array of the full names (including paths) for the files in the specified directory that match the specified search pattern.</returns>
    public static string[] GetFiles(this AbsolutePath path) => Directory.GetFiles(path.Value);

    /// <summary>
    /// Returns the names of files (including their paths) that match the specified search pattern in the specified directory.
    /// </summary>
    /// <param name="path">The directory to search.</param>
    /// <param name="searchPattern">The search string to match against the names of files in <paramref name="path"/>.</param>
    /// <returns>An array of the full names (including paths) for the files in the specified directory that match the specified search pattern.</returns>
    public static string[] GetFiles(this AbsolutePath path, string searchPattern) => Directory.GetFiles(path.Value, searchPattern);

#if NET8_0_OR_GREATER
    /// <summary>
    /// Returns the names of files (including their paths) that match the specified search pattern and enumeration options in the specified directory.
    /// </summary>
    /// <param name="path">The directory to search.</param>
    /// <param name="searchPattern">The search string to match against the names of files in <paramref name="path"/>.</param>
    /// <param name="enumerationOptions">An object that contains the search options to use.</param>
    /// <returns>An array of the full names (including paths) for the files in the specified directory that match the specified search pattern and enumeration options.</returns>
    public static string[] GetFiles(this AbsolutePath path, string searchPattern, EnumerationOptions enumerationOptions) => Directory.GetFiles(path.Value, searchPattern, enumerationOptions);
#endif

    /// <summary>
    /// Returns the names of files (including their paths) that match the specified search pattern in the specified directory, using a value to determine whether to search subdirectories.
    /// </summary>
    /// <param name="path">The directory to search.</param>
    /// <param name="searchPattern">The search string to match against the names of files in <paramref name="path"/>.</param>
    /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or should include all subdirectories.</param>
    /// <returns>An array of the full names (including paths) for the files in the specified directory that match the specified search pattern and option.</returns>
    public static string[] GetFiles(this AbsolutePath path, string searchPattern, SearchOption searchOption) => Directory.GetFiles(path.Value, searchPattern, searchOption);

    /// <summary>
    /// Returns the names of subdirectories (including their paths) in the specified directory.
    /// </summary>
    /// <param name="path">The directory to search.</param>
    /// <returns>An array of the full names (including paths) for the subdirectories in the specified directory.</returns>
    public static string[] GetDirectories(this AbsolutePath path) => Directory.GetDirectories(path.Value);

    /// <summary>
    /// Returns the names of subdirectories (including their paths) that match the specified search pattern in the specified directory.
    /// </summary>
    /// <param name="path">The directory to search.</param>
    /// <param name="searchPattern">The search string to match against the names of subdirectories in <paramref name="path"/>.</param>
    /// <returns>An array of the full names (including paths) for the subdirectories in the specified directory that match the specified search pattern.</returns>
    public static string[] GetDirectories(this AbsolutePath path, string searchPattern) => Directory.GetDirectories(path.Value, searchPattern);

#if NET8_0_OR_GREATER
    /// <summary>
    /// Returns the names of subdirectories (including their paths) that match the specified search pattern and enumeration options in the specified directory.
    /// </summary>
    /// <param name="path">The directory to search.</param>
    /// <param name="searchPattern">The search string to match against the names of subdirectories in <paramref name="path"/>.</param>
    /// <param name="enumerationOptions">An object that contains the search options to use.</param>
    /// <returns>An array of the full names (including paths) for the subdirectories in the specified directory that match the specified search pattern and enumeration options.</returns>
    public static string[] GetDirectories(this AbsolutePath path, string searchPattern, EnumerationOptions enumerationOptions) => Directory.GetDirectories(path.Value, searchPattern, enumerationOptions);
#endif

    /// <summary>
    /// Returns the names of subdirectories (including their paths) that match the specified search pattern in the specified directory, using a value to determine whether to search subdirectories.
    /// </summary>
    /// <param name="path">The directory to search.</param>
    /// <param name="searchPattern">The search string to match against the names of subdirectories in <paramref name="path"/>.</param>
    /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or should include all subdirectories.</param>
    /// <returns>An array of the full names (including paths) for the subdirectories in the specified directory that match the specified search pattern and option.</returns>
    public static string[] GetDirectories(this AbsolutePath path, string searchPattern, SearchOption searchOption) => Directory.GetDirectories(path.Value, searchPattern, searchOption);
}
