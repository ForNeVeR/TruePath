using System.Runtime.InteropServices;

namespace TruePath;

/// <summary>
/// Provides a platform-specific string comparer for comparing file paths.
/// </summary>
public class PlatformDefaultPathComparer : IComparer<string>
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="PlatformDefaultPathComparer"/> class.
    /// </summary>
    public static readonly PlatformDefaultPathComparer Comparer = new();

    private readonly StringComparer comparisonType;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformDefaultPathComparer"/> class.
    /// </summary>
    public PlatformDefaultPathComparer()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            comparisonType = StringComparer.OrdinalIgnoreCase;
        }
        else
        {
            comparisonType = StringComparer.Ordinal;
        }
    }

    /// <summary>
    /// Compares two strings and returns an integer that indicates their relative position in the sort order.
    /// </summary>
    /// <param name="x">The first string to compare.</param>
    /// <param name="y">The second string to compare.</param>
    /// <returns>
    /// A value less than zero if <paramref name="x"/> is less than <paramref name="y"/>; zero if <paramref name="x"/> equals <paramref name="y"/>; a value greater than zero if <paramref name="x"/> is greater than <paramref name="y"/>.
    /// </returns>
    public int Compare(string? x, string? y)
    {
        return comparisonType.Compare(x, y);
    }
}
