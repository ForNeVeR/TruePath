// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath.Tests;

/// <summary>
/// Pins the Win32 constants kept in <see cref="Kernel32"/> to the values from the Windows headers
/// (see <see href="https://learn.microsoft.com/windows/win32/fileio/createfile"/> and
/// <see href="https://learn.microsoft.com/windows/win32/fileio/file-attribute-constants"/>).
/// These values used to be hand-copied and were wrong in some cases (see issue #241).
/// </summary>
public class Kernel32ConstantsTests
{
    [Fact]
    public void FileShareReadMatchesWindowsHeader()
    {
        // winnt.h: FILE_SHARE_READ 0x00000001
        Assert.Equal(0x00000001u, (uint)Kernel32.FileShare.Read);
    }

    [Fact]
    public void FileAccessValuesMatchWindowsHeader()
    {
        // winnt.h: FILE_READ_EA 0x0008, FILE_READ_ATTRIBUTES 0x0080
        Assert.Equal(0x0008u, (uint)Kernel32.FileAccess.ReadEa);
        Assert.Equal(0x0080u, (uint)Kernel32.FileAccess.ReadAttributes);
    }

    [Fact]
    public void FileAttributesValuesMatchWindowsHeader()
    {
        // winnt.h: FILE_FLAG_BACKUP_SEMANTICS 0x02000000, FILE_FLAG_OPEN_REPARSE_POINT 0x00200000
        Assert.Equal(0x02000000u, (uint)Kernel32.FileAttributes.BackupSemantics);
        Assert.Equal(0x00200000u, (uint)Kernel32.FileAttributes.OpenReparsePoint);
    }

    [Fact]
    public void ReparseDataConstantsMatchWindowsHeaders()
    {
        // winioctl.h: FSCTL_GET_REPARSE_POINT = CTL_CODE(FILE_DEVICE_FILE_SYSTEM, 42, METHOD_BUFFERED, FILE_ANY_ACCESS) = 0x000900A8
        Assert.Equal(0x000900A8, Kernel32.FSCTL_GET_REPARSE_POINT);

        // winioctl.h: MAXIMUM_REPARSE_DATA_BUFFER_SIZE = 16 * 1024
        Assert.Equal(16384, Kernel32.MAXIMUM_REPARSE_DATA_BUFFER_SIZE);

        // winnlt.h: IO_REPARSE_TAG_MOUNT_POINT = (0xA0000003L)
        Assert.Equal(0xA0000003u, Kernel32.IOReparseOptions.IO_REPARSE_TAG_MOUNT_POINT);
    }

    [Fact]
    public void SymbolicLinkReparseBufferMatchesWindowsLayout()
    {
        // winioctl.h: REPARSE_DATA_BUFFER's symlink sub-structure:
        // ULONG ReparseTag + USHORT ReparseDataLength + USHORT Reserved
        // + USHORT SubstituteNameOffset + USHORT SubstituteNameLength
        // + USHORT PrintNameOffset + USHORT PrintNameLength + ULONG Flags = 20 bytes.
        Assert.Equal(20, Marshal.SizeOf<Kernel32.SymbolicLinkReparseBuffer>());
    }
}
