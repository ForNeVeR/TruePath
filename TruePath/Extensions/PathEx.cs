#if !NET8_0_OR_GREATER
// ReSharper disable once CheckNamespace
namespace System.IO;

/// <summary>
/// Class that contains custom implementations methods of <see cref="Path"/> class presented in .NET 8 but missing in .NET Standard 2.0.
/// </summary>
internal static class PathEx
{
    public static string GetRelativePath(string relativeTo, string path)
    {
        if (string.IsNullOrEmpty(relativeTo)) throw new ArgumentNullException(nameof(relativeTo));
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

        var relativeUri = new Uri(Path.GetFullPath(relativeTo) + Path.DirectorySeparatorChar);
        var pathUri = new Uri(Path.GetFullPath(path));

        if (relativeUri.Scheme != pathUri.Scheme)
            return path;

        var relativePathUri = relativeUri.MakeRelativeUri(pathUri);
        var relativePath = Uri.UnescapeDataString(relativePathUri.ToString());

        return relativePath.Replace('/', Path.DirectorySeparatorChar);
    }
}
#endif
