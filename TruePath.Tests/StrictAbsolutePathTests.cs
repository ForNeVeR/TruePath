// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

namespace TruePath.Tests;

public class StrictAbsolutePathTests
{
    [Fact]
    public void PathIsNormalizedOnCreation()
    {
        if (!OperatingSystem.IsWindows()) return;

        var path = @"C:/Users/John Doe\Documents";
        var absolutePath = new StrictAbsolutePath(path);
        Assert.Equal(@"C:\Users\John Doe\Documents", absolutePath.Value);
    }

    [Fact]
    public void ConstructorThrowsOnNonRootedPath()
    {
        var path = "uprooted";
        var ex = Assert.Throws<ArgumentException>(() => new StrictAbsolutePath(path));
        Assert.Equal("""Path "uprooted" is not absolute.""", ex.Message);
    }
}
