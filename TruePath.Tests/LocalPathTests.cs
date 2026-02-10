// SPDX-FileCopyrightText: 2024-2026 TruePath contributors <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

namespace TruePath.Tests;

public class LocalPathTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData("foo", ".")]
    [InlineData("foo/bar", "foo")]
    [InlineData("/", null)]
    public void AbsolutePathParent(string relativePath, string? expectedRelativePath)
    {
        var root = new AbsolutePath(OperatingSystem.IsWindows() ? @"A:\" : "/");
        var parent = root / relativePath;
        AbsolutePath? expectedPath = expectedRelativePath == null ? null : new(root / expectedRelativePath);
        Assert.Equal(expectedPath, parent.Parent);
    }

    [Theory]
    [InlineData(".", "")]
    [InlineData("..", "..")]
    [InlineData("../..", "../..")]
    [InlineData(".../...", ".../...")]
    [InlineData(".../..", "")]
    public void ConstructionTest(string pathString, string expectedValue)
    {
        var path = new LocalPath(pathString);
        Assert.Equal(expectedValue.Replace('/', Path.DirectorySeparatorChar), path.Value);
    }

    [Theory]
    [InlineData(".", "..")]
    [InlineData("..", "../..")]
    [InlineData("../..", "../../..")]
    [InlineData("../../", "../../..")]
    [InlineData("../...", "..")]
    [InlineData(".../..", "..")]
    [InlineData("./.", "..")]
    [InlineData("../../.", "../../..")]
    [InlineData("b", ".")]
    [InlineData("../b", "..")]
    [InlineData("b/..", "b/../..")]
    [InlineData("...", ".../..")]
    [InlineData(".../...", "...")]
    public void RelativePathParent(string path, string? expected)
    {
        // Arrange
        var localPath = new LocalPath(path);
        LocalPath? expectedPath = expected == null ? null : new(expected);

        // Act
        var parent = localPath.Parent;

        // Assert
        Assert.Equal(expectedPath, parent);
    }

    [Theory]
    [InlineData("user", "user/documents")]
    [InlineData("usEr", "User/documents")]
    [InlineData("user/documents", "user/documents")]
    [InlineData("user/documents", "user")]
    public void IsPrefixOfShouldBeEquivalentToStartsWith(string pathA, string pathB)
    {
        // Arrange
        var a = new LocalPath(pathA);
        var b = new LocalPath(pathB);

        // Act
        var isPrefix = a.IsPrefixOf(b);
        var startsWith = b.Value.StartsWith(a.Value);

        // Assert
        Assert.Equal(isPrefix, startsWith);
    }

    [Fact]
    public void AbsolutePathIsNormalizedOnCreation()
    {
        if (!OperatingSystem.IsWindows()) return;

        const string path = @"C:/Users/John Doe\Documents";
        var absolutePath = new LocalPath(path);
        Assert.Equal(@"C:\Users\John Doe\Documents", absolutePath.Value);
    }

    [Theory]
    [InlineData("/foo/bar", "/foo", false)]
    [InlineData("/foo/", "/foo/bar/", true)]
    [InlineData("/foo", "/foo1/bar/", false)]
    [InlineData("/foo", "/foo1", false)]
    [InlineData("/foo", "/foo", true)]
    public void IsPrefixOf(string prefix, string other, bool result)
    {
        Assert.Equal(result, new LocalPath(prefix).IsPrefixOf(new LocalPath(other)));
    }

    [Fact]
    public void RelativePathIsNormalizedOnCreation()
    {
        if (!OperatingSystem.IsWindows()) return;

        const string path = @"Users/John Doe\Documents";
        var relativePath = new LocalPath(path);
        Assert.Equal(@"Users\John Doe\Documents", relativePath.Value);
    }

    [Fact]
    public void LocalPathConvertedFromAbsolute()
    {
        var absolutePath = new AbsolutePath("/foo/bar");
        LocalPath localPath1 = absolutePath;
        var localPath2 = new LocalPath(absolutePath);

        Assert.Equal(localPath1, localPath2);
    }

    [Fact]
    public void ResolveToCurrentDirectoryTests()
    {
        var localPath = new LocalPath("foo/bar");
        var currentDirectory = AbsolutePath.CurrentWorkingDirectory;
        var expected = currentDirectory / localPath;
        Assert.Equal(expected, localPath.ResolveToCurrentDirectory());

        try
        {
            var newCurrentDirectory = new AbsolutePath(Path.GetTempPath()).Canonicalize();
            output.WriteLine("New current directory: " + newCurrentDirectory);
            Environment.CurrentDirectory = newCurrentDirectory.Value;
            expected = newCurrentDirectory / localPath;
            Assert.Equal(expected, localPath.ResolveToCurrentDirectory());
        }
        finally
        {
            Environment.CurrentDirectory = currentDirectory.Value;
            output.WriteLine("Current directory reset back to: " + currentDirectory);
        }
    }

    [Fact]
    public void PlatformDefaultPathComparerTest()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

        var path1 = new LocalPath(@"C:\Windows");
        var path2 = new LocalPath(@"C:\WINDOWS");

        Assert.True(path1.Equals(path2, LocalPath.PlatformDefaultComparer));
    }

    [Fact]
    public void EqualsUseStrictStringPathComparer_SamePaths_True()
    {
        // Arrange
        var currentDirectory = Environment.CurrentDirectory;
        var nonCanonicalPath = currentDirectory;

        var path1 = new LocalPath(currentDirectory);
        var path2 = new LocalPath(nonCanonicalPath);

        // Act
        var equals = path1.Equals(path2, LocalPath.StrictStringComparer);

        // Assert
        Assert.True(equals);
    }

    [Fact]
    public void EqualsUseStrictStringPathComparer_NotSamePaths_False()
    {
        // Arrange
        var currentDirectory = Environment.CurrentDirectory;
        var nonCanonicalPath = new string(currentDirectory.ToNonCanonicalCase().ToArray());

        var path1 = new LocalPath(currentDirectory);
        var path2 = new LocalPath(nonCanonicalPath);

        // Act
        var equals = path1.Equals(path2, LocalPath.StrictStringComparer);

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

        var path1 = new LocalPath(currentDirectory);
        var path2 = new LocalPath(nonCanonicalPath);

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

        var path1 = new LocalPath(currentDirectory);
        var path2 = new LocalPath(nonCanonicalPath);

        // Act
        var equals = path1.Equals(path2);

        // Assert
        Assert.True(equals);
    }

    [Theory]
    [InlineData("/path/to/file.txt", "/path/to/file.txt", 0)]
    [InlineData("/path/to/file.txt", "/PATH/TO/FILE.TXT", 1)]
    [InlineData("/PATH/TO/FILE.TXT", "/path/to/file.txt", -1)]
    [InlineData("path/to/apple", "path/to/banana", -1)]
    [InlineData("path/to/banana", "path/to/apple", 1)]
    [InlineData("path/to/folder", "path/to/folder/subfolder", -1)]
    [InlineData("path/to/folder/subfolder", "path/to/folder", 1)]
    public void PlatformDefaultLocalPathOrderingTest_Linux(
        string firstPathString,
        string secondPathString,
        int expected)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return;
        }

        PlatformDefaultLocalPathOrderingTestBase(firstPathString, secondPathString, expected);
    }

    [Theory]
    [InlineData("/path/to/file.txt", "/path/to/file.txt", 0)]
    [InlineData("/path/to/file.txt", "/PATH/TO/FILE.TXT", 0)]
    [InlineData("/PATH/TO/FILE.TXT", "/path/to/file.txt", 0)]
    [InlineData("path/to/apple", "path/to/banana", -1)]
    [InlineData("path/to/banana", "path/to/apple", 1)]
    [InlineData("path/to/folder", "path/to/folder/subfolder", -1)]
    [InlineData("path/to/folder/subfolder", "path/to/folder", 1)]
    public void PlatformDefaultLocalPathOrderingTest_MacOs(
        string firstPathString,
        string secondPathString,
        int expected)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        PlatformDefaultLocalPathOrderingTestBase(firstPathString, secondPathString, expected);
    }

    [Theory]
    [InlineData(@"C:\path\to\folder", @"C:\path\to\folder\subfolder", -1)]
    [InlineData(@"C:\path\to\folder\subfolder", @"C:\path\to\folder", 1)]
    [InlineData(@"C:\path", @"D:\path", -1)]
    [InlineData(@"D:\path", @"C:\path", 1)]
    [InlineData(@"C:\path\to\apple", @"C:\path\to\banana", -1)]
    [InlineData(@"C:\path\to\banana", @"C:\path\to\apple", 1)]
    [InlineData(@"C:\path\to\file.txt", @"C:\PATH\TO\FILE.TXT", 0)]
    [InlineData(@"C:\PATH\TO\FILE.TXT", @"C:\path\to\file.txt", 0)]
    [InlineData(@"\path\to\file.txt", @"\path\to\file.txt", 0)]
    [InlineData(@"\path\to\file.txt", @"\PATH\TO\FILE.TXT", 0)]
    public void PlatformDefaultLocalPathOrderingTest_Windows(
        string firstPathString,
        string secondPathString,
        int expected)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        PlatformDefaultLocalPathOrderingTestBase(firstPathString, secondPathString, expected);
    }

    private static void PlatformDefaultLocalPathOrderingTestBase(
        string firstPathString,
        string secondPathString,
        int expected)
    {
        // Arrange
        var firstPath = new LocalPath(firstPathString);
        var secondPath = new LocalPath(secondPathString);
        var comparer = LocalPath.PlatformDefaultComparer;

        // Act
        var comparisonResult = comparer.Compare(firstPath, secondPath);

        // Assert
        Assert.Equal(expected, Math.Sign(comparisonResult));
    }
}
