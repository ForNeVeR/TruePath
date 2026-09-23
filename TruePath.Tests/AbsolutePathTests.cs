// SPDX-FileCopyrightText: 2024-2026 TruePath contributors <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace TruePath.Tests;

public class AbsolutePathTests
{
    [Fact]
    public void ConstructionTest()
    {
        var root = Utils.SyntheticRoot;
        var path = new AbsolutePath($"{root}/...");
        Assert.Equal($"{root}...", path.Value);
    }

    [Fact]
    public void PathRootReturnsRoot()
    {
      var root = Utils.SyntheticRoot;
      var path = root / "foo" / "bar";

     Assert.Equal(root, path.PathRoot);
    }

    [Fact]
    public void PathRootOfRootReturnsItself()
    {
       var root = Utils.SyntheticRoot;

     Assert.Equal(root, root.PathRoot);
    }

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

        try
        {
            Directory.CreateSymbolicLink(currentDirectory, tempDirectoryInfo);
        }
        catch (IOException e) when (OperatingSystem.IsWindows() && !Utils.RunsOnCi())
        {
            if (e.Message.Contains("privilege") && (e.StackTrace?.Contains("CreateSymbolicLink") ?? false))
            {
                Assert.Skip("Tests involving symlink creation require elevated privileges on Windows, as this is enforced at the OS level.");
            }
            else throw;
        }

        // Act
        var kind = localPath.ReadKind();

        // Assert
        Assert.Equal(FileEntryKind.Symlink, kind);

        Directory.Delete(currentDirectory, true);
    }

    [Theory]
    [InlineData("foo", ".")]
    [InlineData("foo/bar", "foo")]
    [InlineData("/", null)]
    public void ParentIsCalculatedCorrectly(string relativePath, string? expectedRelativePath)
    {
        var root = Utils.SyntheticRoot;
        var parent = root / relativePath;
        AbsolutePath? expectedPath = expectedRelativePath == null ? null : new(root / expectedRelativePath);
        Assert.Equal(expectedPath, parent.Parent);
    }

    [Theory]
    [InlineData("home/user", "home/user/documents", true)]
    [InlineData("home/user/documents", "home/user/documents", true)]
    [InlineData("home/user/documents", "home/user", false)]
    public void IsPrefixOfShouldBeEquivalentToStartsWith(string pathA, string pathB, bool expected)
    {
        // Arrange
        var a = Utils.SyntheticRoot / pathA;
        var b = Utils.SyntheticRoot / pathB;

        // Assert
        Assert.Equal(expected, a.IsPrefixOf(b));
        Assert.Equal(expected, b.StartsWith(a));
    }

    [Fact]
    public void IsPrefixOfFollowsPlatformCaseSensitivityForSameName()
    {
        var root = Utils.SyntheticRoot;
        var a = root / "Foo";
        var b = root / "foo";

        Assert.Equal(Utils.IsPlatformCaseInsensitive(), a.IsPrefixOf(b));
        Assert.Equal(Utils.IsPlatformCaseInsensitive(), b.StartsWith(a));
        Assert.Equal(a == b, a.IsPrefixOf(b));
    }

    [Fact]
    public void IsPrefixOfFollowsPlatformCaseSensitivityForDescendant()
    {
        var root = Utils.SyntheticRoot;
        var a = root / "Foo";
        var b = root / "foo/file.txt";

        Assert.Equal(Utils.IsPlatformCaseInsensitive(), a.IsPrefixOf(b));
        Assert.Equal(Utils.IsPlatformCaseInsensitive(), b.StartsWith(a));
    }

    [Fact]
    public void IsPrefixOfRequiresWholeSegmentRegardlessOfCase()
    {
        var root = Utils.SyntheticRoot;
        var a = root / "Foo";
        var b = root / "foobar";

        Assert.False(a.IsPrefixOf(b));
        Assert.False(b.StartsWith(a));
    }

    [Theory]
    [InlineData("sub", "sub/a.txt", true)]
    [InlineData("sub", "sub", true)]
    [InlineData("sub", "subx/a.txt", false)]
    [InlineData("sub", "submarine", false)]
    [InlineData("sub/folder", "sub", false)]
    public void IsPrefixOfRespectsPathSegmentBoundaries(string prefix, string other, bool expected)
    {
        var root = Utils.SyntheticRoot;

        Assert.Equal(expected, (root / prefix).IsPrefixOf(root / other));
    }

    [Fact]
    public void IsPrefixOfTreatsRootAsPrefixOfDescendants()
    {
        var root = Utils.SyntheticRoot;

        Assert.True(root.IsPrefixOf(root / "sub" / "a.txt"));
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
    public void CurrentWorkingDirectoryGetsChanged()
    {
        var prevPath = AbsolutePath.CurrentWorkingDirectory;
        var path = new AbsolutePath(Environment.ProcessPath!).Parent!.Value;
        try
        {
            AbsolutePath.CurrentWorkingDirectory = path;
            Assert.Equal(path, new AbsolutePath(Environment.CurrentDirectory));
        }
        finally
        {
            AbsolutePath.CurrentWorkingDirectory = prevPath;
        }
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
    [InlineData(@"\Windows")]
    [InlineData("C:Windows")]
    [InlineData("C:")]
    public void ConstructorThrowsOnDriveRelativePathOnWindows(string path)
    {
        if (!OperatingSystem.IsWindows()) return;

        var expectedMessage = $"Path \"{path}\" is not absolute.";
        var ex = Assert.Throws<ArgumentException>(() => new AbsolutePath(path));
        Assert.Equal(expectedMessage, ex.Message);

        ex = Assert.Throws<ArgumentException>(() => new AbsolutePath(new LocalPath(path)));
        Assert.Equal(expectedMessage, ex.Message);
    }

    [Theory]
    [InlineData(@"\x", @"C:\x")]
    [InlineData("C:x", @"C:\base\x")]
    [InlineData("c:x", @"C:\base\x")]
    [InlineData(@"D:\x", @"D:\x")]
    public void AppendDriveRelativePathOnWindows(string appended, string expected)
    {
        if (!OperatingSystem.IsWindows()) return;

        var basePath = new AbsolutePath(@"C:\base");
        Assert.Equal(expected, (basePath / appended).Value);
    }

    [Fact]
    public void AppendPathRelativeToAnotherDriveThrowsOnWindows()
    {
        if (!OperatingSystem.IsWindows()) return;

        var basePath = new AbsolutePath(@"C:\base");
        var ex = Assert.Throws<ArgumentException>(() => basePath / "D:x");
        Assert.Equal("Path \"D:x\" is not absolute.", ex.Message);
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

    [Fact]
    public void PlatformDefaultPathComparerTest()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

        var path1 = new AbsolutePath(@"C:\Windows");
        var path2 = new AbsolutePath(@"C:\WINDOWS");

        Assert.True(path1.Equals(path2, AbsolutePath.PlatformDefaultComparer));
    }

    [Fact]
    public void EqualsUseStrictStringPathComparer_SamePaths_True()
    {
        // Arrange
        var currentDirectory = Environment.CurrentDirectory;
        var nonCanonicalPath = currentDirectory;

        var path1 = new AbsolutePath(currentDirectory);
        var path2 = new AbsolutePath(nonCanonicalPath);

        // Act
        var equals = path1.Equals(path2, AbsolutePath.StrictStringComparer);

        // Assert
        Assert.True(equals);
    }

    [Fact]
    public void EqualsUseStrictStringPathComparer_NotSamePaths_False()
    {
        // Arrange
        var currentDirectory = Environment.CurrentDirectory;
        var nonCanonicalPath = new string(currentDirectory.ToNonCanonicalCase().ToArray());

        var path1 = new AbsolutePath(currentDirectory);
        var path2 = new AbsolutePath(nonCanonicalPath);

        // Act
        var equals = path1.Equals(path2, AbsolutePath.StrictStringComparer);

        // Assert
        Assert.False(equals);
    }

    [Fact]
    public void OnLinux_EqualsDefault_CaseSensitive_False()
    {
        // Arrange
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        var currentDirectory = Environment.CurrentDirectory;
        var nonCanonicalPath = new string(currentDirectory.ToNonCanonicalCase().ToArray());

        var path1 = new AbsolutePath(currentDirectory);
        var path2 = new AbsolutePath(nonCanonicalPath);

        // Act
        var equals = path1.Equals(path2);

        // Assert
        Assert.False(equals);
    }

    [Fact]
    public void OnWindowsOrOsx_EqualsDefault_CaseInsensitive_True()
    {
        // Arrange
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        var currentDirectory = Environment.CurrentDirectory;
        var nonCanonicalPath = new string(currentDirectory.ToNonCanonicalCase().ToArray());

        var path1 = new AbsolutePath(currentDirectory);
        var path2 = new AbsolutePath(nonCanonicalPath);

        // Act
        var equals = path1.Equals(path2);

        // Assert
        Assert.True(equals);
    }

    [Theory]
    [InlineData("/path/to/file.txt", "/path/to/file.txt", 0)]
    [InlineData("/path/to/file.txt", "/PATH/TO/FILE.TXT", 1)]
    [InlineData("/PATH/TO/FILE.TXT", "/path/to/file.txt", -1)]
    [InlineData("/path/to/apple", "/path/to/banana", -1)]
    [InlineData("/path/to/banana", "/path/to/apple", 1)]
    [InlineData("/path/to/folder", "/path/to/folder/subfolder", -1)]
    [InlineData("/path/to/folder/subfolder", "/path/to/folder", 1)]
    public void PlatformDefaultAbsolutePathOrderingTest_Linux(
        string firstPathString,
        string secondPathString,
        int expected)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return;
        }

        PlatformDefaultAbsolutePathOrderingTestBase(firstPathString, secondPathString, expected);
    }

    [Theory]
    [InlineData("/path/to/file.txt", "/path/to/file.txt", 0)]
    [InlineData("/path/to/file.txt", "/PATH/TO/FILE.TXT", 0)]
    [InlineData("/PATH/TO/FILE.TXT", "/path/to/file.txt", 0)]
    [InlineData("/path/to/apple", "/path/to/banana", -1)]
    [InlineData("/path/to/banana", "/path/to/apple", 1)]
    [InlineData("/path/to/folder", "/path/to/folder/subfolder", -1)]
    [InlineData("/path/to/folder/subfolder", "/path/to/folder", 1)]
    public void PlatformDefaultAbsolutePathOrderingTest_MacOs(
        string firstPathString,
        string secondPathString,
        int expected)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        PlatformDefaultAbsolutePathOrderingTestBase(firstPathString, secondPathString, expected);
    }

    [Theory]
    [InlineData(@"C:\path\to\folder", @"C:\path\to\folder\subfolder", -1)]
    [InlineData(@"C:\path\to\folder\subfolder", @"C:\path\to\folder", 1)]
    [InlineData(@"C:\path", @"D:\path", -1)]
    [InlineData(@"D:\path", @"C:\path", 1)]
    [InlineData(@"C:\path\to\apple", @"C:\path\to\banana", -1)]
    [InlineData(@"C:\path\to\file.txt", @"C:\PATH\TO\FILE.TXT", 0)]
    [InlineData(@"C:\PATH\TO\FILE.TXT", @"C:\path\to\file.txt", 0)]
    [InlineData(@"C:\path\to\file.txt", @"C:\path\to\file.txt", 0)]
    public void PlatformDefaultAbsolutePathOrderingTest_Windows(
        string firstPathString,
        string secondPathString,
        int expected)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        PlatformDefaultAbsolutePathOrderingTestBase(firstPathString, secondPathString, expected);
    }

    private static void PlatformDefaultAbsolutePathOrderingTestBase(
        string firstPathString,
        string secondPathString,
        int expected)
    {
        // Arrange
        var firstPath = new AbsolutePath(firstPathString);
        var secondPath = new AbsolutePath(secondPathString);
        var comparer = AbsolutePath.PlatformDefaultComparer;

        // Act
        var comparisonResult = comparer.Compare(firstPath, secondPath);

        // Assert
        Assert.Equal(expected, Math.Sign(comparisonResult));
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
