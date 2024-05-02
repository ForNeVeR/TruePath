// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace TruePath.Benchmarks;

/// <summary>Helper methods to manipulate paths as strings.</summary>
public static class PathStrings
{
    public static string Normalize1(string path)
    {
        // TODO[#19]: Optimize this. It is possible to do with less allocations.
        var segments = new List<(int Start, int End)>();

        int? currentSegmentStart = 0;
        for (var i = 0; i < path.Length; i++)
        {
            if (path[i] == Path.DirectorySeparatorChar || path[i] == Path.AltDirectorySeparatorChar)
            {
                if (currentSegmentStart is { } s)
                {
                    segments.Add((s, i));
                    currentSegmentStart = null;
                }
            }
            else
            {
                currentSegmentStart ??= i;
            }
        }

        if (currentSegmentStart is { } start)
            segments.Add((start, path.Length));

        var resultSegments = new LinkedList<(int Start, int End)>();
        foreach (var segment in segments)
        {
            var text = path.AsSpan()[segment.Start..segment.End];
            switch (text)
            {
                case ".":
                    continue;
                case ".." when resultSegments.Count > 0
                               // check for the root segment (empty)
                               && resultSegments.Last!.Value.Start != resultSegments.Last.Value.End:
                    resultSegments.RemoveLast();
                    break;
                default:
                    resultSegments.AddLast(segment);
                    break;
            }
        }

        var buffer = new StringBuilder();
        var index = 0;
        foreach (var segment in resultSegments)
        {
            buffer.Append(path, segment.Start, segment.End - segment.Start);
            if (++index < resultSegments.Count
                // check for the root segment: in such case, we still want to add the separator
                || segment.Start == segment.End)
                buffer.Append(Path.DirectorySeparatorChar);
        }

        return buffer.ToString();
    }

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
    public static string Normalize2(string path)
    {
        int written = 0;

        char[]? array = path.Length < (IntPtr.Size == 4 ? 512 : 4096) ? null : ArrayPool<char>.Shared.Rent(path.Length);

        Span<char> normalized = array != null ? array.AsSpan() : stackalloc char[path.Length];
        ReadOnlySpan<char> source = path.AsSpan();

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
                var jump = normalized.Slice(0, written - 1).LastIndexOf(Path.DirectorySeparatorChar);

                if (jump == -1 && written > 1)
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

        if (array != null)
            ArrayPool<char>.Shared.Return(array);

        // why create an empty string when you can reuse it
        if (written == 0)
            return string.Empty;

        // remove / at the end of path
        if (written > 2 && normalized[written - 1] == Path.DirectorySeparatorChar)
            written--;

        // alloc new path
        return new string(normalized.Slice(0, written));
    }

    public static string Normalize3(string path)
    {
        int written = 0;

        var array = ArrayPool<char>.Shared.Rent(path.Length);
        Span<char> normalized = array;
        ReadOnlySpan<char> source = path.AsSpan();

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
                var jump = normalized.Slice(0, written - 1).LastIndexOf(Path.DirectorySeparatorChar);

                if (jump == -1 && written > 1)
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


        // why create an empty string when you can reuse it
        if (written == 0)
            return string.Empty;

        // remove / at the end of path
        if (written > 2 && normalized[written - 1] == Path.DirectorySeparatorChar)
            written--;

        // alloc new path
        var result = new string(normalized.Slice(0, written));
        ArrayPool<char>.Shared.Return(array);
        return result;
    }

    public static string Normalize4(string path)
    {
        int written = 0;

        var array = ArrayPool<char>.Shared.Rent(path.Length);
        Span<char> normalized = array;
        ReadOnlySpan<char> source = path.AsSpan();

        var buffer = normalized;

        while (true)
        {
            bool last = false;
            var sv = SearchValues.Create([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]);
            var separator = source.IndexOfAny(sv);

            if (separator == -1) { last = true; separator = source.Length - 1; }

            separator++;
            var block = source.Slice(0, separator);

            bool skip;
            // skip if '.'
            if (block.Length == 1 && block[0] == '.')
                skip = true;
            // skip if './'
            else if (block.Length == 2 && block[0] == '.' && sv.Contains(block[1]))
                skip = true;
            // cut if '..' or '../'
            else if (written != 0 && block.Length is 2 or 3 && block.StartsWith(".."))
            {
                var jump = normalized.Slice(0, written - 1).LastIndexOfAny(sv);

                if (jump == -1 && written > 1)
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
            while (separator < source.Length && sv.Contains(source[separator]))
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

        // why create an empty string when you can reuse it
        if (written == 0)
            return string.Empty;

        // remove / at the end of path
        if (written > 2 && normalized[written - 1] == Path.DirectorySeparatorChar)
            written--;

        // alloc new path
        var result = new string(normalized.Slice(0, written));
        ArrayPool<char>.Shared.Return(array);
        return result;
    }
}
