using RuntimeInformation = System.Runtime.InteropServices.RuntimeInformation;

namespace TruePath.Tests;

public class DiskUtilsTests
{
    [Fact]
    public void DiskUtils_OnWindows_PassBackPath_ReturnCanonicalPath()
    {
        // Arrange
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

        var currentDirectory = Environment.CurrentDirectory;
        var directories = currentDirectory.Split(@"\").ToList();
        var back = Random.Shared.Next(directories.Count);
        var expected = Back(directories, back, @"\");

        var nonCanonicalPath = new string(currentDirectory) + string.Concat(Enumerable.Repeat(@"\..", back));

        // Act
        var actual = DiskUtils.GetRealPath(nonCanonicalPath);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DiskUtils_OnWindows_PassNonCanonicalPath_ReturnCanonicalPath()
    {
        // Arrange
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

        var expected = Environment.CurrentDirectory;
        var nonCanonicalPath = new string(MakeNonCanonicalPath(expected).ToArray());

        // Act
        var actual = DiskUtils.GetRealPath(nonCanonicalPath);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DiskUtils_Posix_PassBackPath_ReturnCanonicalPath()
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
