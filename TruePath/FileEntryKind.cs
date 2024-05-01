using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TruePath;
/// <summary>
/// Represents a kind of file system entry.
/// </summary>
public enum FileEntryKind
{
    /// <summary>An unknown kind of file system entry.</summary>
    Unknown,
    /// <summary>A file.</summary>
    File,
    /// <summary>A directory.</summary>
    Directory,
    // TODO: Add more kinds of file system entries.

}
