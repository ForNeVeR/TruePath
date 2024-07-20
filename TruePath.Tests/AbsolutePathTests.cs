// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace TruePath.Tests;

public class AbsolutePathTests
{
    [Fact]
    public void ReadKind_NonExistent()
    {
        // Arrange
        var currentDirectory = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid().ToString());
        var localPath = new AbsolutePath(currentDirectory);

        // Act
        var kind = localPath.ReadKind();

        // Assert
        Assert.Null(kind);
    }

    [Fact]
    public void ReadKind_IsDirectory()
    {
        // Arrange
        var currentDirectory = Environment.CurrentDirectory;
        var localPath = new AbsolutePath(currentDirectory);

        // Act
        var kind = localPath.ReadKind();

        // Assert
        Assert.Equal(FileEntryKind.Directory, kind);
    }

    [Fact]
    public void ReadKind_IsFile()
    {
        // Arrange
        string tempFilePath = Path.GetTempFileName();
        var localPath = new AbsolutePath(tempFilePath);

        // Act
        var kind = localPath.ReadKind();

        // Assert
        Assert.Equal(FileEntryKind.File, kind);
    }

    [Fact]
    public void OnWindows_ReadKind_IsJunction()
    {
        // Arrange
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

        var currentDirectory = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid().ToString());
        var localPath = new AbsolutePath(currentDirectory);

        var tempDirectoryInfo = Path.GetTempPath();

        var created = CreateJunction(currentDirectory, tempDirectoryInfo);

        Assert.True(created);

        // Act
        var kind = localPath.ReadKind();

        // Assert
        Assert.Equal(FileEntryKind.Junction, kind);

        Directory.Delete(currentDirectory, true);
    }

    [Fact]
    public void ReadKind_IsSymlink()
    {
        // Arrange
        var currentDirectory = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid().ToString());
        var localPath = new AbsolutePath(currentDirectory);

        var tempDirectoryInfo = Path.GetTempPath();

        Directory.CreateSymbolicLink(currentDirectory, tempDirectoryInfo);

        // Act
        var kind = localPath.ReadKind();

        // Assert
        Assert.Equal(FileEntryKind.Symlink, kind);

        Directory.Delete(currentDirectory, true);
    }

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

    private static bool CreateJunction(string path, string target)
    {
        return Mklink(path, target, "J");
    }

    private static bool Mklink(string path, string target, string type)
    {
        string cmdline = $"cmd /c mklink /{type} {path} {target}";

        ProcessStartInfo si = new ProcessStartInfo("cmd.exe", cmdline)
        {
            UseShellExecute = false
        };

        Process? p = Process.Start(si);
        if (p == null)
        {
            return false;
        }
        p.WaitForExit();

        return p.ExitCode == 0;
    }
}
