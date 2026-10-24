// SPDX-FileCopyrightText: 2024-2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace TruePath;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal static class Kernel32
{
    /// <summary>
    /// Retrieves the final path for the specified file handle.
    /// </summary>
    /// <param name="hFile">The file handle.</param>
    /// <param name="buffer">A buffer that receives the final path.</param>
    /// <param name="bufferLength">The size of the buffer, in characters.</param>
    /// <param name="dwFlags">The flags to specify the path format.</param>
    /// <returns>The length of the string copied to the buffer.</returns>
    [DllImport("kernel32.dll",
        EntryPoint = "GetFinalPathNameByHandleW",
        SetLastError = true,
        CharSet = CharSet.Unicode)]
    internal static extern unsafe uint GetFinalPathNameByHandle(
        IntPtr hFile,
        char* buffer,
        uint bufferLength,
        uint dwFlags = 0);

    [Flags]
    internal enum FileAttributes : uint
    {
        /// <summary>FILE_FLAG_BACKUP_SEMANTICS</summary>
        BackupSemantics = 0x02000000,

        /// <summary>FILE_FLAG_OPEN_REPARSE_POINT</summary>
        OpenReparsePoint = 0x00200000
    }

    [Flags]
    internal enum FileAccess : uint
    {
        /// <summary>FILE_READ_EA</summary>
        ReadEa = 0x0008,

        /// <summary>FILE_READ_ATTRIBUTES</summary>
        ReadAttributes = 0x0080
    }

    [Flags]
    internal enum FileShare : uint
    {
        /// <summary>FILE_SHARE_READ</summary>
        Read = 0x01
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
    [DllImport("kernel32.dll",
        EntryPoint = "CreateFileW",
        SetLastError = true,
        CharSet = CharSet.Unicode)]
    internal static extern SafeFileHandle CreateFile(
        [MarshalAs(UnmanagedType.LPTStr)] string filename,
        FileAccess access,
        FileShare share,
        nint securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
        FileMode creationDisposition,
        FileAttributes flagsAndAttributes,
        nint templateFile);

    /// <summary>
    /// Control code for retrieving reparse point data.
    /// </summary>
    /// <remarks>
    /// For more information, see <see href="https://learn.microsoft.com/windows/win32/api/winioctl/ni-winioctl-fsctl_get_reparse_point"/>.
    /// </remarks>
    internal const int FSCTL_GET_REPARSE_POINT = 0x000900a8;

    /// <summary>
    /// Sends a control code directly to a specified device driver, causing the corresponding device to perform the corresponding operation.
    /// </summary>
    /// <param name="hDevice">A handle to the device on which the operation is to be performed.</param>
    /// <param name="dwIoControlCode">The control code for the operation.</param>
    /// <param name="lpInBuffer">A pointer to the input buffer that contains the data required to perform the operation.</param>
    /// <param name="nInBufferSize">The size of the input buffer, in bytes.</param>
    /// <param name="lpOutBuffer">A pointer to the output buffer that receives the data returned by the operation.</param>
    /// <param name="nOutBufferSize">The size of the output buffer, in bytes.</param>
    /// <param name="lpBytesReturned">A variable that receives the size of the data stored in the output buffer, in bytes.</param>
    /// <param name="lpOverlapped">A pointer to an OVERLAPPED structure for asynchronous operations. For synchronous operations, this parameter is set to <see cref="IntPtr.Zero"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the operation succeeds; otherwise, <see langword="false"/>. If the operation fails, call <see cref="Marshal.GetLastWin32Error"/> to get extended error information.
    /// </returns>
    /// <remarks>
    /// For more information, see <see href="https://learn.microsoft.com/windows-hardware/drivers/ifs/fsctl-get-reparse-point"/>.
    /// </remarks>
    [DllImport("kernel32.dll", EntryPoint = "DeviceIoControl", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern unsafe bool DeviceIoControl(
        SafeHandle hDevice,
        uint dwIoControlCode,
        void* lpInBuffer,
        uint nInBufferSize,
        void* lpOutBuffer,
        uint nOutBufferSize,
        out uint lpBytesReturned,
        IntPtr lpOverlapped);

    /// <summary>
    /// The maximum size of the reparse data buffer.
    /// </summary>
    /// <remarks>
    /// For more information, see <see href="https://learn.microsoft.com/windows-hardware/drivers/ifs/fsctl-get-reparse-point"/>.
    /// </remarks>
    internal const int MAXIMUM_REPARSE_DATA_BUFFER_SIZE = 16 * 1024;

    /// <summary>
    /// Contains constants for reparse tags used in IO operations.
    /// </summary>
    internal static class IOReparseOptions
    {
        /// <summary>
        /// Reparse tag for a mount point.
        /// </summary>
        internal const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;
    }

    /// <summary>
    /// Represents a symbolic link reparse buffer.
    /// </summary>
    /// <remarks>
    /// For more information, see <see href="https://msdn.microsoft.com/library/windows/hardware/ff552012.aspx"/>.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal struct SymbolicLinkReparseBuffer
    {
        /// <summary>
        /// The reparse tag.
        /// </summary>
        internal uint ReparseTag;

        /// <summary>
        /// The length of the reparse data.
        /// </summary>
        internal ushort ReparseDataLength;

        /// <summary>
        /// Reserved; do not use.
        /// </summary>
        internal ushort Reserved;

        /// <summary>
        /// The offset, in bytes, to the substitute name string in the PathBuffer array.
        /// </summary>
        internal ushort SubstituteNameOffset;

        /// <summary>
        /// The length, in bytes, of the substitute name string. If this string is null-terminated, SubstituteNameLength does not include space for the null character.
        /// </summary>
        internal ushort SubstituteNameLength;

        /// <summary>
        /// The offset, in bytes, to the print name string in the PathBuffer array.
        /// </summary>
        internal ushort PrintNameOffset;

        /// <summary>
        /// The length, in bytes, of the print name string. If this string is null-terminated, PrintNameLength does not include space for the null character.
        /// </summary>
        internal ushort PrintNameLength;

        /// <summary>
        /// Flags that control the behavior of the symbolic link.
        /// </summary>
        internal uint Flags;
    }

}
