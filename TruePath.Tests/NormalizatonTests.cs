// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath.Tests;

using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using System.Linq;
using Xunit.Abstractions;

public class NormalizatonTests
{
    private readonly ITestOutputHelper _TestOutputHelper;
    public NormalizatonTests(ITestOutputHelper testOutputHelper)
    {
        _TestOutputHelper = testOutputHelper;
    }

    [Property(Arbitrary = new[] { typeof(AnyOsPath) })]
    public void NormalizedPathDoesNotEndWithDirSeparator(List<string> pathParts)
    {
        var sourcePath = string.Join("", pathParts);

        // Act
        var normalizedPath = PathStrings.Normalize(sourcePath);

        _TestOutputHelper.WriteLine($"{sourcePath} => {normalizedPath}");
        Assert.True(normalizedPath == ""
            || normalizedPath == Path.DirectorySeparatorChar.ToString()
            || (normalizedPath.Length == 3 && normalizedPath[1] == ':')
            || normalizedPath[^1] != Path.DirectorySeparatorChar);
    }

    [Property(Arbitrary = new[] { typeof(AnyOsPath) })]
    public void DepthPreserverd(List<string> pathParts)
    {
        var sourcePath = string.Join("", pathParts);

        // Act
        var normalizedPath = PathStrings.Normalize(sourcePath);

        _TestOutputHelper.WriteLine($"{sourcePath} => {normalizedPath}");
        var collapsedBlock = CollapseSameBlocks(pathParts);
        var expectedDepth = CountDepth(collapsedBlock);
        var actualDepth = CountDepth([.. normalizedPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries)]);
        if (actualDepth != expectedDepth)
        {
            _TestOutputHelper.WriteLine($"{string.Join("|", collapsedBlock)}");
        }
        Assert.Equal(expectedDepth, actualDepth);

        static int CountDepth(List<string> pathParts)
        {
            int depth = 0;
            foreach (var part in pathParts)
            {
                if (part == Path.DirectorySeparatorChar.ToString() || part == Path.AltDirectorySeparatorChar.ToString())
                {
                    continue;
                }

                // Skip home drive which does not affect depth.
                if (part.Contains(':'))
                {
                    continue;
                }

                if (part == "..")
                {
                    if (depth > 0)
                    {
                        depth--;
                    }
                }
                else if (part != ".")
                {
                    depth++;
                }
            }
            return depth;
        }
    }

    private static List<string> CollapseSameBlocks(IEnumerable<string> pathParts)
    {
        var result = new List<string>();
        var lastSeparator = false;
        var hasHomeDrive = pathParts.Any(part => part.Length > 1 && part[1] == ':');
        string? homeDrive = null;
        foreach (var item in pathParts)
        {
            if (homeDrive is null)
            {
                homeDrive = item;
            }

            if (item.Length > 1 && item[1] == ':') continue;

            var currentSeparator = item == Path.DirectorySeparatorChar.ToString() || item == Path.AltDirectorySeparatorChar.ToString();
            if (lastSeparator && currentSeparator)
            {
                // Skip consecutive separators
                continue;
            }

            if (lastSeparator || currentSeparator)
            {
                result.Add(item);
            }
            else
            {
                if (result.Count > 0)
                {
                    result[^1] += item;
                }
                else
                {
                    result.Add(item);
                }
            }

            lastSeparator = currentSeparator;

        }

        if (hasHomeDrive && homeDrive is { })
        {
            result.Insert(0, homeDrive);
        }

        return result;
    }
}

internal static class PathGenerators
{
    public static Gen<List<string>> LinuxPathItemsGenerator()
    {
        var baseDir = Gen.Constant("..");
        var currentDir = Gen.Constant(".");
        var threeDots = Gen.Constant("...");
        var dots = Gen.OneOf([baseDir, currentDir, threeDots]);
        var textPath = Gen.NonEmptyListOf(Gen.Choose('a', 'z').Or(Gen.Choose('A', 'Z')).Or(Gen.Choose('0', '9'))).Select(l => string.Join("", l.Select(c => (char)c)));

        var directorySeparatorChar = Gen.Constant(Path.DirectorySeparatorChar).Select(c => c.ToString());
        var altDirectorySeparatorChar = Gen.Constant(Path.AltDirectorySeparatorChar).Select(c => c.ToString());
        return Gen.NonEmptyListOf(Gen.OneOf([dots, directorySeparatorChar, altDirectorySeparatorChar, textPath]))
            .Where(l =>
            {
                for (int i = 0; i < l.Count - 1; i++)
                {
                    // Avoid consecutive dots like "..../.."
                    if (l[i][0] == '.' && l[i + 1][0] == '.') return false;
                }

                return true;
            });
    }

    public static Gen<List<string>> WindowsPathItemsGenerator()
    {
        var driveLetter = Gen.Choose('a', 'z').Or(Gen.Choose('A', 'Z')).Select(c => (char)c + ":");
        var directorySeparatorChar = Gen.Constant(Path.DirectorySeparatorChar).Select(c => c.ToString());
        var altDirectorySeparatorChar = Gen.Constant(Path.AltDirectorySeparatorChar).Select(c => c.ToString());
        var separator = Gen.OneOf([directorySeparatorChar, altDirectorySeparatorChar]);
        var drivePrefix = Gen.Zip(driveLetter, separator).Select(static t =>
        {
            var (driveLetter, separator) = t;
            return driveLetter + separator;
        });
        return Gen.Zip(drivePrefix, LinuxPathItemsGenerator()).Select(static t =>
        {
            var (prefix, items) = t;
            items.Insert(0, prefix);
            return items;
        });
    }
}

public class AnyOsPath
{
    public static Arbitrary<List<string>> Paths()
    {
        return Arb.From(Gen.Or(PathGenerators.LinuxPathItemsGenerator(), PathGenerators.WindowsPathItemsGenerator()));
    }
}

public class LinuxPath
{
    public static Arbitrary<List<string>> Paths()
    {
        return Arb.From(PathGenerators.LinuxPathItemsGenerator());
    }
}

public class WindowsPath
{
    public static Arbitrary<List<string>> Paths()
    {
        return Arb.From(PathGenerators.WindowsPathItemsGenerator());
    }
}
