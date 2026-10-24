// SPDX-FileCopyrightText: 2024-2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Windows.Wdk.Storage.FileSystem;
using Windows.Win32;
using Windows.Win32.Storage.FileSystem;
using Microsoft.Win32.SafeHandles;

namespace TruePath;

/// <summary>
/// Utility class for handling disk operations and obtaining real paths.
/// </summary>
internal static class DiskUtils
{
    // from https://github.com/dotnet/corefx/blob/9c06da6a34fcefa6fb37776ac57b80730e37387c/src/Common/src/System/IO/PathInternal.Windows.cs#L52
    /// <summary>
    /// The maximum path length for Windows.
    /// </summary>
    private const short WindowsMaxPath = short.MaxValue;

    private const string WindowsOnlyJustification = "Only called on Windows, which is checked by the callers.";

    /// <summary>
    /// Determines whether the specified path is a junction (mount point).
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns><see langword="true"/> if the specified path is a junction; otherwise, <see langword="false"/>.</returns>
    public static bool IsJunction(string path)
    {
        return TryGetReparseTag(path) == PInvoke.IO_REPARSE_TAG_MOUNT_POINT;
    }

    /// <summary>
    /// Tries to get the reparse tag for the specified path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>
    /// The reparse tag if the path is a reparse point; otherwise, <see langword="null"/>.
    /// </returns>
    [SuppressMessage("Interoperability", "CA1416", Justification = WindowsOnlyJustification)]
    private static unsafe uint? TryGetReparseTag(string path)
    {
        using SafeFileHandle output = PInvoke.CreateFile(
            path,
            (uint)FILE_ACCESS_RIGHTS.FILE_READ_ATTRIBUTES,
            FILE_SHARE_MODE.FILE_SHARE_READ,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_FLAG_BACKUP_SEMANTICS | FILE_FLAGS_AND_ATTRIBUTES.FILE_FLAG_OPEN_REPARSE_POINT);

        if (output.IsInvalid)
        {
            return null;
        }

        const int bufferSize = (int)PInvoke.MAXIMUM_REPARSE_DATA_BUFFER_SIZE;
        var rentedBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        var buffer = rentedBuffer.AsSpan(0, bufferSize);
        bool success = PInvoke.DeviceIoControl(
            output,
            PInvoke.FSCTL_GET_REPARSE_POINT,
            lpInBuffer: default,
            lpOutBuffer: buffer,
            out _,
            lpOverlapped: null);

        if (!success)
        {
            return null;
        }

        success = MemoryMarshal.TryRead(buffer, out REPARSE_DATA_BUFFER reparseDataBuffer);
        ArrayPool<byte>.Shared.Return(rentedBuffer);

        return success ? reparseDataBuffer.ReparseTag : null;
    }

    /// <summary>
    /// Gets the real (absolute) path of the specified <paramref name="path"/> depending on the operating system.
    /// </summary>
    /// <param name="path">The file path to resolve.</param>
    /// <returns>The resolved absolute path.</returns>
    internal static string GetRealPath(string path)
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
        var ptr = Libc.RealPath(path, IntPtr.Zero);

        if (ptr == IntPtr.Zero)
        {
            return path;
        }

        try
        {
            return Marshal.PtrToStringAnsi(ptr)!; // never returns null since ptr is not zero here
        }
        finally
        {
            Libc.Free(ptr);
        }
    }

    /// <summary>
    /// Gets the real (absolute) path of the specified <paramref name="path"/> for Windows systems.
    /// </summary>
    /// <param name="path">The file path to resolve.</param>
    /// <returns>The resolved absolute path, or the original path if resolution fails.</returns>
    [SuppressMessage("Interoperability", "CA1416", Justification = WindowsOnlyJustification)]
    private static string GetWindowsRealPath(string path)
    {
        using var handle = PInvoke.CreateFile(path,
            (uint)FILE_ACCESS_RIGHTS.FILE_READ_EA,
            FILE_SHARE_MODE.FILE_SHARE_READ,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_FLAG_BACKUP_SEMANTICS);

        if (handle.IsInvalid)
        {
            return path;
        }

        return GetWindowsRealPathByHandle(handle) ?? path;
    }

    /// <summary>
    /// Gets the real (absolute) path by file handle for Windows systems.
    /// </summary>
    /// <param name="handle">The file handle.</param>
    /// <returns>The resolved absolute path, or <c>null</c> if resolution fails.</returns>
    [SuppressMessage("Interoperability", "CA1416", Justification = WindowsOnlyJustification)]
    private static string? GetWindowsRealPathByHandle(SafeFileHandle handle)
    {
        // this is called for each storage environment for the Data, Journals and Temp paths
        // WindowsMaxPath is 32K and although this is called only once we can have a lot of storage environments
        if (GetPath(256, out var realPath) == false)
        {
            if (GetPath((uint)WindowsMaxPath, out realPath) == false)
                return null;
        }

        if (string.IsNullOrWhiteSpace(realPath) || realPath is null)
        {
            return null;
        }

        //The string that is returned by this function uses the \?\ syntax
        if (realPath.Length >= 8 && realPath.AsSpan().StartsWith(@"\\?\UNC\".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            // network path, replace `\\?\UNC\` with `\\`
            realPath = string.Concat("\\", realPath[7..]);
        }

        if (realPath.Length >= 4 && realPath.AsSpan().StartsWith(@"\\?\".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            // local path, remove `\\?\`
            realPath = realPath.AsSpan(4).ToString();
        }

        return realPath;

        bool GetPath(uint bufferSize, out string? outputPath)
        {
            var charArray = ArrayPool<char>.Shared.Rent((int)bufferSize);

            var result = PInvoke.GetFinalPathNameByHandle(
                handle,
                charArray.AsSpan(0, (int)bufferSize),
                GETFINALPATHNAMEBYHANDLE_FLAGS.FILE_NAME_NORMALIZED);
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
