// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

namespace TruePath.Tests;

public class AbsolutePathTests
{
    [Theory]
    [InlineData("/home/user", "/home/user/documents")]
    [InlineData("/home/usEr", "/home/User/documents")]
    [InlineData("/home/user/documents", "/home/user/documents")]
    [InlineData("/home/user/documents", "/home/user")]
    public void IsPrefixOfShouldBeEquivalentToStartsWith(string pathA, string pathB)
    {
        if (OperatingSystem.IsWindows()) return;

        // Arrange
        var a = new AbsolutePath(pathA);
        var b = new AbsolutePath(pathB);

        // Act
        var isPrefix = a.IsPrefixOf(b);
        var startsWith = b.Value.StartsWith(a.Value);

        // Assert
        Assert.Equal(isPrefix, startsWith);
    }

    [Fact]
    public void CurrentWorkingDirectoryShouldReturnCorrectAbsolutePath()
    {
        // Arrange
        var expectedPath = Directory.GetCurrentDirectory();

        // Act
        var actualPath = AbsolutePath.CurrentWorkingDirectory;

        // Assert
        Assert.Equal(expectedPath, actualPath.Value);
    }

    [Fact]
    public void CurrentWorkingDirectoryShouldBeAbsolute()
    {
        // Act
        var path = AbsolutePath.CurrentWorkingDirectory;

        // Assert
        Assert.True(Path.IsPathRooted(path.Value));
    }

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

    [Theory]
    [InlineData("/etc/bin", "/usr/bin", "../../usr/bin")]
    [InlineData("/usr/bin/log", "/usr/bin", "..")]
    [InlineData("/usr/bin", "/usr/bin/log", "log")]
    public void RelativeToReturnsCorrectRelativePath(string from, string to, string expected)
    {
        if (OperatingSystem.IsWindows()) return;

        var fromPath = new AbsolutePath(from);
        var toPath = new AbsolutePath(to);

        LocalPath relativePath = toPath.RelativeTo(fromPath);

        Assert.Equal(expected, relativePath.Value);
    }

    [Theory]
    [InlineData(@"C:\bin", @"D:\bin", @"D:\bin")]
    [InlineData(@"C:\bin\debug", @"C:\bin", "..")]
    [InlineData(@"C:\bin", @"C:\bin\log", "log")]
    public void RelativeToReturnsCorrectRelativePathForWindows(string from, string to, string expected)
    {
        if (OperatingSystem.IsWindows() is false) return;

        var fromPath = new AbsolutePath(from);
        var toPath = new AbsolutePath(to);

        LocalPath relativePath = toPath.RelativeTo(fromPath);

        Assert.Equal(expected, relativePath.Value);
    }

    [Fact]
    public void CanonicalizationCaseOnMacOs()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return;

        var newDirectory = new AbsolutePath(Path.GetTempFileName()).Canonicalize();
        File.Delete(newDirectory.ToString());
        newDirectory /= "foobar";
        Directory.CreateDirectory(newDirectory.Value);

        var incorrectCaseDirectory = newDirectory.Parent!.Value / "FOOBAR";
        var result = incorrectCaseDirectory.Canonicalize();
        Assert.Equal(newDirectory.Value, result.Value);
    }
}
