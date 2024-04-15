using System.Runtime.InteropServices;

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
    [InlineData(".", ".")]
    [InlineData("./foo", "foo")]
    [InlineData("..", "..")]
    [InlineData("./..", "..")]
    [InlineData("a/..", ".")]
    [InlineData("a/../..", "..")]
    [InlineData("a/../../.", "..")]
    [InlineData("foo/./bar/../var/./dar/..", "foo/var")]
    [InlineData("foo/.bar", "foo/.bar")]
    [InlineData("/.", "/")]
    [InlineData("/..", "/..")]
    [InlineData("x/foo/bar/../..", "x")]
    [InlineData("x/foo/bar/.../.", "x/foo/bar/...")]
    [InlineData("x/foo/..bar/", "x/foo/..bar")]
    public void DotFoldersAreTraversed(string input, string expected)
    {
        Assert.Equal(NormalizeSeparators(expected), PathStrings.Normalize(input));
    }

    private string NormalizeSeparators(string path) =>
        path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
}
