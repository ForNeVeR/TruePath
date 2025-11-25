// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

using TruePath.SystemIo;

namespace TruePath.Tests;

public class PathIoTests
{
    [Fact]
    public void ExistsFileTest()
    {
        DoWithTempFolder(tempFolder =>
        {
            var file = tempFolder / "file.txt";
            Assert.False(file.ExistsFile());
            Assert.False(file.ExistsDirectory());
            Assert.False(file.Exists());
            file.WriteAllText("test");
            Assert.True(file.ExistsFile());
            Assert.False(file.ExistsDirectory());
            Assert.True(file.Exists());
        });
    }

    [Fact]
    public void ExistsFolderTest()
    {
        DoWithTempFolder(tempFolder =>
        {
            var file = tempFolder / "folder";
            Assert.False(file.ExistsFile());
            Assert.False(file.ExistsDirectory());
            Assert.False(file.Exists());
            file.CreateDirectory();
            Assert.False(file.ExistsFile());
            Assert.True(file.ExistsDirectory());
            Assert.True(file.Exists());
        });
    }

    private static void DoWithTempFolder(Action<AbsolutePath> test)
    {
        var tempFolder = Temporary.CreateTempFolder();
        try
        {
            test(tempFolder);
        }
        finally
        {
            tempFolder.DeleteDirectoryRecursively();
        }
    }
}
