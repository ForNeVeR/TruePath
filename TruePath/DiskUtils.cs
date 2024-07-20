// SPDX-FileCopyrightText: .NET Foundation and Contributors <https://github.com/dotnet/corefx/blob/9c06da6a34fcefa6fb37776ac57b80730e37387c/src/Common/src/System/IO/PathInternal.Windows.cs>
// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
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
public static class DiskUtils
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
    /// Tries to get the reparse data buffer for the specified path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>
    /// A <see cref="SymbolicLinkReparseBuffer"/> containing the reparse data if successful; otherwise, <see langword="null"/>.
    /// </returns>
    internal static unsafe SymbolicLinkReparseBuffer? TryGetReparseDataBuffer(string path)
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

        bool success;
        fixed (byte* pBuffer = buffer)
        {
            success = DeviceIoControl(
                output,
                dwIoControlCode: FSCTL_GET_REPARSE_POINT,
                lpInBuffer: null,
                nInBufferSize: 0,
                lpOutBuffer: pBuffer,
                nOutBufferSize: MAXIMUM_REPARSE_DATA_BUFFER_SIZE,
                out _,
                IntPtr.Zero);

            if (!success)
            {
                return null;
            }
        }

        Span<byte> bufferSpan = new(buffer);
        success = MemoryMarshal.TryRead(bufferSpan, out SymbolicLinkReparseBuffer reparseDataBuffer);

        ArrayPool<byte>.Shared.Return(buffer);

        if (success)
        {
            return reparseDataBuffer;
        }

        return null;
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
        using var ptr = Libc.RealPath(path, IntPtr.Zero);

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
}
