// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using Xunit.Abstractions;

namespace TruePath.Tests;

public class PathStringsTests(ITestOutputHelper output)
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
    public void WindowsSpecificDotFoldersAreTraversed(string input, string expected)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

        // Arrange
        var driveLetter = RandomDriveLetter();
        input = driveLetter + input;
        expected = driveLetter + expected;

        output.WriteLine($"{driveLetter} is selected as the drive letter.");

        //Act
        var actual = PathStrings.Normalize(input);

        // Assert
        Assert.Equal(NormalizeSeparators(expected), actual);
    }

    private static string NormalizeSeparators(string path) =>
        path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

    private static string RandomDriveLetter()
    {
        var rnd = new Random(Guid.NewGuid().GetHashCode());

        int lowerBound = Convert.ToInt16('A');
        int upperBound = Convert.ToInt16('Z');

        var letterIndex = rnd.Next(lowerBound, upperBound + 1); // include upperBound

        var letter = Convert.ToChar(letterIndex);

        if (rnd.NextSingle() >= 0.5)
        {
            letter = char.ToLower(letter);
        }

        return letter + ":";
    }
}
