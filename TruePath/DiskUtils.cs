// SPDX-FileCopyrightText: 2024-2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Buffers;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using static TruePath.Kernel32;

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

    /// <summary>
    /// Determines whether the specified path is a junction (mount point).
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns><see langword="true"/> if the specified path is a junction; otherwise, <see langword="false"/>.</returns>
    public static bool IsJunction(string path)
    {
        var reparseDataBuffer = TryGetReparseDataBuffer(path);

        if (reparseDataBuffer == null)
        {
            return false;
        }

        return reparseDataBuffer.Value.ReparseTag == IOReparseOptions.IO_REPARSE_TAG_MOUNT_POINT;
    }

    /// <summary>
    /// Tries to get the reparse data buffer header for the specified path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>
    /// A <see cref="ReparseDataBufferHeader"/> containing the common reparse data header if successful; otherwise,
    /// <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// Only the header common to all reparse tags is returned. The layout of the data following it depends on the tag,
    /// so it must not be interpreted without checking <see cref="ReparseDataBufferHeader.ReparseTag"/> first.
    /// </remarks>
    internal static unsafe ReparseDataBufferHeader? TryGetReparseDataBuffer(string path)
    {
        using SafeFileHandle output = CreateFile(
            path,
            Kernel32.FileAccess.ReadAttributes,
            Kernel32.FileShare.Read,
            IntPtr.Zero,
            FileMode.Open,
            Kernel32.FileAttributes.BackupSemantics | Kernel32.FileAttributes.OpenReparsePoint,
            IntPtr.Zero);

        if (output.IsInvalid)
        {
            return null;
        }

        byte[] buffer = ArrayPool<byte>.Shared.Rent(MAXIMUM_REPARSE_DATA_BUFFER_SIZE);
        try
        {
            // Pooled arrays are not zeroed, so clear the part of the buffer we may read. The driver may return no data
            // at all (e.g. when the path is not a reparse point), and the response may also be shorter than the header.
            buffer.AsSpan(0, sizeof(ReparseDataBufferHeader)).Clear();

            bool success;
            uint bytesReturned;
            fixed (byte* pBuffer = buffer)
            {
                success = DeviceIoControl(
                    output,
                    dwIoControlCode: FSCTL_GET_REPARSE_POINT,
                    lpInBuffer: null,
                    nInBufferSize: 0,
                    lpOutBuffer: pBuffer,
                    nOutBufferSize: MAXIMUM_REPARSE_DATA_BUFFER_SIZE,
                    out bytesReturned,
                    IntPtr.Zero);
            }

            if (!success)
            {
                return null;
            }

            // The rented array may be larger than the requested buffer, and the driver may return less data than the
            // buffer can hold: never inspect bytes the driver did not write.
            if (bytesReturned > (uint)buffer.Length)
            {
                return null;
            }

            return ParseReparseDataBuffer(new ReadOnlySpan<byte>(buffer, 0, (int)bytesReturned));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Parses the common header of a reparse data buffer.
    /// </summary>
    /// <param name="buffer">The bytes returned by the driver.</param>
    /// <returns>
    /// The parsed header, or <see langword="null"/> if <paramref name="buffer"/> is shorter than the header.
    /// </returns>
    internal static ReparseDataBufferHeader? ParseReparseDataBuffer(ReadOnlySpan<byte> buffer)
    {
        return MemoryMarshal.TryRead(buffer, out ReparseDataBufferHeader reparseDataBuffer)
            ? reparseDataBuffer
            : null;
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
    private static string GetWindowsRealPath(string path)
    {
        using var handle = CreateFile(path,
            Kernel32.FileAccess.ReadEa,
            Kernel32.FileShare.Read,
            IntPtr.Zero,
            FileMode.Open,
            Kernel32.FileAttributes.BackupSemantics,
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
}
