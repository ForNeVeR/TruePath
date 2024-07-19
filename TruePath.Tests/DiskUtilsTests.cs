// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Text;
using RuntimeInformation = System.Runtime.InteropServices.RuntimeInformation;

namespace TruePath.Tests;

public class DiskUtilsTests
{
    [Fact]
    public void DiskUtils_PassBackPath_ReturnCanonicalPath()
    {
        var tempPath = new AbsolutePath(Path.GetTempPath()).Canonicalize();
        var expected = tempPath.Value;
        var nonCanonicalPath = (tempPath / "foobar" / "..").Value;

        // Act
        var actual = DiskUtils.GetRealPath(nonCanonicalPath);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DiskUtils_OnCaseInsensitiveFs_PassNonCanonicalPath_ReturnCanonicalPath()
    {
        // Arrange
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return;

        var expected = new AbsolutePath(Path.GetTempPath()).Canonicalize();
        Assert.True(
            expected.Value.Split(Path.DirectorySeparatorChar)[1].Length > 0,
            $"""There should be at least one directory in the temporary path "{expected}" for this test.""");
        var nonCanonicalPath = InvertCase(expected.Value);
        Assert.NotEqual(expected.Value, nonCanonicalPath);

        // Act
        var actual = DiskUtils.GetRealPath(nonCanonicalPath);

        // Assert
        Assert.Equal(expected.Value, actual);
    }

    private static string InvertCase(string path)
    {
        var builder = new StringBuilder();
        foreach (var c in path)
        {
            builder.Append(char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c));
        }

        return builder.ToString();
    }
}
