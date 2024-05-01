using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TruePath;
/// <summary>
/// Extension methods for <see cref="IPath"/> and <see cref="IPath{TPath}"/>.
/// </summary>
public static class PathExtensions
{
    /// <summary>
    /// Gets the extension of the file name of the path.
    /// </summary>
    /// <typeparam name="TPath"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetExtension(this IPath path)
    {
        var fileExtenstion = Path.GetExtension(path.FileName);
        if (string.IsNullOrEmpty(fileExtenstion))
        {
            throw new ArgumentException(string.Format("{0} is not a file", path.FileName));
        }
        return fileExtenstion;
    }
    /// <summary>
    /// Gets the extension of the file name of the path without the dot.
    /// </summary>
    /// <typeparam name="TPath"></typeparam>
    /// <param name="path">
    /// The path to get the extension from.
    /// </param>
    /// <returns>The extension of the file name of the path without the dot.</returns>
    public static string GetExtensionWithoutDot<TPath>(this TPath path) where TPath : IPath
    {
        return GetExtension(path).TrimStart('.');
    }

    /// <summary>
    /// Gets the kind of the file system entry.
    /// </summary>
    /// <typeparam name="TPath"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public static FileEntryKind GetKind<TPath>(this TPath path) where TPath : IPath
    {
        // Check if the path exists
        if (!File.Exists(path.Value) && !Directory.Exists(path.Value))
        {
            return FileEntryKind.Unknown;
        }
        //
        var attributes = File.GetAttributes(path.Value);
        if (attributes.HasFlag(FileAttributes.Directory))
        {
            // Check for reparse point, which is used by symlinks and junctions
            if (attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                // TODO: define if it is symlinks and junctions
                return FileEntryKind.Unknown;
            }
            return FileEntryKind.Directory;
        }

        return FileEntryKind.File;
    }
}
