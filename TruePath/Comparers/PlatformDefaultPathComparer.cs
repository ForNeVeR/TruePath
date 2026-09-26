// SPDX-FileCopyrightText: 2024 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace TruePath.Comparers;

/// <summary>
/// <para>Provides a default comparer for comparing file paths, aware of the current platform.</para>
/// <para>
/// On <b>Windows</b>, <b>macOS</b>, <b>iOS</b> and <b>tvOS</b>, this will perform <b>case-insensitive</b> string
/// comparison, since the file systems are case-insensitive on these operating systems by default.
/// </para>
/// <para>On <b>Linux</b>, the comparison will be <b>case-sensitive</b>.</para>
/// </summary>
/// <remarks>
/// Note that this comparison <b>does not guarantee correctness</b>: in practice, on any platform to control
/// case-sensitiveness of either the whole file system or a part of it. This class does not take this into account,
/// having a benefit of no accessing the file system for any of the comparisons.
/// </remarks>
internal class PlatformDefaultPathComparer<TPath> : IPathComparer<TPath> where TPath : IPath
{
    private readonly StringComparer _stringComparer =
        PlatformDefaultPathComparer.DefaultStringComparison == StringComparison.OrdinalIgnoreCase
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

    public bool Equals(TPath? x, TPath? y)
    {
        return _stringComparer.Equals(x?.Value, y?.Value);
    }

    public int GetHashCode(TPath obj)
    {
        return _stringComparer.GetHashCode(obj.Value);
    }

    public int Compare(TPath? x, TPath? y)
    {
        return _stringComparer.Compare(x?.Value, y?.Value);
    }
}

internal static class PlatformDefaultPathComparer
{
    // Matches the .NET runtime's PathInternal.IsCaseSensitive:
    // https://github.com/dotnet/runtime/blob/60629d14374c56f1cb51819049ad1fa529307f8d/src/libraries/Common/src/System/IO/PathInternal.CaseSensitivity.cs#L23-L29
    internal static readonly StringComparison DefaultStringComparison =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
        || RuntimeInformation.IsOSPlatform(OSPlatform.Create("IOS"))
        || RuntimeInformation.IsOSPlatform(OSPlatform.Create("TVOS"))
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
}
