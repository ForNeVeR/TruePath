using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.Win32.SafeHandles;

namespace TruePath;

/// <summary>
/// Provides interop methods for the libc library.
/// </summary>
internal static partial class Libc
{
    /// <summary>
    /// Resolves the absolute path of the specified <paramref name="path"/> and stores it in the provided <paramref name="buffer"/>.
    /// </summary>
    /// <param name="path">The file path to resolve.</param>
    /// <param name="buffer">A pointer to a buffer where the resolved absolute path will be stored.</param>
    /// <returns>A <see cref="SafeFileHandle"/> representing the resolved path, or an invalid handle if the function fails.</returns>
    /// <remarks>
    /// This method is an interop call to the 'realpath' function in the libc library. The <paramref name="buffer"/> should be allocated with enough space to hold the resolved path.
    /// </remarks>
    [LibraryImport("libc",
        EntryPoint = "realpath",
        SetLastError = true,
        StringMarshalling = StringMarshalling.Custom,
        StringMarshallingCustomType = typeof(AnsiStringMarshaller))]
    internal static partial SafeFileHandle RealPath(string path, IntPtr buffer);
}
