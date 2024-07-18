using System.Buffers;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.Win32.SafeHandles;

// ReSharper disable InconsistentNaming

namespace TruePath;

public static partial class Syscall
{
    [LibraryImport("libc",
        EntryPoint = "realpath",
        SetLastError = true,
        StringMarshalling = StringMarshalling.Custom,
        StringMarshallingCustomType = typeof(AnsiStringMarshaller))]
    internal static partial SafeFileHandle realpath(string path, IntPtr buffer);
}

public static partial class DiskUtils
{
    // from https://github.com/dotnet/corefx/blob/9c06da6a34fcefa6fb37776ac57b80730e37387c/src/Common/src/System/IO/PathInternal.Windows.cs#L52
    public const short WindowsMaxPath = short.MaxValue;
    public static string GetAutoRealPath(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetWindowsRealPathByPath(path);
        }

        return GetPosixRealPath(path);
    }

    public static string GetPosixRealPath(string path)
    {
        using var ptr = Syscall.realpath(path, IntPtr.Zero);

        if (ptr.DangerousGetHandle() == IntPtr.Zero)
        {
            return path;
        }

        var result = Marshal.PtrToStringAnsi(ptr.DangerousGetHandle());

        return result ?? path;
    }

    public static string GetWindowsRealPathByPath(string path)
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
