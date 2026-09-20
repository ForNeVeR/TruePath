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
    [InlineData(@"C:\filename.foo", "bar", @"C:\filename.bar")]
    [InlineData(@"\file", "bar", @"\file.bar")]
    [InlineData(@"\file", ".bar", @"\file.bar")]
    [InlineData(@"\file.", ".bar", @"\file.bar")]
    [InlineData("file.foo", "bar", "file.bar")]
    [InlineData(".gitignore", "hgignore", ".hgignore")]
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
    [InlineData("file.txt", "md", "file.md")]
    [InlineData("file.txt", ".md", "file.md")]
    [InlineData("file", "md", "file.md")]
    [InlineData("file", ".md", "file.md")]
    [InlineData("archive.tar.gz", "zip", "archive.tar.zip")]
    [InlineData("archive.zip", ".tar.gz", "archive.tar.gz")]
    [InlineData("file.txt", "", "file.")]
    [InlineData("file.txt", null, "file")]
    [InlineData(".gitignore", "hgignore", ".hgignore")]
    public void WithExtensionArgumentTests(string inputPath, string? newExtension, string expected)
    {
        // Arrange
        var path = new LocalPath(inputPath);

        // Act
        var newPath = path.WithExtension(newExtension);

        // Assert
        Assert.Equal(expected, newPath.Value);
    }

    [Theory]
    [InlineData("/file", "bar", "/file.bar")]
    [InlineData("/file", ".bar", "/file.bar")]
    [InlineData("/file.", ".bar", "/file.bar")]
    [InlineData("file.foo", "bar", "file.bar")]
    [InlineData(".gitignore", "hgignore", ".hgignore")]
    public void WithExtensionTests_Unix(string inputPath, string newExtension, string expected)
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

    [Theory]
    [InlineData(".gitignore", null)]
    [InlineData(".gitignore", "")]
    [InlineData(".gitignore", ".")]
    [InlineData("foo/.gitignore", null)]
    [InlineData("..", null)]
    [InlineData("..", "bar")]
    [InlineData("../..", "txt")]
    [InlineData("", "txt")]
    [InlineData("a/..", "txt")]
    [InlineData("file.txt", "foo/bar")]
    public void WithExtensionThrowsIfTheResultIsNotAFileName(string inputPath, string? newExtension)
    {
        // Arrange
        var path = new LocalPath(inputPath);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => path.WithExtension(newExtension));
    }

    [Theory]
    [InlineData(@"C:\foo\.gitignore", null)]
    [InlineData(@"C:\.gitignore", null)]
    [InlineData(@"C:\", "bar")]
    [InlineData(@"\", "bar")]
    public void WithExtensionThrowsIfTheResultIsNotAFileName_Windows(string inputPath, string? newExtension)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        // Arrange
        var local = new LocalPath(inputPath);
        var absolute = new AbsolutePath(inputPath);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => local.WithExtension(newExtension));
        Assert.Throws<ArgumentException>(() => absolute.WithExtension(newExtension));
    }

    [Theory]
    [InlineData("/foo/.gitignore", null)]
    [InlineData("/.gitignore", null)]
    [InlineData("/", "bar")]
    public void WithExtensionThrowsIfTheResultIsNotAFileName_Unix(string inputPath, string? newExtension)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        // Arrange
        var local = new LocalPath(inputPath);
        var absolute = new AbsolutePath(inputPath);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => local.WithExtension(newExtension));
        Assert.Throws<ArgumentException>(() => absolute.WithExtension(newExtension));
    }
}
