// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

using Xunit.Abstractions;

namespace TruePath.Tests;

public class LocalPathTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData("foo", ".")]
    [InlineData("foo/bar", "foo")]
    [InlineData("/", null)]
    public void AbsolutePathParent(string relativePath, string? expectedRelativePath)
    {
        var root = new AbsolutePath(OperatingSystem.IsWindows() ? @"A:\" : "/");
        var parent = root / relativePath;
        AbsolutePath? expectedPath = expectedRelativePath == null ? null : new(root / expectedRelativePath);
        Assert.Equal(expectedPath, parent.Parent);
    }

    [Theory]
    [InlineData(".", "..")]
    [InlineData("..", "../..")]
    [InlineData("../..", "../../..")]
    [InlineData("../../", "../../..")]
    [InlineData("../...", "..")]
    [InlineData(".../..", "..")]
    [InlineData("./.", "..")]
    [InlineData("../../.", "../../..")]
    [InlineData("b", ".")]
    [InlineData("../b", "..")]
    [InlineData("b/..", "b/../..")]
    public void RelativePathParent(string path, string? expected)
    {
        // Arrange
        var localPath = new LocalPath(path);
        LocalPath? expectedPath = expected == null ? null : new(expected);

        // Act
        var parent = localPath.Parent;

        // Assert
        Assert.Equal(expectedPath, parent);
    }

    [Theory]
    [InlineData("user", "user/documents")]
    [InlineData("usEr", "User/documents")]
    [InlineData("user/documents", "user/documents")]
    [InlineData("user/documents", "user")]
    public void IsPrefixOfShouldBeEquivalentToStartsWith(string pathA, string pathB)
    {
        // Arrange
        var a = new LocalPath(pathA);
        var b = new LocalPath(pathB);

        // Act
        var isPrefix = a.IsPrefixOf(b);
        var startsWith = b.Value.StartsWith(a.Value);

        // Assert
        Assert.Equal(isPrefix, startsWith);
    }

    [Fact]
    public void AbsolutePathIsNormalizedOnCreation()
    {
        if (!OperatingSystem.IsWindows()) return;

        const string path = @"C:/Users/John Doe\Documents";
        var absolutePath = new LocalPath(path);
        Assert.Equal(@"C:\Users\John Doe\Documents", absolutePath.Value);
    }

    [Theory]
    [InlineData("/foo/bar", "/foo", false)]
    [InlineData("/foo/", "/foo/bar/", true)]
    [InlineData("/foo", "/foo1/bar/", false)]
    [InlineData("/foo", "/foo1", false)]
    [InlineData("/foo", "/foo", true)]
    public void IsPrefixOf(string prefix, string other, bool result)
    {
        Assert.Equal(result, new LocalPath(prefix).IsPrefixOf(new LocalPath(other)));
    }

    [Fact]
    public void RelativePathIsNormalizedOnCreation()
    {
        if (!OperatingSystem.IsWindows()) return;

        const string path = @"Users/John Doe\Documents";
        var relativePath = new LocalPath(path);
        Assert.Equal(@"Users\John Doe\Documents", relativePath.Value);
    }

    [Fact]
    public void LocalPathConvertedFromAbsolute()
    {
        var absolutePath = new AbsolutePath("/foo/bar");
        LocalPath localPath1 = absolutePath;
        var localPath2 = new LocalPath(absolutePath);

        Assert.Equal(localPath1, localPath2);
    }

    [Fact]
    public void ResolveToCurrentDirectoryTests()
    {
        var localPath = new LocalPath("foo/bar");
        var currentDirectory = AbsolutePath.CurrentWorkingDirectory;
        var expected = currentDirectory / localPath;
        Assert.Equal(expected, localPath.ResolveToCurrentDirectory());

        try
        {
            var newCurrentDirectory = new AbsolutePath(Path.GetTempPath()).Canonicalize();
            output.WriteLine("New current directory: " + newCurrentDirectory);
            Environment.CurrentDirectory = newCurrentDirectory.Value;
            expected = newCurrentDirectory / localPath;
            Assert.Equal(expected, localPath.ResolveToCurrentDirectory());
        }
        finally
        {
            Environment.CurrentDirectory = currentDirectory.Value;
            output.WriteLine("Current directory reset back to: " + currentDirectory);
        }
    }
}
