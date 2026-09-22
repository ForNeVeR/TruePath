// SPDX-FileCopyrightText: 2024-2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Buffers;
using System.Runtime.InteropServices;
#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace TruePath;

/// <summary>Helper methods to manipulate paths as strings.</summary>
public static class PathStrings
{
    private const char VolumeSeparatorChar = ':';

    /// <summary>
    /// <para>
    /// Will convert a path string to a normalized path, using path separator specific for the current system.
    /// </para>
    /// <para>
    /// The normalization includes:
    /// <list type="bullet">
    ///     <item>
    ///         converting all the <see cref="Path.AltDirectorySeparatorChar"/> to
    ///         <see cref="Path.DirectorySeparatorChar"/> (e.g. <c>/</c> to <c>\</c> on Windows),
    ///     </item>
    ///     <item>
    ///         collapsing any repeated separators in the input to only one separator (e.g. <c>//</c> to just
    ///         <c>/</c> on Unix),
    ///     </item>
    ///     <item>
    ///         resolving any sequence of current and parent directory references (subsequently, <c>.</c> and
    ///         <c>..</c>) if possible (e.g. <c>foo/../.</c> is resolved to just <c>foo</c>). A path that resolves
    ///         to the current directory is normalized to an <b>empty</b> path: both <c>.</c> and <c>a/..</c>
    ///         become <c>&quot;&quot;</c>. Parent directory references that cannot be resolved are preserved,
    ///         since there is nothing above them to fold into: <c>..</c> and <c>../..</c> are not affected by the
    ///         normalization,
    ///     </item>
    ///     <item>
    ///         trimming <b>all</b> trailing separators (e.g. <c>a/b/c/d////</c> is normalized to
    ///         <c>a/b/c/d</c>), except for the case with root folder: <c>X:\</c> (Windows) or <c>/</c> (Unix)
    ///         doesn't get trimmed.
    ///     </item>
    /// </list>
    /// </para>
    /// <para>
    /// Note that this operation will never perform any file IO, and is purely string manipulation.
    /// </para>
    /// </summary>
    public static string Normalize(string path) =>
        Normalize(path, driveBasedSystem: RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

#if NET8_0_OR_GREATER
    [SkipLocalsInit] // is necessary to prevent the CLR from filling stackalloc with zeros.
#endif
    internal static string Normalize(string path, bool driveBasedSystem)
    {
        bool containsDriveLetter = driveBasedSystem && SourceContainsDriveLetter(path.AsSpan());

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
            else if (written != 0
                && (
                    block is ".."
                    || block.SequenceEqual($"..{Path.DirectorySeparatorChar}".AsSpan())
                    || block.SequenceEqual($"..{Path.AltDirectorySeparatorChar}".AsSpan())
                ))
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
                    written = last ? jump : jump + 1;
                    buffer = normalized[written..];
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
            return path[..2];
        }

        if (written == 0)
        {
            return string.Empty;
        }

        // remove / at the end of path
        if (written > 1 && normalized[written - 1] == Path.DirectorySeparatorChar)
            written--;

        // alloc new path
        string? result;

        if (containsDriveLetter)
        {
            var normalizedArr = normalized[..written].ToArray();
            result = new string(path.AsSpan(0, 2).ToArray()) + new string(normalizedArr);
        }
        else
        {
            result = new string(normalized[..written].ToArray());
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
        if (source.Length < 2)
        {
            return false;
        }

        return source[1] == VolumeSeparatorChar && (uint)((source[0] | 0x20) - 'a') <= 'z' - 'a';
    }
}
