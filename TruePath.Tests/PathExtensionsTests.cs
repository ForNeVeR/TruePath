// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath.Tests;

public class PathExtensionsTests
{
    [Theory]
    [InlineData("..", ".")]
    [InlineData("foo/bar.txt", ".txt")]
    [InlineData("/foo/bar.txt", ".txt")]
    [InlineData("foo/bar.", ".")]
    [InlineData("foo/bar", "")]
    [InlineData(".gitignore", ".gitignore")]
    public void GetExtensionWithDotTests(string path, string expected)
    {
        IPath local = new LocalPath(path);
        Assert.Equal(expected,  local.GetExtensionWithDot());

        if (!path.StartsWith('/')) return;

        IPath a = new AbsolutePath(path);
        Assert.Equal(expected, a.GetExtensionWithDot());
    }

    [Theory]
    [InlineData(".", "")]
    [InlineData("..", "")]
    [InlineData("foo/bar.txt", "txt")]
    [InlineData("/foo/bar.txt", "txt")]
    [InlineData("foo/bar.", "")]
    [InlineData("foo/bar", "")]
    [InlineData(".gitignore", "gitignore")]
    public void GetExtensionWithoutDotTests(string path, string expected)
    {
        IPath l = new LocalPath(path);
        Assert.Equal(expected, l.GetExtensionWithoutDot());

        if (!path.StartsWith('/')) return;

        IPath a = new AbsolutePath(path);
        Assert.Equal(expected, a.GetExtensionWithoutDot());
    }

    [Theory]
    [InlineData("..", ".")]
    [InlineData("foo/bar.txt", "bar")]
    [InlineData("/foo/bar.txt", "bar")]
    [InlineData("foo/bar.", "bar")]
    [InlineData("foo/bar", "bar")]
    [InlineData(".gitignore", "")]
    public void GetFilenameWithoutExtensionTests(string path, string expected)
    {
        IPath l = new LocalPath(path);
        Assert.Equal(expected, l.GetFilenameWithoutExtension());

        if (!path.StartsWith('/')) return;

        IPath a = new AbsolutePath(path);
        Assert.Equal(expected, a.GetFilenameWithoutExtension());
    }

    [Theory]
    [InlineData("..")]
    [InlineData("file.txt")]
    [InlineData("file..txt")]
    [InlineData(".gitignore")]
    [InlineData("gitignore.")]
    public void FileNameInvariantTests(string inputPath)
    {
        var path = new LocalPath(inputPath);
        var fileName = path.FileName;
        Assert.Equal(fileName, path.GetFilenameWithoutExtension() + path.GetExtensionWithDot());
    }

    [Theory]
    [InlineData(@"C:\", "bar", @"C:\.bar")]
    [InlineData(@"C:\filename.foo", "bar", @"C:\filename.bar")]
    [InlineData(@"\", "bar", @"\.bar")]
    [InlineData(@"\file", "bar", @"\file.bar")]
    [InlineData(@"\file", ".bar", @"\file.bar")]
    [InlineData(@"\file.", ".bar", @"\file.bar")]
    [InlineData("file.foo", "bar", "file.bar")]
    public void WithExtensionTests_Windows(string inputPath, string newExtension, string expected)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        // Arrange
        var path = new LocalPath(inputPath);

        // Act
        var newPath = path.WithExtension(newExtension);

        // Assert
        Assert.Equal(expected, newPath.Value);
    }

    [Theory]
    [InlineData("/", "bar", "/.bar")]
    [InlineData("/file", "bar", "/file.bar")]
    [InlineData("/file", ".bar", "/file.bar")]
    [InlineData("/file.", ".bar", "/file.bar")]
    [InlineData("file.foo", "bar", "file.bar")]
    public void WithExtensionTests_Unix(string inputPath, string @newExtension, string expected)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        // Arrange
        var path = new LocalPath(inputPath);

        // Act
        var newPath = path.WithExtension(newExtension);

        // Assert
        Assert.Equal(expected, newPath.Value);
    }
}
