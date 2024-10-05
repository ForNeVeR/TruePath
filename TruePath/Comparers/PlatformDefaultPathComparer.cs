// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace TruePath.Comparers;

/// <summary>
/// <para>Provides a default comparer for comparing file paths, aware of the current platform.</para>
/// <para>
///     On <b>Windows</b> and <b>macOS</b>, this will perform <b>case-insensitive</b> comparison, since the file
///     systems are case-insensitive on these operating systems by default.
/// </para>
/// <para>On <b>Linux</b>, the comparison will be <b>case-sensitive</b>.</para>
/// </summary>
/// <remarks>
/// Note that this comparison <b>does not guarantee correctness</b>: in practice, on any platform to control
/// case-sensitiveness of either the whole file system or a part of it. This class does not take this into account,
/// having a benefit of no accessing the file system for any of the comparisons.
/// </remarks>
public class PlatformDefaultPathComparer : IComparer<string>
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="PlatformDefaultPathComparer"/> class.
    /// </summary>
    public static readonly PlatformDefaultPathComparer Instance = new();

    private readonly StringComparer _comparisonType;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformDefaultPathComparer"/> class.
    /// </summary>
    private PlatformDefaultPathComparer()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            _comparisonType = StringComparer.OrdinalIgnoreCase;
        }
        else
        {
            _comparisonType = StringComparer.Ordinal;
        }
    }

    /// <inheritdoc cref="IComparer{T}.Compare"/>
    public int Compare(string? x, string? y)
    {
        return _comparisonType.Compare(x, y);
    }
}
