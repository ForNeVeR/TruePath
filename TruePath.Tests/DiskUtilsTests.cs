// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Text;
using TruePath.SystemIo;
using RuntimeInformation = System.Runtime.InteropServices.RuntimeInformation;

namespace TruePath.Tests;

public class DiskUtilsTests
{
    [Fact]
    public void DiskUtils_PassBackPath_ReturnCanonicalPath()
    {
        var tempPath = new AbsolutePath(Path.GetTempPath()).Canonicalize();
        var expected = tempPath.Value;
        var nonCanonicalPath = (tempPath / "foobar" / "..").Value;

        // Act
        var actual = DiskUtils.GetRealPath(nonCanonicalPath);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DiskUtils_OnCaseInsensitiveFs_PassNonCanonicalPath_ReturnCanonicalPath()
    {
        // Arrange
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return;

        var expected = new AbsolutePath(Path.GetTempPath()).Canonicalize();
        Assert.True(
            expected.Value.Split(Path.DirectorySeparatorChar)[1].Length > 0,
            $"""There should be at least one directory in the temporary path "{expected}" for this test.""");
        var nonCanonicalPath = InvertCase(expected.Value);
        Assert.NotEqual(expected.Value, nonCanonicalPath);

        // Act
        var actual = DiskUtils.GetRealPath(nonCanonicalPath);

        // Assert
        Assert.Equal(expected.Value, actual);
    }

    [Fact]
    public void DiskUtils_PassNonExistentPath_KeepFileDescriptorZeroOpen()
    {
        // Arrange
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

        var placeholderPath = Temporary.CreateTempFile();
        try
        {
            // File descriptor 0 (normally the standard input) may be closed in the test process: then the next opened
            // file takes it.
            using var placeholder = IsFileDescriptorOpen(0) ? null : File.OpenHandle(placeholderPath.Value);
            Assert.True(IsFileDescriptorOpen(0), "File descriptor 0 should be open before the test.");
            var nonExistentPath = Temporary.SystemTempDirectory() / Guid.NewGuid().ToString() / "file.txt";

            // Act
            var actual = DiskUtils.GetRealPath(nonExistentPath.Value);

            // Assert
            Assert.Equal(nonExistentPath, new AbsolutePath(actual));
            Assert.True(IsFileDescriptorOpen(0), "File descriptor 0 should stay open.");
        }
        finally
        {
            placeholderPath.Delete();
        }
    }

    [Fact]
    public void ParseReparseDataBuffer_EmptyBuffer_ReturnsNull()
    {
        Assert.Null(DiskUtils.ParseReparseDataBuffer(ReadOnlySpan<byte>.Empty));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(7)]
    public void ParseReparseDataBuffer_BufferShorterThanHeader_ReturnsNull(int length)
    {
        Assert.Null(DiskUtils.ParseReparseDataBuffer(new byte[length]));
    }

    [Fact]
    public void ParseReparseDataBuffer_ReturnsCommonHeaderFields()
    {
        // Arrange
        var expected = new Kernel32.ReparseDataBufferHeader
        {
            ReparseTag = Kernel32.IOReparseOptions.IO_REPARSE_TAG_MOUNT_POINT,
            ReparseDataLength = 0x18,
            Reserved = 0
        };
        var buffer = new byte[Marshal.SizeOf<Kernel32.ReparseDataBufferHeader>()];
        MemoryMarshal.Write(buffer, in expected);

        // Act
        var actual = DiskUtils.ParseReparseDataBuffer(buffer);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected.ReparseTag, actual.Value.ReparseTag);
        Assert.Equal(expected.ReparseDataLength, actual.Value.ReparseDataLength);
        Assert.Equal(expected.Reserved, actual.Value.Reserved);
    }

    [Fact]
    public void ParseReparseDataBuffer_IgnoresTagSpecificDataAfterHeader()
    {
        // Arrange: a mount point response has no symbolic-link fields, only the common header is valid.
        var headerSize = Marshal.SizeOf<Kernel32.ReparseDataBufferHeader>();
        var expectedTag = Kernel32.IOReparseOptions.IO_REPARSE_TAG_MOUNT_POINT;
        var buffer = new byte[headerSize + 12];
        MemoryMarshal.Write(buffer.AsSpan(0, sizeof(uint)), in expectedTag);
        buffer.AsSpan(headerSize).Fill(0xFF);

        // Act
        var actual = DiskUtils.ParseReparseDataBuffer(buffer);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expectedTag, actual.Value.ReparseTag);
    }

    [Fact]
    public void TryGetReparseDataBuffer_OnNonReparsePoint_ReturnsNull()
    {
        // Arrange
        if (!OperatingSystem.IsWindows()) return;

        var directory = Temporary.CreateTempFolder();
        try
        {
            // Act
            var actual = DiskUtils.TryGetReparseDataBuffer(directory.Value);

            // Assert
            Assert.Null(actual);
        }
        finally
        {
            directory.DeleteEmptyDirectory();
        }
    }

    // ReSharper disable once InconsistentNaming
    private const int F_GETFD = 1; // The same on Linux and macOS.

    [DllImport("libc", EntryPoint = "fcntl", SetLastError = true)]
    private static extern int FCntl(int fd, int cmd);

    private static bool IsFileDescriptorOpen(int fd) => FCntl(fd, F_GETFD) != -1;

    private static string InvertCase(string path)
    {
        var builder = new StringBuilder();
        foreach (var c in path)
        {
            builder.Append(char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c));
        }

        return builder.ToString();
    }
}
