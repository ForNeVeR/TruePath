// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

namespace TruePath.Tests;

public class AbsolutePathTests
{
    [Fact]
    public void PathIsNormalizedOnCreation()
    {
        if (!OperatingSystem.IsWindows()) return;

        const string path = @"C:/Users/John Doe\Documents";
        var absolutePath = new AbsolutePath(path);
        Assert.Equal(@"C:\Users\John Doe\Documents", absolutePath.Value);
    }

    [Fact]
    public void ConstructorThrowsOnNonRootedPath()
    {
        const string path = "uprooted";
        const string expectedMessage = $"Path \"{path}\" is not absolute.";
        var ex = Assert.Throws<ArgumentException>(() => new AbsolutePath(path));
        Assert.Equal(expectedMessage, ex.Message);

        var localPath = new LocalPath("uprooted");
        ex = Assert.Throws<ArgumentException>(() => new AbsolutePath(localPath));
        Assert.Equal(expectedMessage, ex.Message);
    }
}
