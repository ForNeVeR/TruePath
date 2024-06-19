// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

namespace TruePath.Tests;

public class LocalPathTests
{
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
}
