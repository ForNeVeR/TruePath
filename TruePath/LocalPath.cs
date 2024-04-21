namespace TruePath;

/// <summary>
/// <para>A path pointing to a place in the local file system.</para>
/// <para>It may be either absolute or relative.</para>
/// <para>
///     Always stored in a normalized form. Read the documentation on <see cref="TruePath.PathStrings.Normalize"/> to
///     know what form of normalization does the path use.
/// </para>
/// </summary>
public readonly struct LocalPath(string value)
{
    /// <summary>The normalized path string.</summary>
    public string Value { get; } = PathStrings.Normalize(value);

    /// <summary>
    /// <para>Checks whether th path is absolute.</para>
    /// <para>
    ///     Currently, any rooted paths are considered absolute, but this is a subject to change: on Windows, there
    ///     will be an additional requirement for a path to be either UNC or start from a disk letter.
    /// </para>
    /// </summary>
    public bool IsAbsolute => Path.IsPathRooted(Value);

    /// <summary>
    /// The parent of this path. Will be <c>null</c> for a rooted absolute path, or relative path pointing to the
    /// current directory.
    /// </summary>
    // TODO: Add tests for parent of `.` and `""` and `..`: what should they be actually?
    public LocalPath? Parent => Path.GetDirectoryName(Value) is { } parent ? new(parent) : null;

    /// <summary>The full name of the last component of this path.</summary>
    public string FileName => Path.GetFileName(Value);

    public override string ToString() => Value;

    public bool Equals(LocalPath other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is LocalPath other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(LocalPath left, LocalPath right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(LocalPath left, LocalPath right)
    {
        return !left.Equals(right);
    }

    /// <remarks>
    /// Checks for a non-strict prefix: if the paths are equal then they are still considered prefixes of each other.
    /// </remarks>
    public bool IsPrefixOf(LocalPath other)
    {
        if (!(Value.Length <= other.Value.Length && other.Value.StartsWith(Value))) return false;
        return other.Value.Length == Value.Length || other.Value[Value.Length] == Path.DirectorySeparatorChar;
    }

    public LocalPath RelativeTo(LocalPath basePath) => new(Path.GetRelativePath(basePath.Value, Value));

    /// <remarks>
    /// Note that in case path <paramref name="b"/> is <b>absolute</b>, it will completely take over and the
    /// <paramref name="basePath"/> will be ignored.
    /// </remarks>
    public static LocalPath operator /(LocalPath basePath, LocalPath b) =>
        new(Path.Combine(basePath.Value, b.Value));

    /// <remarks>
    /// Note that in case path <paramref name="b"/> is <b>absolute</b>, it will completely take over and the
    /// <paramref name="basePath"/> will be ignored.
    /// </remarks>
    public static LocalPath operator /(LocalPath basePath, string b) => basePath / new LocalPath(b);

    public static implicit operator LocalPath(AbsolutePath path) => new(path.Value);
}
