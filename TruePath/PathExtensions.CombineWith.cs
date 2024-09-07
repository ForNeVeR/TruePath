namespace TruePath;

public static partial class PathExtensions
{
    public static IPath CombineWith(this IPath path, IPath other)
    {
        var combined = Path.Combine(path.Value, other.Value);

        return Path.IsPathFullyQualified(combined) ? new AbsolutePath(combined) : new LocalPath(combined);
    }

    public static IPath CombineWith(this IPath path, string other)
    {
        var combined = Path.Combine(path.Value, other);

        return Path.IsPathFullyQualified(combined) ? new AbsolutePath(combined) : new LocalPath(combined);
    }

    public static IPath CombineWith(this string path, string other)
    {
        var combined = Path.Combine(path, other);

        return Path.IsPathFullyQualified(combined) ? new AbsolutePath(combined) : new LocalPath(combined);
    }

    public static IPath CombineWith(this string path, IPath other)
    {
        var combined = Path.Combine(path, other.Value);

        return Path.IsPathFullyQualified(combined) ? new AbsolutePath(combined) : new LocalPath(combined);
    }
}
