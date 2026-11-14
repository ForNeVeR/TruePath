// SPDX-FileCopyrightText: 2025-2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
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

    [Fact]
    public void GetFilesByLocalPathPatternTest()
    {
        DoWithTempFolder(tempFolder =>
        {
            var matchingFile = tempFolder / "matching.txt";
            matchingFile.WriteAllText("test");
            (tempFolder / "other.log").WriteAllText("test");

            var files = tempFolder.GetFiles(new LocalPathPattern("*.txt"));

            Assert.Equal([matchingFile], files);
        });
    }

    [Fact]
    public void EnumerateDirectoriesTest()
    {
        DoWithTempFolder(tempFolder =>
        {
            var alpha = tempFolder / "alpha";
            var beta = tempFolder / "beta";
            var nested = alpha / "nested";
            nested.CreateDirectory();
            beta.CreateDirectory();
            (tempFolder / "file.txt").WriteAllText("test");

            Assert.Equal([alpha, beta], tempFolder.EnumerateDirectories().OrderBy(x => x.Value));
            Assert.Equal([alpha], tempFolder.EnumerateDirectories("a*"));
            Assert.Equal(
                [alpha, nested, beta],
                tempFolder.EnumerateDirectories("*", SearchOption.AllDirectories).OrderBy(x => x.Value));
        });
    }

    [Fact]
    public void EnumerateFileSystemEntriesTest()
    {
        DoWithTempFolder(tempFolder =>
        {
            var alpha = tempFolder / "alpha";
            var nested = alpha / "nested.txt";
            var beta = tempFolder / "beta.txt";
            alpha.CreateDirectory();
            nested.WriteAllText("test");
            beta.WriteAllText("test");

            Assert.Equal([alpha, beta], tempFolder.EnumerateFileSystemEntries().OrderBy(x => x.Value));
            Assert.Equal([alpha], tempFolder.EnumerateFileSystemEntries("a*"));
            Assert.Equal(
                [alpha, nested, beta],
                tempFolder.EnumerateFileSystemEntries("*", SearchOption.AllDirectories).OrderBy(x => x.Value));
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
