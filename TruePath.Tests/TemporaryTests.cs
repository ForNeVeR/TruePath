// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath.Tests;

public class TemporaryTests
{
    [Fact]
    public void SystemTempDirectory_ReturnsValidDirectory()
    {
        // Act
        var tempDir = Temporary.SystemTempDirectory();

        // Assert
        Assert.True(Directory.Exists(tempDir.Value));
    }

    [Fact]
    public void CreateTempFile_CreatesNewFile()
    {
        // Act
        var tempFile = Temporary.CreateTempFile();

        // Assert
        Assert.True(File.Exists(tempFile.Value));

        // Cleanup
        File.Delete(tempFile.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("test_")]
    public void CreateTempFolder_CreatesNewFolder(string? prefix)
    {
        // Act
        var tempFolder = Temporary.CreateTempFolder(prefix);

        // Assert
        Assert.True(Directory.Exists(tempFolder.Value));
        if (prefix != null)
        {
            Assert.StartsWith(prefix, tempFolder.FileName);
        }

        // Cleanup
        Directory.Delete(tempFolder.Value);
    }
}

