// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath.Tests;

public class Kernel32Tests
{
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
