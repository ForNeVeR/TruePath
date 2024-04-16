// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

using System.Text;

namespace TruePath;

internal static class PathStrings
{
    public static string Normalize(string path)
    {
        // TODO[36]: Optimize this. It is possible to do with less allocations.
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
