// SPDX-FileCopyrightText: 2024-2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
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
