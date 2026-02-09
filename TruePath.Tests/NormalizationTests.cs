// SPDX-FileCopyrightText: 2024-2025 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using JetBrains.Annotations;
using Xunit.Abstractions;

namespace TruePath.Tests;

public class NormalizationTests(ITestOutputHelper testOutputHelper)
{
    [Property(Arbitrary = [typeof(AnyOsPath)])]
    public void NormalizedPathDoesNotEndWithDirSeparator(List<string> pathParts)
    {
        var sourcePath = string.Join("", pathParts);

        // Act
        var normalizedPath = PathStrings.Normalize(sourcePath);

        testOutputHelper.WriteLine($"{sourcePath} => {normalizedPath}");
        Assert.True(normalizedPath == ""
            || normalizedPath == Path.DirectorySeparatorChar.ToString()
            || normalizedPath is [_, ':', _]
            || normalizedPath[^1] != Path.DirectorySeparatorChar);
    }

    [Property(Arbitrary = [typeof(AnyOsPath)])]
    public void NormalizedPathDoesNotContainAltDirSeparator(List<string> pathParts)
    {
        if (Path.DirectorySeparatorChar == Path.AltDirectorySeparatorChar) // test doesn't make sense on these systems
            return;

        var sourcePath = string.Join("", pathParts);

        // Act
        var normalizedPath = PathStrings.Normalize(sourcePath);

        testOutputHelper.WriteLine($"{sourcePath} => {normalizedPath}");
        Assert.True(!normalizedPath.Contains(Path.AltDirectorySeparatorChar));
    }

    [Property(Arbitrary = [typeof(AnyOsPath)])]
    public void NormalizedPathDoesNotContainTwoDirSeparator(List<string> pathParts)
    {
        var sourcePath = string.Join("", pathParts);

        // Act
        var normalizedPath = PathStrings.Normalize(sourcePath);

        testOutputHelper.WriteLine($"{sourcePath} => {normalizedPath}");
        Assert.True(!normalizedPath.Contains(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar));
    }

    [Property(Arbitrary = [typeof(AnyOsPath)])]
    public void DepthPreserved(List<string> pathParts)
    {
        var sourcePath = string.Join("", pathParts);

        // Act
        var normalizedPath = PathStrings.Normalize(sourcePath);

        testOutputHelper.WriteLine($"{sourcePath} => {normalizedPath}");
        var collapsedBlock = CollapseSameBlocks(pathParts);
        var expectedDepth = CountDepth(collapsedBlock);
        var actualDepth = CountDepth([.. normalizedPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries)]);
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

    private static List<string> CollapseSameBlocks(IReadOnlyCollection<string> pathParts)
    {
        var result = new List<string>();
        var lastSeparator = false;
        var hasHomeDrive = pathParts.Any(part => part.Length > 1 && part[1] == ':');
        string? homeDrive = null;
        foreach (var item in pathParts)
        {
            homeDrive ??= item;

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
        var dots = Gen.OneOf(baseDir, currentDir, threeDots);
        var textPath = Gen.Choose('a', 'z')
            .Or(Gen.Choose('A', 'Z'))
            .Or(Gen.Choose('0', '9'))
            .NonEmptyListOf()
            .Select(l => string.Join("", l.Select(c => (char)c)));

        var directorySeparatorChar = Gen.Constant(Path.DirectorySeparatorChar).Select(c => c.ToString());
        var altDirectorySeparatorChar = Gen.Constant(Path.AltDirectorySeparatorChar).Select(c => c.ToString());
        return Gen.OneOf(dots, directorySeparatorChar, altDirectorySeparatorChar, textPath).NonEmptyListOf()
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
        var drivePrefix = driveLetter.Zip(separator).Select(static t =>
        {
            var (driveLetter, separator) = t;
            return driveLetter + separator;
        });
        return drivePrefix.Zip(LinuxPathItemsGenerator()).Select(static t =>
        {
            var (prefix, items) = t;
            items.Insert(0, prefix);
            return items;
        });
    }
}

public class AnyOsPath
{
    [UsedImplicitly]
    public static Arbitrary<List<string>> Paths()
    {
        return Arb.From(PathGenerators.LinuxPathItemsGenerator().Or(PathGenerators.WindowsPathItemsGenerator()));
    }
}

[UsedImplicitly]
public class LinuxPath
{
    [UsedImplicitly]
    public static Arbitrary<List<string>> Paths()
    {
        return Arb.From(PathGenerators.LinuxPathItemsGenerator());
    }
}

[UsedImplicitly]
public class WindowsPath
{
    [UsedImplicitly]
    public static Arbitrary<List<string>> Paths()
    {
        return Arb.From(PathGenerators.WindowsPathItemsGenerator());
    }
}
