// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath.Tests;

public class PathExtensionsTests
{
    [Theory]
    [InlineData("foo/bar.txt", ".txt")]
    [InlineData("/foo/bar.txt", ".txt")]
    [InlineData("foo/bar.", "")]
    [InlineData("foo/bar", null)]
    [InlineData(".gitignore", ".gitignore")]
    public void GetExtensionWithDotTests(string path, string? expected)
    {
        IPath local = new LocalPath(path);
        Assert.Equal(expected,  local.GetExtensionWithDot());

        if (!path.StartsWith('/')) return;

        IPath a = new AbsolutePath(path);
        Assert.Equal(expected, a.GetExtensionWithDot());
    }

    [Theory]
    [InlineData("foo/bar.txt", "txt")]
    [InlineData("/foo/bar.txt", "txt")]
    [InlineData("foo/bar.", "")]
    [InlineData("foo/bar", null)]
    [InlineData(".gitignore", "gitignore")]
    public void GetExtensionWithoutDotTests(string path, string? expected)
    {
        IPath l = new LocalPath(path);
        Assert.Equal(expected, l.GetExtensionWithoutDot());

        if (!path.StartsWith('/')) return;

        IPath a = new AbsolutePath(path);
        Assert.Equal(expected, a.GetExtensionWithoutDot());
    }
}
