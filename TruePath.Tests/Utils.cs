// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath.Tests;

public static class Utils
{
    internal static string MakeNonCanonicalPath(this string path)
    {
        var result = new char[path.Length];
        for (var i = 0; i < path.Length; i++)
        {
            result[i] = i % 2 == 0 ? char.ToUpper(path[i]) : char.ToLower(path[i]);
        }

        var nonCanonicalPath = new string(result);
        if (path.Equals(nonCanonicalPath, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The non-canonical path is equal to the original path.");
        }

        return nonCanonicalPath;
    }
}
