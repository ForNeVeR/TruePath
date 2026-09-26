// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath.Tests;

public class PathExTests
{
    [Theory]
    [InlineData("a", "a/b")]
    [InlineData("a/b", "a")]
    [InlineData("a/b", "a/c")]
    [InlineData("a", "a")]
    [InlineData("a", "b")]
    [InlineData(".", "a")]
    [InlineData("a b", "a b/c d")]
    [InlineData("a", "a/b#c")]
    [InlineData("a", "a/%41")]
    [InlineData("a", "a/b/")]
    [InlineData("a/", "a")]
    [InlineData("Foodie", "Foobar")]
    public void GetRelativePathBehavesAsPathGetRelativePath(string relativeTo, string path)
    {
        AssertSameRelativePath(relativeTo, path);
    }

    [Theory]
    [InlineData(@"C:\a", @"D:\b")]
    [InlineData(@"C:\a", @"C:\a")]
    [InlineData(@"c:\a", @"C:\a\b")]
    [InlineData(@"\a", @"\a\b")]
    [InlineData("A:x", @"A:x\y")]
    [InlineData(@"C:\A", @"c:\a\b")]
    [InlineData(@"C:\a\", @"C:\a")]
    [InlineData(@"C:\Foodie", @"C:\Foobar")]
    public void GetRelativePathBehavesAsPathGetRelativePathOnWindows(string relativeTo, string path)
    {
        if (!OperatingSystem.IsWindows()) return;
        AssertSameRelativePath(relativeTo, path);
    }

    [Theory]
    [InlineData("/a", "/b/c")]
    [InlineData("/", "/a")]
    [InlineData("/a/b", "/a")]
    [InlineData("/a/", "/a/b/")]
    [InlineData("/Foodie", "/Foobar")]
    public void GetRelativePathBehavesAsPathGetRelativePathOnUnix(string relativeTo, string path)
    {
        if (OperatingSystem.IsWindows()) return;
        AssertSameRelativePath(relativeTo, path);
    }

    [Theory]
    [InlineData(null, "a")]
    [InlineData("a", null)]
    [InlineData("", "a")]
    [InlineData("a", "")]
    public void GetRelativePathThrowsAsPathGetRelativePath(string? relativeTo, string? path)
    {
        AssertSameException(relativeTo, path);
    }

    [Theory]
    [InlineData("   ", "a")]
    [InlineData("a", "   ")]
    public void GetRelativePathThrowsAsPathGetRelativePathOnWindows(string relativeTo, string path)
    {
        if (!OperatingSystem.IsWindows()) return;
        AssertSameException(relativeTo, path);
    }

    private static void AssertSameRelativePath(string relativeTo, string path)
    {
        var expected = Path.GetRelativePath(relativeTo, path);
        var actual = PathEx.GetRelativePath(relativeTo, path);
        Assert.Equal(expected, actual);
    }

    private static void AssertSameException(string? relativeTo, string? path)
    {
        var expected = Assert.ThrowsAny<ArgumentException>(() => Path.GetRelativePath(relativeTo!, path!));
        var actual = Assert.ThrowsAny<ArgumentException>(() => PathEx.GetRelativePath(relativeTo!, path!));
        Assert.Equal(expected.GetType(), actual.GetType());
        Assert.Equal(expected.ParamName, actual.ParamName);
    }
}
