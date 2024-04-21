// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

using System.Text;

namespace TruePath;

/// <summary>Helper methods to manipulate paths as strings.</summary>
public static class PathStrings
{
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
    public static string Normalize(string path)
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
}
