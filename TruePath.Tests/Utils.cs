namespace TruePath.Tests;

public static class Utils
{
    internal static IEnumerable<char> MakeNonCanonicalPath(this string path)
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
