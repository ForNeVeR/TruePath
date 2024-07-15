// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

namespace TruePath.Tests;

public class PathStringsTests
{
    [Fact]
    public void SlashesShouldBeNormalized()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
        const string path = @"a/b\c/d";
        Assert.Equal(@"a\b\c\d", PathStrings.Normalize(path));
    }

    [Theory]
    [InlineData("/a/b/c/d", "/a/b/c/d")]
    [InlineData("/a/b/c/d/", "/a/b/c/d")]
    [InlineData("a/b/c/d//", "a/b/c/d")]
    [InlineData("a/b/c/d////", "a/b/c/d")]
    public void TrailingSlashShouldBeRemoved(string input, string expected)
    {
        Assert.Equal(NormalizeSeparators(expected), PathStrings.Normalize(input));
    }

    [Theory]
    [InlineData("/a/b/c/d", "/a/b/c/d")]
    [InlineData("//a/b/c/d", "/a/b/c/d")]
    [InlineData("//a///b//c/d//", "/a/b/c/d")]
    [InlineData("a///b////c/d//", "a/b/c/d")]
    public void SeparatorsAreDeduplicated(string input, string expected)
    {
        Assert.Equal(NormalizeSeparators(expected), PathStrings.Normalize(input));
    }

    [Theory]
    [InlineData(".", "")]
    [InlineData("./foo", "foo")]
    [InlineData("..", "..")]
    [InlineData("./..", "..")]
    [InlineData("a/..", "")]
    [InlineData("a/../..", "..")]
    [InlineData("a/../../.", "..")]
    [InlineData("a/../../..", "../..")]
    [InlineData("foo/./bar/../var/./dar/..", "foo/var")]
    [InlineData("foo/.bar", "foo/.bar")]
    [InlineData("/.", "/")]
    [InlineData("/..", "/..")]
    [InlineData("/../..", "/../..")]
    [InlineData("/../../foo/..", "/../..")]
    [InlineData("x/foo/bar/../..", "x")]
    [InlineData("x/foo/bar/.../.", "x/foo/bar/...")]
    [InlineData("x/foo/..bar/", "x/foo/..bar")]
    [InlineData("../../foo", "../../foo")]
    [InlineData("../../../foo", "../../../foo")]
    [InlineData("../foo/..", "..")]
    public void DotFoldersAreTraversed(string input, string expected)
    {
        Assert.Equal(NormalizeSeparators(expected), PathStrings.Normalize(input));
    }

    [Theory(Skip = "TODO[#16]: Make it work properly")]
    [InlineData("C:.", "C:")]
    [InlineData("C:./foo", "C:foo")]
    [InlineData("C:..", "C:..")]
    [InlineData("C:/..", "C:/..")]
    public void WindowsSpecificDotFoldersAreTraversed(string input, string expected)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
        Assert.Equal(NormalizeSeparators(expected), PathStrings.Normalize(input));
    }

    private static string NormalizeSeparators(string path) =>
        path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
}
