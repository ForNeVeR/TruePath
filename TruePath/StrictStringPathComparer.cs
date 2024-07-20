namespace TruePath;

/// <summary>
/// Provides a strict string comparer for comparing file paths using ordinal comparison.
/// </summary>
public class StrictStringPathComparer : IComparer<string>
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="StrictStringPathComparer"/> class.
    /// </summary>
    public static readonly StrictStringPathComparer Comparer = new();

    /// <summary>
    /// Compares two strings and returns an integer that indicates their relative position in the sort order using ordinal comparison.
    /// </summary>
    /// <param name="x">The first string to compare.</param>
    /// <param name="y">The second string to compare.</param>
    /// <returns>
    /// A value less than zero if <paramref name="x"/> is less than <paramref name="y"/>; zero if <paramref name="x"/> equals <paramref name="y"/>; a value greater than zero if <paramref name="x"/> is greater than <paramref name="y"/>.
    /// </returns>
    public int Compare(string? x, string? y)
    {
        return StringComparer.Ordinal.Compare(x, y);
    }
}
