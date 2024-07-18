// SPDX-FileCopyrightText: .NET Foundation and Contributors <https://github.com/dotnet/corefx/blob/9c06da6a34fcefa6fb37776ac57b80730e37387c/src/Common/src/System/IO/PathInternal.Windows.cs>
// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Buffers;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.Win32.SafeHandles;

namespace TruePath;

/// <summary>
/// Provides interop methods for the libc library.
/// </summary>
internal static partial class Libc
{
    /// <summary>
    /// Resolves the absolute path of the specified <paramref name="path"/> and stores it in the provided <paramref name="buffer"/>.
    /// </summary>
    /// <param name="path">The file path to resolve.</param>
    /// <param name="buffer">A pointer to a buffer where the resolved absolute path will be stored.</param>
    /// <returns>A <see cref="SafeFileHandle"/> representing the resolved path, or an invalid handle if the function fails.</returns>
    /// <remarks>
    /// This method is an interop call to the 'realpath' function in the libc library. The <paramref name="buffer"/> should be allocated with enough space to hold the resolved path.
    /// </remarks>
    [LibraryImport("libc",
        EntryPoint = "realpath",
        SetLastError = true,
        StringMarshalling = StringMarshalling.Custom,
        StringMarshallingCustomType = typeof(AnsiStringMarshaller))]
    // ReSharper disable InconsistentNaming
    internal static partial SafeFileHandle realpath(string path, IntPtr buffer);
}

/// <summary>
/// Utility class for handling disk operations and obtaining real paths.
/// </summary>
public static partial class DiskUtils
{
    // from https://github.com/dotnet/corefx/blob/9c06da6a34fcefa6fb37776ac57b80730e37387c/src/Common/src/System/IO/PathInternal.Windows.cs#L52
    /// <summary>
    /// The maximum path length for Windows.
    /// </summary>
    private const short WindowsMaxPath = short.MaxValue;

    /// <summary>
    /// Gets the real (absolute) path of the specified <paramref name="path"/> depending on the operating system.
    /// </summary>
    /// <param name="path">The file path to resolve.</param>
    /// <returns>The resolved absolute path.</returns>
    public static string GetRealPath(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetWindowsRealPath(path);
        }

        return GetPosixRealPath(path);
    }

    /// <summary>
    /// Gets the real (absolute) path of the specified <paramref name="path"/> for POSIX systems.
    /// </summary>
    /// <param name="path">The file path to resolve.</param>
    /// <returns>The resolved absolute path, or the original path if resolution fails.</returns>
    private static string GetPosixRealPath(string path)
    {
        using var ptr = Libc.realpath(path, IntPtr.Zero);

        if (ptr.DangerousGetHandle() == IntPtr.Zero)
        {
            return path;
        }

        var result = Marshal.PtrToStringAnsi(ptr.DangerousGetHandle());

        return result ?? path;
    }

    /// <summary>
    /// Gets the real (absolute) path of the specified <paramref name="path"/> for Windows systems.
    /// </summary>
    /// <param name="path">The file path to resolve.</param>
    /// <returns>The resolved absolute path, or the original path if resolution fails.</returns>
    private static string GetWindowsRealPath(string path)
    {
        using var handle = CreateFile(path,
            FileAccess.ReadEa,
            FileShare.Read,
            IntPtr.Zero,
            FileMode.Open,
            FileAttributes.BackupSemantics,
            IntPtr.Zero);

        if (handle.IsInvalid)
        {
            return path;
        }

        return GetWindowsRealPathByHandle(handle.DangerousGetHandle()) ?? path;
    }

    /// <summary>
    /// Gets the real (absolute) path by file handle for Windows systems.
    /// </summary>
    /// <param name="handle">The file handle.</param>
    /// <returns>The resolved absolute path, or <c>null</c> if resolution fails.</returns>
    private static unsafe string? GetWindowsRealPathByHandle(IntPtr handle)
    {
        // this is called for each storage environment for the Data, Journals and Temp paths
        // WindowsMaxPath is 32K and although this is called only once we can have a lot of storage environments
        if (GetPath(256, out var realPath) == false)
        {
            if (GetPath((uint)WindowsMaxPath, out realPath) == false)
                return null;
        }

        if (string.IsNullOrWhiteSpace(realPath))
        {
            return null;
        }

        //The string that is returned by this function uses the \?\ syntax
        if (realPath.Length >= 8 && realPath.AsSpan().StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase))
        {
            // network path, replace `\\?\UNC\` with `\\`
            realPath = string.Concat("\\", realPath.AsSpan(7));
        }

        if (realPath.Length >= 4 && realPath.AsSpan().StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase))
        {
            // local path, remove `\\?\`
            realPath = realPath.AsSpan(4).ToString();
        }

        return realPath;

        bool GetPath(uint bufferSize, out string? outputPath)
        {
            var charArray = ArrayPool<char>.Shared.Rent((int)bufferSize);

            fixed (char* buffer = charArray)
            {
                var result = GetFinalPathNameByHandle(handle, buffer, bufferSize);
                if (result == 0)
                {
                    outputPath = null;
                    ArrayPool<char>.Shared.Return(charArray);
                    return false;
                }

                // return value is the size of the path
                if (result > bufferSize)
                {
                    outputPath = null;
                    ArrayPool<char>.Shared.Return(charArray);
                    return false;
                }

                outputPath = new string(charArray, 0, (int)result);
                ArrayPool<char>.Shared.Return(charArray);
                return true;
            }
        }
    }

    [Flags]
    internal enum FileAttributes : uint
    {
        /// <summary>FILE_ATTRIBUTE_ARCHIVE</summary>
        Archive = 0x20,

        /// <summary>FILE_ATTRIBUTE_COMPRESSED</summary>
        Compressed = 0x800,

        /// <summary>FILE_ATTRIBUTE_DEVICE</summary>
        Device = 0x40,

        /// <summary>FILE_ATTRIBUTE_DIRECTORY</summary>
        Directory = 0x10,

        /// <summary>FILE_ATTRIBUTE_ENCRYPTED</summary>
        Encrypted = 0x4000,

        /// <summary>FILE_ATTRIBUTE_HIDDEN</summary>
        Hidden = 0x02,

        /// <summary>FILE_ATTRIBUTE_INTEGRITY_STREAM</summary>
        IntegrityStream = 0x8000,

        /// <summary>FILE_ATTRIBUTE_NORMAL</summary>
        Normal = 0x80,

        /// <summary>FILE_ATTRIBUTE_NOT_CONTENT_INDEXED</summary>
        NotContentIndexed = 0x2000,

        /// <summary>FILE_ATTRIBUTE_NO_SCRUB_DATA</summary>
        NoScrubData = 0x20000,

        /// <summary>FILE_ATTRIBUTE_OFFLINE</summary>
        Offline = 0x1000,

        /// <summary>FILE_ATTRIBUTE_READONLY</summary>
        Readonly = 0x01,

        /// <summary>FILE_ATTRIBUTE_REPARSE_POINT</summary>
        ReparsePoint = 0x400,

        /// <summary>FILE_ATTRIBUTE_SPARSE_FILE</summary>
        SparseFile = 0x200,

        /// <summary>FILE_ATTRIBUTE_SYSTEM</summary>
        System = 0x04,

        /// <summary>FILE_ATTRIBUTE_TEMPORARY</summary>
        Temporary = 0x100,

        /// <summary>FILE_ATTRIBUTE_VIRTUAL</summary>
        Virtual = 0x10000,

        /// <summary>FILE_FLAG_BACKUP_SEMANTICS</summary>
        BackupSemantics = 0x02000000,

        /// <summary>FILE_FLAG_DELETE_ON_CLOSE</summary>
        DeleteOnClose = 0x04000000,

        /// <summary>FILE_FLAG_NO_BUFFERING</summary>
        NoBuffering = 0x20000000,

        /// <summary>FILE_FLAG_OPEN_NO_RECALL</summary>
        OpenNoRecall = 0x00100000,

        /// <summary>FILE_FLAG_OPEN_REPARSE_POINT</summary>
        OpenReparsePoint = 0x00200000,

        /// <summary>FILE_FLAG_OVERLAPPED</summary>
        Overlapped = 0x40000000,

        /// <summary>FILE_FLAG_POSIX_SEMANTICS</summary>
        PosixSemantics = 0x0100000,

        /// <summary>FILE_FLAG_RANDOM_ACCESS</summary>
        RandomAccess = 0x10000000,

        /// <summary>FILE_FLAG_SESSION_AWARE</summary>
        SessionAware = 0x00800000,

        /// <summary>FILE_FLAG_SEQUENTIAL_SCAN</summary>
        SequentialScan = 0x08000000,

        /// <summary>FILE_FLAG_WRITE_THROUGH</summary>
        WriteThrough = 0x80000000
    }

    [Flags]
    internal enum FileAccess : uint
    {
        /// <summary>FILE_READ_DATA</summary>
        ReadData = 0x0001,

        /// <summary>FILE_LIST_DIRECTORY</summary>
        ListDirectory = ReadData,

        /// <summary>FILE_WRITE_DATA</summary>
        WriteData = 0x0002,

        /// <summary>FILE_ADD_FILE</summary>
        AddFile = WriteData,

        /// <summary>FILE_APPEND_DATA</summary>
        AppendData = 0x0004,

        /// <summary>FILE_ADD_SUBDIRECTORY</summary>
        AddSubdirectory = AppendData,

        /// <summary>FILE_CREATE_PIPE_INSTANCE</summary>
        CreatePipeInstance = AppendData,

        /// <summary>FILE_READ_EA</summary>
        ReadEa = 0x0008,

        /// <summary>FILE_WRITE_EA</summary>
        WriteEa = 0x0010,

        /// <summary>FILE_EXECUTE</summary>
        Execute = 0x0020,

        /// <summary>FILE_TRAVERSE</summary>
        Traverse = Execute,

        /// <summary>FILE_DELETE_CHILD</summary>
        DeleteChild = 0x0040,

        /// <summary>FILE_READ_ATTRIBUTES</summary>
        ReadAttributes = 0x0080,

        /// <summary>FILE_WRITE_ATTRIBUTES</summary>
        WriteAttributes = 0x0100,

        /// <summary>GENERIC_READ</summary>
        GenericRead = 0x80000000,

        /// <summary>GENERIC_WRITE</summary>
        GenericWrite = 0x40000000,

        /// <summary>GENERIC_EXECUTE</summary>
        GenericExecute = 0x20000000,

        /// <summary>GENERIC_ALL</summary>
        GenericAll = 0x10000000
    }

    [Flags]
    internal enum FileShare : uint
    {
        /// <summary>FILE_SHARE_NONE</summary>
        None = 0x00,

        /// <summary>FILE_SHARE_READ</summary>
        Read = 0x01,

        /// <summary>FILE_SHARE_WRITE</summary>
        Write = 0x02,

        /// <summary>FILE_SHARE_DELETE</summary>
        Delete = 0x03
    }

    /// <summary>
    /// Creates or opens a file or I/O device.
    /// </summary>
    /// <param name="filename">The name of the file or device to be created or opened.</param>
    /// <param name="access">The requested access to the file or device.</param>
    /// <param name="share">The requested sharing mode of the file or device.</param>
    /// <param name="securityAttributes">A pointer to a <c>SECURITY_ATTRIBUTES</c> structure or <c>IntPtr.Zero</c>.</param>
    /// <param name="creationDisposition">An action to take on a file or device that exists or does not exist.</param>
    /// <param name="flagsAndAttributes">The file or device attributes and flags.</param>
    /// <param name="templateFile">A valid handle to a template file, or <c>IntPtr.Zero</c>.</param>
    /// <returns>A <see cref="SafeFileHandle"/> for the opened file or device.</returns>
    [LibraryImport("kernel32.dll",
        EntryPoint = "CreateFileW",
        SetLastError = true,
        StringMarshalling = StringMarshalling.Utf16)]
    private static partial SafeFileHandle CreateFile(
        [MarshalAs(UnmanagedType.LPTStr)] string filename,
        FileAccess access,
        FileShare share,
        nint securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
        FileMode creationDisposition,
        FileAttributes flagsAndAttributes,
        nint templateFile);

    /// <summary>
    /// Retrieves the final path for the specified file handle.
    /// </summary>
    /// <param name="hFile">The file handle.</param>
    /// <param name="buffer">A buffer that receives the final path.</param>
    /// <param name="bufferLength">The size of the buffer, in characters.</param>
    /// <param name="dwFlags">The flags to specify the path format.</param>
    /// <returns>The length of the string copied to the buffer.</returns>
    [LibraryImport("kernel32.dll",
        EntryPoint = "GetFinalPathNameByHandleW",
        SetLastError = true,
        StringMarshalling = StringMarshalling.Utf16)]
    private static unsafe partial uint GetFinalPathNameByHandle(
        IntPtr hFile,
        char* buffer,
        uint bufferLength,
        uint dwFlags = 0);
}
