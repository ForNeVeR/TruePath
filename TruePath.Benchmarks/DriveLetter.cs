// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Buffers;

namespace TruePath.Benchmarks;

public static class DriveLetter
{
    private static readonly SearchValues<char> DriveLetters = SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");

    internal const char VolumeSeparatorChar = ':';

    internal static bool UseLatinLetterRange(ReadOnlySpan<char> source)
    {
        if (source.Length < 2)
        {
            return false;
        }

        return source[1] == VolumeSeparatorChar && (uint)((source[0] | 0x20) - 'a') <= 'z' - 'a';
    }

    internal static bool UseSearchValues(ReadOnlySpan<char> source)
    {
        if (source.Length < 2) return false;

        var letter = source[0];
        var colon = source[1];

        return DriveLetters.Contains(letter) && colon == ':';
    }
}
