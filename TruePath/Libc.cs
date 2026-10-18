// SPDX-FileCopyrightText: 2024-2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace TruePath;

/// <summary>
/// Provides interop methods for the libc library.
/// </summary>
internal static class Libc
{
    /// <summary>
    /// Resolves the absolute path of the specified <paramref name="path"/> and stores it in the provided <paramref name="buffer"/>.
    /// </summary>
    /// <param name="path">The file path to resolve.</param>
    /// <param name="buffer">A pointer to a buffer where the resolved absolute path will be stored.</param>
    /// <returns>
    /// A pointer to the resolved path, or <see cref="IntPtr.Zero"/> if the function fails. If <paramref name="buffer"/>
    /// is <see cref="IntPtr.Zero"/>, the result is allocated by libc and should be released with <see cref="Free"/>.
    /// </returns>
    /// <remarks>
    /// This method is an interop call to the 'realpath' function in the libc library. The <paramref name="buffer"/>, if
    /// not <see cref="IntPtr.Zero"/>, should be allocated with enough space to hold the resolved path.
    /// </remarks>
    [DllImport("libc",
        EntryPoint = "realpath",
        CharSet = CharSet.Ansi)]
    internal static extern IntPtr RealPath(string path, IntPtr buffer);

    /// <summary>Releases the memory allocated by libc.</summary>
    /// <param name="ptr">The pointer to the memory to release.</param>
    [DllImport("libc", EntryPoint = "free")]
    internal static extern void Free(IntPtr ptr);
}
