// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace TruePath.Comparers;

/// <summary>
/// <para>Provides a default comparer for comparing file paths, aware of the current platform.</para>
/// <para>
///     On <b>Windows</b> and <b>macOS</b>, this will perform <b>case-insensitive</b> string comparison, since the file
///     systems are case-insensitive on these operating systems by default.
/// </para>
/// <para>On <b>Linux</b>, the comparison will be <b>case-sensitive</b>.</para>
/// </summary>
/// <remarks>
/// Note that this comparison <b>does not guarantee correctness</b>: in practice, on any platform to control
/// case-sensitiveness of either the whole file system or a part of it. This class does not take this into account,
/// having a benefit of no accessing the file system for any of the comparisons.
/// </remarks>
internal class PlatformDefaultPathComparer<TPath> : PathComparer<TPath> where TPath : IPath
{
    private readonly StringComparer _stringComparer;

    public PlatformDefaultPathComparer()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            _stringComparer = StringComparer.OrdinalIgnoreCase;
        }
        else
        {
            _stringComparer = StringComparer.Ordinal;
        }
    }

    public override bool Equals(TPath? x, TPath? y)
    {
        return _stringComparer.Equals(x?.Value, y?.Value);
    }

    public override int GetHashCode(TPath obj)
    {
        return _stringComparer.GetHashCode(obj.Value);
    }

    public override int Compare(TPath? x, TPath? y)
    {
        return _stringComparer.Compare(x?.Value, y?.Value);
    }
}
