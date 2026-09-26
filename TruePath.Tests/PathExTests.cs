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
    public void GetRelativePathBehavesAsPathGetRelativePathOnWindows(string relativeTo, string path)
    {
        if (!OperatingSystem.IsWindows()) return;
        AssertSameRelativePath(relativeTo, path);
    }

    [Theory]
    [InlineData("/a", "/b/c")]
    [InlineData("/", "/a")]
    [InlineData("/a/b", "/a")]
    public void GetRelativePathBehavesAsPathGetRelativePathOnUnix(string relativeTo, string path)
    {
        if (OperatingSystem.IsWindows()) return;
        AssertSameRelativePath(relativeTo, path);
    }

    private static void AssertSameRelativePath(string relativeTo, string path)
    {
        var expected = new LocalPath(Path.GetRelativePath(relativeTo, path));
        var actual = new LocalPath(PathEx.GetRelativePath(relativeTo, path));
        Assert.Equal(expected, actual);
    }
}
