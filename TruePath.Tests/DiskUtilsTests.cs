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
        // Arrange
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

        var currentDirectory = Environment.CurrentDirectory;
        var directories = currentDirectory.Split("/").ToList();
        var back = Random.Shared.Next(directories.Count);
        var expected = Back(directories, back, "/");

        var nonCanonicalPath = new string(currentDirectory) + string.Concat(Enumerable.Repeat("/..", back));

        // Act
        var actual = DiskUtils.GetRealPath(nonCanonicalPath);

        // Assert
        Assert.Equal(expected, actual);
    }

    private static string Back(List<string> folders, int stepsBack, string delimiter)
    {
        int finalIndex = folders.Count - stepsBack - 1;
        List<string> finalFolders = folders[..(finalIndex + 1)];

        if (finalFolders.Count == 1)
        {
            return delimiter;
        }

        return string.Join(delimiter, finalFolders);
    }

    private static IEnumerable<char> MakeNonCanonicalPath(string path)
    {
        foreach (var @char in path)
        {
            if (char.IsLetter(@char) && Random.Shared.NextSingle() >= 0.5)
            {
                yield return char.ToUpper(@char);
                continue;
            }

            yield return @char;
        }
    }
}
