// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
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

    [Fact]
    public void RootPathEndWithSeparator()
    {
        var root = OperatingSystem.IsWindows() ? @"A:\" : "/";
        Assert.Equal(root, PathStrings.Normalize(root));

        var rootWithRepeatedSeparators = OperatingSystem.IsWindows() ? @"A:\\\\" : "//";
        Assert.Equal(root, PathStrings.Normalize(rootWithRepeatedSeparators));
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
    [InlineData("...", "...")]
    [InlineData(".../..", "")]
    [InlineData(".../...", ".../...")]
    [InlineData(".../../...", "...")]
    [InlineData("foo/bar/../file.ext", "foo/file.ext")]
    [InlineData("N/", "N")]
    public void DotFoldersAreTraversedCorrectly(string input, string expected)
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

        var driveLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        foreach (var driveLetter in driveLetters)
        {
            var inputPath = $"{driveLetter}:{input}";
            var expectedPath = $"{driveLetter}:{expected}";

            //Act
            var actual = PathStrings.Normalize(inputPath);

            // Assert
            Assert.Equal(NormalizeSeparators(expectedPath), actual);
        }

        driveLetters += driveLetters.ToLowerInvariant();
        foreach (var driveLetter in driveLetters)
        {
            var inputPath = $"{driveLetter}:{input}";
            var expectedPath = $"{driveLetter}:{expected}";

            //Act
            var actual = PathStrings.Normalize(inputPath);

            // Assert
            Assert.Equal(NormalizeSeparators(expectedPath), actual);
        }
    }

    [Theory]
    [InlineData("C:/", "C:")]
    [InlineData("C:/../file", "file")]
    public void Normalize_DriveLetterCheckDisabled_TreatsColonAsRegularPathCharacter(string input, string expected)
    {
        var actual = PathStrings.Normalize(input, driveBasedSystem: false);

        Assert.Equal(NormalizeSeparators(expected), actual);
    }

    [Theory]
    [InlineData("C:/", "C:/")]
    [InlineData("C:/../file", "C:/../file")]
    public void Normalize_DriveLetterCheckEnabled_PreservesWindowsDrivePrefix(string input, string expected)
    {
        var actual = PathStrings.Normalize(input, driveBasedSystem: true);

        Assert.Equal(NormalizeSeparators(expected), actual);
    }

    [Fact]
    public void Normalize_DriveLetterHandling_MatchesCurrentPlatform()
    {
        var actual = PathStrings.Normalize("C:/../file");
        var expected = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "C:/../file" : "file";

        Assert.Equal(NormalizeSeparators(expected), actual);
    }

    private static string NormalizeSeparators(string path) =>
        path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
}
