using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace TruePath;

internal static partial class Kernel32
{
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
    internal static unsafe partial uint GetFinalPathNameByHandle(
        IntPtr hFile,
        char* buffer,
        uint bufferLength,
        uint dwFlags = 0);

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
    internal static partial SafeFileHandle CreateFile(
        [MarshalAs(UnmanagedType.LPTStr)] string filename,
        FileAccess access,
        FileShare share,
        nint securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
        FileMode creationDisposition,
        FileAttributes flagsAndAttributes,
        nint templateFile);

    /// <summary>
    // /// Control code for retrieving reparse point data.
    // /// </summary>
    // /// <remarks>
    // /// For more information, see <see href="https://learn.microsoft.com/windows/win32/api/winioctl/ni-winioctl-fsctl_get_reparse_point"/>.
    // /// </remarks>
    internal const int FSCTL_GET_REPARSE_POINT = 0x000900a8;

    /// <summary>
    /// Control code for reading the storage capacity of a device.
    /// </summary>
    /// <remarks>
    /// For more information, see <see href="https://learn.microsoft.com/windows-hardware/drivers/ddi/ntddstor/ni-ntddstor-ioctl_storage_read_capacity"/>.
    /// </remarks>
    internal const int IOCTL_STORAGE_READ_CAPACITY = 0x002D5140;

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
    [LibraryImport("kernel32.dll", EntryPoint = "DeviceIoControl", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static unsafe partial bool DeviceIoControl(
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
        /// Reparse tag for a file placeholder.
        /// </summary>
        internal const uint IO_REPARSE_TAG_FILE_PLACEHOLDER = 0x80000015;

        /// <summary>
        /// Reparse tag for a mount point.
        /// </summary>
        internal const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;

        /// <summary>
        /// Reparse tag for a symbolic link.
        /// </summary>
        internal const uint IO_REPARSE_TAG_SYMLINK = 0xA000000C;
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
