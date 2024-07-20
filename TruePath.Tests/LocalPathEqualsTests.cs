namespace TruePath.Tests;

public partial class LocalPathTests
{
    [Fact]
    public void EqualsUseStrictStringPathComparer_SamePaths_True()
    {
        // Arrange
        var currentDirectory = Environment.CurrentDirectory;
        var nonCanonicalPath = currentDirectory;

        var path1 = new LocalPath(currentDirectory);
        var path2 = new LocalPath(nonCanonicalPath);

        // Act
        var equals = path1.Equals(path2, StrictStringPathComparer.Comparer);

        // Assert
        Assert.True(equals);
    }

    [Fact]
    public void EqualsUseStrictStringPathComparer_NotSamePaths_False()
    {
        // Arrange
        var currentDirectory = Environment.CurrentDirectory;
        var nonCanonicalPath = new string(currentDirectory.MakeNonCanonicalPath().ToArray());

        var path1 = new LocalPath(currentDirectory);
        var path2 = new LocalPath(nonCanonicalPath);

        // Act
        var equals = path1.Equals(path2, StrictStringPathComparer.Comparer);

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
        var nonCanonicalPath = new string(currentDirectory.MakeNonCanonicalPath().ToArray());

        var path1 = new LocalPath(currentDirectory);
        var path2 = new LocalPath(nonCanonicalPath);

        // Act
        var equals = path1.Equals(path2);

        // Assert
        Assert.False(equals);
    }

    [Fact]
    public void EqualsNull_False()
    {
        // Arrange
        var currentDirectory = Environment.CurrentDirectory;
        var nonCanonicalPath = currentDirectory;

        var path1 = new LocalPath(currentDirectory);
        var path2 = new LocalPath(nonCanonicalPath);

        // Act
        var equals = path1.Equals(path2, null);

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
        var nonCanonicalPath = new string(currentDirectory.MakeNonCanonicalPath().ToArray());

        var path1 = new LocalPath(currentDirectory);
        var path2 = new LocalPath(nonCanonicalPath);

        // Act
        var equals = path1.Equals(path2);

        // Assert
        Assert.True(equals);
    }
}
