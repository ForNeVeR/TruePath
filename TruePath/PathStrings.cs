// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Buffers;
using System.Runtime.CompilerServices;

namespace TruePath;

/// <summary>Helper methods to manipulate paths as strings.</summary>
public static class PathStrings
{
    private static readonly SearchValues<char> DriveLetters = SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");

    /// <summary>
    /// <para>
    ///     Will convert a path string to a normalized path, using path separator specific for the current system.
    /// </para>
    /// <para>
    ///     The normalization includes:
    ///     <list type="bullet">
    ///         <item>
    ///             converting all the <see cref="Path.AltDirectorySeparatorChar"/> to
    ///             <see cref="Path.DirectorySeparatorChar"/> (e.g. <c>/</c> to <c>\</c> on Windows),
    ///         </item>
    ///         <item>
    ///             collapsing any repeated separators in the input to only one separator (e.g. <c>//</c> to just
    ///             <c>/</c> on Unix),
    ///         </item>
    ///         <item>
    ///             resolving any sequence of current and parent directory marks (subsequently, <c>.</c> and <c>..</c>)
    ///             if possible (meaning they will not be replaced if they are in the root position: paths such as
    ///             <c>.</c> or <c>../..</c> will not be affected by the normalization, while e.g. <c>foo/../.</c> will
    ///             be resolved to just <c>foo</c>).
    ///         </item>
    ///     </list>
    /// </para>
    /// <para>
    ///     Note that this operation will never perform any file IO, and is purely string manipulation.
    /// </para>
    /// </summary>
    [SkipLocalsInit] // is necessary to prevent the CLR from filling stackalloc with zeros.
    public static string Normalize(string path)
    {
        bool containsDriveLetter = SourceContainsDriveLetter(path.AsSpan());

        int written = 0;

        char[]? array = path.Length <= 512 ? null : ArrayPool<char>.Shared.Rent(path.Length);

        Span<char> normalized = array != null ? array.AsSpan() : stackalloc char[path.Length];
        ReadOnlySpan<char> source = containsDriveLetter ? path.AsSpan()[2..] : path.AsSpan();

        var buffer = normalized;

        while (true)
        {
            bool last = false;
            var separator = source.IndexOf(Path.DirectorySeparatorChar);
            var altSeparator = source.IndexOf(Path.AltDirectorySeparatorChar);

            if (altSeparator == -1 && separator == -1) { last = true; separator = source.Length - 1; }
            else if (separator == -1) separator = altSeparator;
            else if (altSeparator == -1) { }
            else separator = Math.Min(separator, altSeparator);

            separator++;
            var block = source.Slice(0, separator);

            bool skip;
            // skip if '.'
            if (block.Length == 1 && block[0] == '.')
                skip = true;
            // skip if './'
            else if (block.Length == 2 && block[0] == '.' && (block[1] == Path.DirectorySeparatorChar || block[1] == Path.AltDirectorySeparatorChar))
                skip = true;
            // cut if '..' or '../'
            else if (written != 0 && block.Length is 2 or 3 && block.StartsWith(".."))
            {
                var alreadyWrittenPart = normalized[..(written - 1)];
                var jump = alreadyWrittenPart.LastIndexOf(Path.DirectorySeparatorChar);

                // Check if the last entry in the normalized path is "..": in this case, no need to skip (we keep a
                // train of ../../.. in the normalized path's root because they are impossible to get rid of during
                // normalization).
                var lastEntryStartIndex = jump + 1;
                var lastEntry = alreadyWrittenPart[lastEntryStartIndex..];
                if (lastEntry is "..")
                {
                    skip = false;
                }
                else if (jump == -1 && written > 1)
                {
                    written = 0;
                    buffer = normalized;
                    skip = true;
                }
                else if (jump != -1)
                {
                    written = jump;
                    buffer = normalized.Slice(written + 1);
                    skip = true;
                }
                else
                    skip = false;
            }
            else
                skip = false;

            // append sliced path
            if (!skip)
            {
                block.CopyTo(buffer);
                written += separator;
                // replace \ with / if ends with \
                if (separator > 0 && buffer[separator - 1] == Path.AltDirectorySeparatorChar)
                    buffer[separator - 1] = Path.DirectorySeparatorChar;
                buffer = buffer.Slice(separator);
            }

            // skip the following / or \
            while (separator < source.Length && (source[separator] == Path.DirectorySeparatorChar || source[separator] == Path.AltDirectorySeparatorChar))
                separator++;

            // next iter
            source = source.Slice(separator);
            // append everything else if there`s no more '\' or '/'
            if (last)
            {
                source.CopyTo(buffer);
                written += source.Length;
                break;
            }
        }

        if (written == 0 && containsDriveLetter)
        {
            return new string(path.AsSpan(0, 2));
        }

        if (written == 0)
        {
            return string.Empty;
        }

        // remove / at the end of path
        if (written > 2 && normalized[written - 1] == Path.DirectorySeparatorChar)
            written--;

        // alloc new path
        string? result;

        if (containsDriveLetter)
        {
            var normalizedRef = new ReadOnlySpan<char>(normalized.ToArray(), 0, written);
            result = string.Concat(path.AsSpan(0, 2), normalizedRef.Slice(0, written));
        }
        else
        {
            result = new string(normalized.Slice(0, written));
        }

        normalized.Slice(0, written);
        if (array != null)
            ArrayPool<char>.Shared.Return(array);
        return result;
    }

    /// <summary>
    /// Determines whether the specified source contains a drive letter.
    /// </summary>
    /// <param name="source">A read-only span of characters to be checked.</param>
    /// <returns>
    ///   <c>true</c> if the source contains a drive letter (e.g., 'C:'); otherwise, <c>false</c>.
    /// </returns>
    private static bool SourceContainsDriveLetter(ReadOnlySpan<char> source)
    {
        if (source.Length < 2) return false;

        var letter = source[0];
        var colon = source[1];

        return DriveLetters.Contains(letter) && colon == ':';
    }

    /// <summary>
    /// Resolves and normalizes relative paths in the given path string by removing redundant path components.
    /// </summary>
    /// <param name="source">The path string to process.</param>
    /// <returns>
    /// The normalized path string. If the source path contains only directory separators or dots, an empty string is returned. Otherwise, the original path is returned.
    /// </returns>
    /// <remarks>
    /// This method iterates over the characters in the input path string and removes those that are directory separators or dots. If the resulting length of the path is zero, indicating that the path only contained separators or dots, it returns an empty string. Otherwise, it returns the original path unchanged.
    /// </remarks>
    internal static string ResolveRelativePaths(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return string.Empty;
        }

        var c = source.Length;

        foreach (var @char in source)
        {
            if (@char == Path.DirectorySeparatorChar || @char == '.')
            {
                c--;
            }
        }

        return c == 0 ? string.Empty : source;
    }
}
