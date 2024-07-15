namespace TruePath.Tests;

public class DiskUtilsTests
{
    [Fact]
    public void DiskUtils_OnWindows_PassPath_ReturnCanonicalPath()
    {
        // Arrange
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

        var expected = Environment.CurrentDirectory;
        var nonCanonicalPath = new string(MakeNonCanonicalPath(expected).ToArray());

        // Act
        var actual = DiskUtils.GetWindowsRealPathByPath(nonCanonicalPath);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DiskUtils_NonWindows_PassPath_ReturnCanonicalPath()
    {
        // Arrange
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

        var expected = Environment.CurrentDirectory;
        var nonCanonicalPath = new string(MakeNonCanonicalPath(expected).ToArray());

        // Act
        var actual = DiskUtils.GetPosixRealPath(nonCanonicalPath);

        // Assert
        Assert.Equal(expected, actual);
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
