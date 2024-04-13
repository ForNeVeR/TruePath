// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

namespace TruePath.Tests;

public class RelativePathTests
{
    [Fact]
    public void PathIsNormalizedOnCreation()
    {
        if (!OperatingSystem.IsWindows()) return;

        var path = @"Users/John Doe\Documents";
        var relativePath = new RelativePath(path);
        Assert.Equal(@"C:\Users\John Doe\Documents", relativePath.Value);
    }
}
