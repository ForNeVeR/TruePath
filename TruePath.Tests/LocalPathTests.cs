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
        var root = Utils.SyntheticRoot;
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
    [InlineData("C:", "C:..")]
    [InlineData("C:..", @"C:..\..")]
    [InlineData(@"C:..\..", @"C:..\..\..")]
    [InlineData("C:foo", "C:")]
    [InlineData(@"C:foo\bar", "C:foo")]
    public void DriveRelativePathParent(string path, string expected)
    {
        if (!OperatingSystem.IsWindows()) return;

        var parent = new LocalPath(path).Parent;

        Assert.Equal(expected, parent?.Value);
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
        var startsWith = b.StartsWith(a);

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
    [InlineData("/", "/frob", true)]
    [InlineData("", "foo", true)]
    [InlineData(".", "foo", true)]
    [InlineData(".", "", true)]
    [InlineData("a/..", "foo/bar", true)]
    [InlineData(".", "/foo", false)]
    [InlineData("", "/foo", false)]
    [InlineData("/", "foo", false)]
    [InlineData("/foo", "foo/bar", false)]
    [InlineData("", "../evil", false)]
    [InlineData(".", "..", false)]
    [InlineData("", "..bar", true)]
    [InlineData("\u00ADfoo", "fooo/bar", false)]
    [InlineData("\u00ADfoo", "\u00ADfoo/bar", true)]
    public void IsPrefixOfAndStartsWith(string prefix, string other, bool result)
    {
        var a = new LocalPath(prefix);
        var b = new LocalPath(other);

        Assert.Equal(result, a.IsPrefixOf(b));
        Assert.Equal(result, b.StartsWith(a));
    }

    [Fact]
    public void IsPrefixOfFollowsPlatformCaseSensitivityForSameName()
    {
        var a = new LocalPath("Foo");
        var b = new LocalPath("foo");

        Assert.Equal(Utils.IsPlatformCaseInsensitive(), a.IsPrefixOf(b));
        Assert.Equal(Utils.IsPlatformCaseInsensitive(), b.StartsWith(a));
        Assert.Equal(a == b, a.IsPrefixOf(b));
    }

    [Fact]
    public void IsPrefixOfFollowsPlatformCaseSensitivityForDescendant()
    {
        var a = new LocalPath("Foo");
        var b = new LocalPath("foo/bar");

        Assert.Equal(Utils.IsPlatformCaseInsensitive(), a.IsPrefixOf(b));
        Assert.Equal(Utils.IsPlatformCaseInsensitive(), b.StartsWith(a));
    }

    [Fact]
    public void IsPrefixOfRequiresWholeSegmentRegardlessOfCase()
    {
        var a = new LocalPath("Foo");
        var b = new LocalPath("foobar");

        Assert.False(a.IsPrefixOf(b));
        Assert.False(b.StartsWith(a));
    }

    [Fact]
    public void EmptyParentIsPrefixOfDescendantPath()
    {
        var parent = new LocalPath("foo").Parent;
        Assert.NotNull(parent);
        Assert.Equal("", parent.Value.Value);
        Assert.True(parent.Value.IsPrefixOf(new LocalPath("bar")));
    }

    [Fact]
    public void EmptyParentIsNotPrefixOfPathEscapingUpwards()
    {
        var parent = new LocalPath("foo").Parent;
        Assert.NotNull(parent);
        Assert.False(parent.Value.IsPrefixOf(new LocalPath("../evil")));
    }

    [Fact]
    public void IsPrefixOfRooted()
    {
        var root = new LocalPath(Utils.SyntheticRoot);
        var subRoot = root / "frob";
        Assert.True(root.IsPrefixOf(subRoot));
    }

    [Theory]
    [InlineData("")]
    [InlineData("foo")]
    [InlineData("foo/bar")]
    [InlineData("..")]
    public void PathRootOfRelativePathIsNull(string path)
    {
        Assert.Null(new LocalPath(path).PathRoot);
    }

    [Theory]
    [InlineData("")]
    [InlineData("foo/bar")]
    public void PathRootOfAbsolutePath(string relativePart)
    {
        var root = Utils.SyntheticRoot;
        var path = new LocalPath(root / relativePart);
        Assert.Equal(root, path.PathRoot);
    }

    [Theory]
    [InlineData(@"C:\foo", @"C:\")]
    [InlineData(@"C:\", @"C:\")]
    [InlineData("C:foo", @"C:\")]
    [InlineData(@"C:foo\bar", @"C:\")]
    [InlineData("C:", @"C:\")]
    [InlineData(@"\foo", null)]
    [InlineData(@"\", null)]
    public void PathRootOnWindows(string path, string? expectedRoot)
    {
        if (!OperatingSystem.IsWindows()) return;

        AbsolutePath? expected = expectedRoot == null ? null : new AbsolutePath(expectedRoot);
        Assert.Equal(expected, new LocalPath(path).PathRoot);
    }

    [Theory]
    [InlineData("C:foo")]
    [InlineData("C:")]
    public void PathRootOfDriveLikePathOnUnixIsNull(string path)
    {
        if (OperatingSystem.IsWindows()) return;

        Assert.Null(new LocalPath(path).PathRoot);
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
        var absolutePath = Utils.SyntheticRoot / "foo/bar";
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
    public void ResolveToCurrentDirectoryForDriveRelativePathsOnWindows()
    {
        if (!OperatingSystem.IsWindows()) return;

        var currentDirectory = AbsolutePath.CurrentWorkingDirectory;
        var drive = currentDirectory.Value.Substring(0, 2);

        Assert.Equal(new AbsolutePath(drive + @"\foo"), new LocalPath(@"\foo").ResolveToCurrentDirectory());
        Assert.Equal(currentDirectory / "foo", new LocalPath(drive + "foo").ResolveToCurrentDirectory());
        Assert.Equal(currentDirectory, new LocalPath(drive).ResolveToCurrentDirectory());
    }

    [Theory]
    [InlineData(@"C:\", PathKind.Absolute)]
    [InlineData(@"C:\Windows", PathKind.Absolute)]
    [InlineData("c:/windows/system32", PathKind.Absolute)]
    [InlineData("", PathKind.Relative)]
    [InlineData(".", PathKind.Relative)]
    [InlineData("Windows", PathKind.Relative)]
    [InlineData(@"..\Windows", PathKind.Relative)]
    [InlineData(@"Windows\System32", PathKind.Relative)]
    [InlineData("1:foo", PathKind.Relative)]
    [InlineData(@"\", PathKind.DriveRootRelative)]
    [InlineData(@"\Windows", PathKind.DriveRootRelative)]
    [InlineData("/Windows", PathKind.DriveRootRelative)]
    [InlineData("C:", PathKind.DriveCurrentDirectoryRelative)]
    [InlineData("C:Windows", PathKind.DriveCurrentDirectoryRelative)]
    [InlineData(@"c:Windows\System32", PathKind.DriveCurrentDirectoryRelative)]
    [InlineData("C:..", PathKind.DriveCurrentDirectoryRelative)]
    [InlineData("C:.", PathKind.DriveCurrentDirectoryRelative)]
    public void KindOnWindows(string path, PathKind expected)
    {
        if (!OperatingSystem.IsWindows()) return;

        var localPath = new LocalPath(path);
        Assert.Equal(expected, localPath.Kind);
        Assert.Equal(expected == PathKind.Absolute, localPath.IsAbsolute);
    }

    [Theory]
    [InlineData("/", PathKind.Absolute)]
    [InlineData("/usr/bin", PathKind.Absolute)]
    [InlineData("//usr", PathKind.Absolute)]
    [InlineData("", PathKind.Relative)]
    [InlineData("usr", PathKind.Relative)]
    [InlineData("../usr", PathKind.Relative)]
    [InlineData("C:", PathKind.Relative)]
    [InlineData("C:foo", PathKind.Relative)]
    [InlineData(@"C:\foo", PathKind.Relative)]
    [InlineData(@"\foo", PathKind.Relative)]
    public void KindOnUnix(string path, PathKind expected)
    {
        if (OperatingSystem.IsWindows()) return;

        var localPath = new LocalPath(path);
        Assert.Equal(expected, localPath.Kind);
        Assert.Equal(expected == PathKind.Absolute, localPath.IsAbsolute);
    }

    [Theory]
    [InlineData(@"\", @"\Windows", true)]
    [InlineData(@"C:\", @"\Windows", false)]
    [InlineData(@"\Windows", @"C:\Windows", false)]
    [InlineData("C:", "C:Windows", true)]
    [InlineData("c:", "C:Windows", true)]
    [InlineData("C:", "C:", true)]
    [InlineData("C:Windows", @"C:Windows\System32", true)]
    [InlineData("C:Win", "C:Windows", false)]
    [InlineData("C:", @"C:..\x", false)]
    [InlineData("C:", "D:Windows", false)]
    [InlineData("C:", @"C:\Windows", false)]
    [InlineData(@"C:\", @"C:Windows", false)]
    [InlineData("C:Windows", @"Windows\x", false)]
    [InlineData("", "C:Windows", false)]
    [InlineData("", @"\Windows", false)]
    public void IsPrefixOfAcrossKindsOnWindows(string prefix, string other, bool result)
    {
        if (!OperatingSystem.IsWindows()) return;

        var a = new LocalPath(prefix);
        var b = new LocalPath(other);

        Assert.Equal(result, a.IsPrefixOf(b));
        Assert.Equal(result, b.StartsWith(a));
    }

    [Fact]
    public void AppendMatrixOnWindows()
    {
        if (!OperatingSystem.IsWindows()) return;

        // Every base path kind combined with every appended path kind. The Expected column is the result of
        // LocalPath's operator /. It matches C++ std::filesystem::path::operator/ (as observed on MSVC 14.51), except
        // that drive letters are compared case-insensitively (the c:x cases), and that the result is normalized.
        // The PathCombine column pins the behavior of Path.Combine for the same arguments on purpose, to document
        // where the two differ.
        (string Base, string Appended, string Expected, string PathCombine)[] cases =
        [
            (@"C:\base", "x", @"C:\base\x", @"C:\base\x"),
            (@"C:\base", @"\x", @"C:\x", @"\x"),
            (@"C:\base", @"D:\x", @"D:\x", @"D:\x"),
            (@"C:\base", "D:x", "D:x", "D:x"),
            (@"C:\base", "C:x", @"C:\base\x", "C:x"),
            (@"C:\base", "c:x", @"C:\base\x", "c:x"),
            (@"C:\base", "", @"C:\base", @"C:\base"),

            ("base", "x", @"base\x", @"base\x"),
            ("base", @"\x", @"\x", @"\x"),
            ("base", @"D:\x", @"D:\x", @"D:\x"),
            ("base", "D:x", "D:x", "D:x"),
            ("base", "C:x", "C:x", "C:x"),
            ("base", "c:x", "c:x", "c:x"),
            ("base", "", "base", "base"),

            (@"\base", "x", @"\base\x", @"\base\x"),
            (@"\base", @"\x", @"\x", @"\x"),
            (@"\base", @"D:\x", @"D:\x", @"D:\x"),
            (@"\base", "D:x", "D:x", "D:x"),
            (@"\base", "C:x", "C:x", "C:x"),
            (@"\base", "c:x", "c:x", "c:x"),
            (@"\base", "", @"\base", @"\base"),

            ("C:base", "x", @"C:base\x", @"C:base\x"),
            ("C:base", @"\x", @"C:\x", @"\x"),
            ("C:base", @"D:\x", @"D:\x", @"D:\x"),
            ("C:base", "D:x", "D:x", "D:x"),
            ("C:base", "C:x", @"C:base\x", "C:x"),
            ("C:base", "c:x", @"C:base\x", "c:x"),
            ("C:base", "", "C:base", "C:base"),

            ("C:", "x", "C:x", @"C:\x"),
            ("C:", @"\x", @"C:\x", @"\x"),
            ("C:", @"D:\x", @"D:\x", @"D:\x"),
            ("C:", "D:x", "D:x", "D:x"),
            ("C:", "C:x", "C:x", "C:x"),
            ("C:", "c:x", "C:x", "c:x"),
            ("C:", "", "C:", "C:"),

            (@"C:\", "x", @"C:\x", @"C:\x"),
            (@"C:\", @"\x", @"C:\x", @"\x"),
            (@"C:\", @"D:\x", @"D:\x", @"D:\x"),
            (@"C:\", "D:x", "D:x", "D:x"),
            (@"C:\", "C:x", @"C:\x", "C:x"),
            (@"C:\", "c:x", @"C:\x", "c:x"),
            (@"C:\", "", @"C:\", @"C:\"),

            (@"\", "x", @"\x", @"\x"),
            (@"\", @"\x", @"\x", @"\x"),
            (@"\", @"D:\x", @"D:\x", @"D:\x"),
            (@"\", "D:x", "D:x", "D:x"),
            (@"\", "C:x", "C:x", "C:x"),
            (@"\", "c:x", "c:x", "c:x"),
            (@"\", "", @"\", @"\"),

            ("", "x", "x", "x"),
            ("", @"\x", @"\x", @"\x"),
            ("", @"D:\x", @"D:\x", @"D:\x"),
            ("", "D:x", "D:x", "D:x"),
            ("", "C:x", "C:x", "C:x"),
            ("", "c:x", "c:x", "c:x"),
            ("", "", "", ""),
        ];

        var failures = new List<string>();
        foreach (var (basePath, appended, expected, expectedPathCombine) in cases)
        {
            var actual = (new LocalPath(basePath) / appended).Value;
            if (actual != expected)
                failures.Add($"\"{basePath}\" / \"{appended}\": expected \"{expected}\", got \"{actual}\".");

            var actualPathCombine = Path.Combine(basePath, appended);
            if (actualPathCombine != expectedPathCombine)
                failures.Add(
                    $"Path.Combine(\"{basePath}\", \"{appended}\"): expected \"{expectedPathCombine}\", got \"{actualPathCombine}\".");
        }

        Assert.Empty(failures);
    }

    [Fact]
    public void AppendMatrixOnUnix()
    {
        if (OperatingSystem.IsWindows()) return;

        // On Unix, operator / always agrees with Path.Combine, up to normalization.
        (string Base, string Appended, string Expected)[] cases =
        [
            ("/base", "x", "/base/x"),
            ("/base", "/x", "/x"),
            ("/base", @"\x", @"/base/\x"),
            ("/base", "", "/base"),
            ("/", "x", "/x"),
            ("base", "C:x", "base/C:x"),
            ("C:", "x", "C:/x"),
            ("", "x", "x"),
        ];

        var failures = new List<string>();
        foreach (var (basePath, appended, expected) in cases)
        {
            var actual = (new LocalPath(basePath) / appended).Value;
            if (actual != expected)
                failures.Add($"\"{basePath}\" / \"{appended}\": expected \"{expected}\", got \"{actual}\".");

            var actualPathCombine = new LocalPath(Path.Combine(basePath, appended)).Value;
            if (actualPathCombine != expected)
                failures.Add(
                    $"Path.Combine(\"{basePath}\", \"{appended}\"): expected \"{expected}\", got \"{actualPathCombine}\".");
        }

        Assert.Empty(failures);
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
