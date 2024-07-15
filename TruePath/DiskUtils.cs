using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable InconsistentNaming

namespace TruePath;

public static unsafe partial class Syscall
{
    private const string LIBC_6 = "libc";

    [LibraryImport(LIBC_6, SetLastError = true)]
    public static partial int readlink(
        [MarshalAs(UnmanagedType.LPStr)] string path,
        byte* buffer, int bufferSize);
}

public static class DiskUtils
{
    // from https://github.com/dotnet/corefx/blob/9c06da6a34fcefa6fb37776ac57b80730e37387c/src/Common/src/System/IO/PathInternal.Windows.cs#L52
    public const short WindowsMaxPath = short.MaxValue;

    public const int LinuxMaxPath = 4096;

    public static string GetAutoRealPath(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetWindowsRealPathByPath(path);
        }

        return GetPosixRealPath(path);
    }

    public static unsafe string GetPosixRealPath(string path)
    {
        var byteArray = ArrayPool<byte>.Shared.Rent(LinuxMaxPath);

        fixed (byte* buffer = byteArray)
        {
            var result = Syscall.readlink(path, buffer, LinuxMaxPath);
            if (result == -1)
            {
                ArrayPool<byte>.Shared.Return(byteArray);

                // not a symbolic link
                return path;
            }

            var realPath = Encoding.UTF8.GetString(byteArray, 0, result);

            ArrayPool<byte>.Shared.Return(byteArray);

            return realPath;
        }
    }

    public static string GetWindowsRealPathByPath(string path)
    {
        var handle = CreateFile(path,
            FILE_READ_EA,
            FileShare.Read,
            IntPtr.Zero,
            FileMode.Open,
            FILE_FLAG_BACKUP_SEMANTICS,
            IntPtr.Zero);

        if (handle == INVALID_HANDLE_VALUE)
        {
            return path;
        }

        try
        {
            return GetWindowsRealPathByHandle(handle) ?? path;
        }
        finally
        {
            CloseHandle(handle);
        }
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
                var result = GetFinalPathNameByHandle(handle, buffer, bufferSize, 0);
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

    private const uint FILE_READ_EA = 0x0008;
    private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x2000000;
    private static readonly IntPtr INVALID_HANDLE_VALUE = new(-1);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateFile(
        [MarshalAs(UnmanagedType.LPTStr)] string filename,
        [MarshalAs(UnmanagedType.U4)] uint access,
        [MarshalAs(UnmanagedType.U4)] FileShare share,
        IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
        [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
        [MarshalAs(UnmanagedType.U4)] uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern unsafe uint GetFinalPathNameByHandle(
        IntPtr hFile,
        char* buffer,
        uint bufferLength,
        uint dwFlags);
}
