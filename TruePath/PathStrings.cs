// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

using System.Text;

namespace TruePath;

internal static class PathStrings
{
    public static string Normalize(string path)
    {
        var buffer = new StringBuilder(path.Length);
        var isSeparator = false;
        foreach (var c in path)
        {
            if (c == Path.AltDirectorySeparatorChar || c == Path.DirectorySeparatorChar)
            {
                if (isSeparator) continue; // deduplicate separators
                buffer.Append(Path.DirectorySeparatorChar);
                isSeparator = true;
            }
            else
            {
                if (isSeparator)
                {
                    // Leave separator mode and enter
                }
                buffer.Append(c);
                isSeparator = false;
            }
        }

        var lastNonSeparatorIndex = buffer.Length;
        for (; lastNonSeparatorIndex-- > 0 && buffer[lastNonSeparatorIndex] == Path.DirectorySeparatorChar;){}
        buffer.Length = lastNonSeparatorIndex + 1;

        var dotCount = 0;
        var itemsToEat = 0;
        for (var i = buffer.Length; i-- > 0;)
        {
            var c = buffer[i];
            if (c == '.')
            {
                ++dotCount;
            }
            else if (c == Path.DirectorySeparatorChar)
            {
                if (dotCount == 1)
                {
                    buffer.Remove(i, 2);
                }
                else if (dotCount == 2)
                {
                    buffer.Remove(i, 3);
                    dotCount = 0;
                    ++itemsToEat;
                }
                else
                {
                    dotCount = 0;
                    if (itemsToEat <= 0) continue;
                    var indexOfPrevSlash = buffer.LastIndexOf(Path.DirectorySeparatorChar);
                    if (indexOfPrevSlash is { } index)
                    {
                        buffer.Remove(index, buffer.Length - index);
                        --itemsToEat;
                    }
                }
            }
            else
            {
                dotCount = 0;
            }
        }

        for (var i = 0; i < itemsToEat; ++i)
        {
            if (buffer.Length == 0) buffer.Append("..");
            else buffer.Append(Path.DirectorySeparatorChar).Append("..");
        }

        return buffer.ToString();
    }

    private static int? LastIndexOf(this StringBuilder buffer, char c)
    {
        for (var i = buffer.Length; i-- > 0;)
        {
            if (buffer[i] == c)
            {
                return i;
            }
        }
        return null;
    }
}
