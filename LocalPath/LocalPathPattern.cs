namespace LocalPath;

/// <summary>
/// <para>An opaque pattern that may be checked if it corresponds to a local path.</para>
/// <para>This is a token type, created with an idea that it should be used by some external means.</para>
/// </summary>
/// <param name="Pattern">
/// The pattern text. May be of various formats: say, a glob. This library doesn't dictate any particular format and
/// gives no guarantees.
/// </param>
public record struct LocalPathPattern(string Pattern)
{
    public override string ToString() => Pattern;
}
