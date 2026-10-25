// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace TruePath.PathIoGenerator;

/// <summary>
///     Documentation lines for the <c>System.IO.File</c>/<c>System.IO.Directory</c> methods, extracted from the BCL
///     XML documentation files passed as additional files. The command-line compiler does not attach XML
///     documentation to referenced assemblies, so <see cref="ISymbol.GetDocumentationCommentXml"/> alone is not
///     enough in command-line builds.
/// </summary>
public sealed class BclDocIndex
{
    private readonly bool _isNetstandard;
    private readonly ImmutableDictionary<string, XElement> _members;

    private BclDocIndex(ImmutableArray<AdditionalText> files, ImmutableDictionary<string, XElement> members)
    {
        _isNetstandard = files.All(file => Path.GetFileName(file.Path) == "netstandard.xml");
        _members = members;
    }

    public bool TryGetMethodMember(string docId, out XElement member)
    {
        if (_members.TryGetValue(docId, out var stored))
        {
            member = new XElement(stored);
            return true;
        }
        member = null!;
        return false;
    }

    // Only DocFormatting reads the shared instance, and it clones before normalizing parameters.
    internal XElement? GetMethodMember(string docId) => _members.TryGetValue(docId, out var member) ? member : null;

    internal static BclDocIndex Merge(ImmutableArray<BclDocIndex> indexes, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var members = ImmutableDictionary.CreateBuilder<string, XElement>();
        foreach (var index in indexes.OrderBy(index => index._isNetstandard ? 0 : 1))
        foreach (var pair in index._members)
        {
            cancellationToken.ThrowIfCancellationRequested();
            members[pair.Key] = pair.Value;
        }
        return new BclDocIndex(ImmutableArray<AdditionalText>.Empty, members.ToImmutable());
    }

    public static BclDocIndex Build(ImmutableArray<AdditionalText> files, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // netstandard.xml is processed first, so that the framework-specific System.Runtime.xml wins on conflicts.
        var orderedFiles = files.OrderBy(file => Path.GetFileName(file.Path) == "netstandard.xml" ? 0 : 1);

        var builder = ImmutableDictionary.CreateBuilder<string, XElement>();
        foreach (var file in orderedFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var text = file.GetText(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (text == null) continue;

            // Some of the BCL documentation files (netstandard.xml in particular) contain invalid XML, so member
            // elements are extracted line by line and parsed one by one: a broken member is then skipped instead
            // of failing the whole file.
            var prefix = new char[64];
            var memberStart = 0;
            string? currentName = null;
            foreach (var textLine in text.Lines)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var span = textLine.Span;
                if (currentName == null)
                {
                    if (!StartsMember(text, span, prefix, out var start)) continue;
                    var nameStart = start;
                    while (nameStart < span.End && text[nameStart] != '"') nameStart++;
                    if (nameStart == span.End) continue;
                    nameStart++;
                    var nameEnd = nameStart;
                    while (nameEnd < span.End && text[nameEnd] != '"') nameEnd++;
                    if (nameEnd == span.End) continue;
                    if (!Matches(text, nameStart, nameEnd, "M:System.IO.File.")
                        && !Matches(text, nameStart, nameEnd, "M:System.IO.Directory.")) continue;

                    currentName = text.ToString(TextSpan.FromBounds(nameStart, nameEnd));
                    memberStart = span.Start;
                }

                var end = span.End;
                while (end > span.Start && char.IsWhiteSpace(text[end - 1])) end--;
                if (!Matches(text, end - "</member>".Length, end, "</member>")) continue;
                try
                {
                    // Allocate only a relevant member. XElement normalizes CR/LF while parsing, just as the
                    // previous line-by-line reconstruction did; unrelated documentation never becomes a string.
                    builder[currentName] = XElement.Parse(text.ToString(TextSpan.FromBounds(memberStart, span.End)));
                }
                catch (System.Xml.XmlException)
                {
                    // Skip a malformed member without hiding other valid entries.
                }
                currentName = null;
            }
        }

        return new BclDocIndex(files, builder.ToImmutable());
    }

    private static bool StartsMember(SourceText text, TextSpan line, char[] prefix, out int start)
    {
        // Bulk copying avoids virtual SourceText indexer calls for every character of every unrelated line.
        var length = Math.Min(prefix.Length, line.Length);
        text.CopyTo(line.Start, prefix, 0, length);
        var offset = 0;
        while (offset < length && char.IsWhiteSpace(prefix[offset])) offset++;
        start = line.Start + offset;
        const string opening = "<member ";
        if (offset + opening.Length <= length)
        {
            for (var i = 0; i < opening.Length; i++)
                if (prefix[offset + i] != opening[i]) return false;
            return true;
        }
        // Unusually deep indentation or a tag straddling the prefix still uses the full line.
        while (start < line.End && char.IsWhiteSpace(text[start])) start++;
        return Matches(text, start, line.End, opening);
    }

    private static bool Matches(SourceText text, int start, int end, string value)
    {
        if (start < 0 || end - start < value.Length) return false;
        for (var i = 0; i < value.Length; i++)
            if (text[start + i] != value[i]) return false;
        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is BclDocIndex other && _isNetstandard == other._isNetstandard
            && _members.Count == other._members.Count
            && _members.All(pair => other._members.TryGetValue(pair.Key, out var member) && XNode.DeepEquals(pair.Value, member));
    }

    public override int GetHashCode()
    {
        return _members.Count;
    }
}

internal static class DocFormatting
{
    private static readonly Regex ParamRefRegex = new(
        "<paramref\\s+name=\"(?<name>[^\"]+)\"\\s*(?:/>|>(?<inner>[^<]*)</paramref>)",
        RegexOptions.Compiled);

    // Some legacy BCL documentation contains docfx artifact crefs such as "~/docs/…​.md", which are not valid
    // member references and would fail the XML documentation checks.
    private static readonly Regex PairedSeeCrefRegex = new(
        "<(?<tag>see|seealso)\\s+cref=\"(?<cref>[^\"]+)\">(?<text>[^<]*)</(?:see|seealso)>",
        RegexOptions.Compiled);

    private static readonly Regex SelfClosingSeeCrefRegex = new(
        "<(?:see|seealso)\\s+cref=\"(?<cref>[^\"]+)\"\\s*/>",
        RegexOptions.Compiled);

    /// <summary>
    ///     Formats the child elements of a BCL `member` element as `///`-ready lines. Parameter references are
    ///     normalized against the actual parameter names of the generated method: the BCL documentation sometimes
    ///     mismatches them (in casing or entirely), which would break XML documentation checks.
    /// </summary>
    public static ImmutableArray<string> ToDocLines(XElement member, ImmutableArray<string> parameterNames, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Work on a copy: the element may be a cached BclDocIndex entry shared across runs, and this method
        // renames its param elements in place.
        member = new XElement(member);

        var documentedParameters = new HashSet<string>(StringComparer.Ordinal);
        var elements = new List<string>();
        // A writer allocates a sizable buffer. Reuse it across this member's independent XML fragments.
        var buffer = new StringBuilder();
        using var textWriter = new StringWriter(buffer, CultureInfo.InvariantCulture);
        using var xmlWriter = XmlWriter.Create(textWriter, new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            ConformanceLevel = ConformanceLevel.Fragment,
        });
        foreach (var element in member.Elements())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (element.Name.LocalName == "param")
            {
                var name = element.Attribute("name")?.Value;
                if (name == null) continue;

                var actual = MatchParameter(name, parameterNames);
                if (actual == null) continue; // BCL documentation for a parameter this method does not have.

                if (!string.Equals(actual, name, StringComparison.Ordinal)) element.SetAttributeValue("name", actual);
                documentedParameters.Add(actual);
            }

            element.WriteTo(xmlWriter);
            xmlWriter.Flush();
            elements.Add(buffer.ToString());
            buffer.Clear();
        }

        foreach (var missing in parameterNames.Where(p => !documentedParameters.Contains(p)))
            elements.Add("<param name=\"" + missing + "\">See the documentation of the corresponding System.IO API.</param>");

        return elements
            .SelectMany(element => Rewrite(element, parameterNames).Replace("\r\n", "\n").Split('\n'))
            .Where(line => line.Trim().Length > 0)
            .ToImmutableArray();
    }

    /// <summary>
    ///     Finds the generated parameter a BCL documentation name refers to: either a direct (case-insensitive)
    ///     match, or a `…Name`-suffixed BCL spelling such as `sourceFileName` referring to the generated
    ///     `sourceFile` parameter.
    /// </summary>
    private static string? MatchParameter(string documentationName, ImmutableArray<string> parameterNames)
    {
        var direct = parameterNames.FirstOrDefault(p => string.Equals(p, documentationName, StringComparison.OrdinalIgnoreCase));
        if (direct != null) return direct;

        return parameterNames.FirstOrDefault(p =>
            string.Equals(p + "Name", documentationName, StringComparison.OrdinalIgnoreCase));
    }

    private static string Rewrite(string line, ImmutableArray<string> parameterNames)
    {
        line = ParamRefRegex.Replace(line, match =>
        {
            var actual = MatchParameter(match.Groups["name"].Value, parameterNames);
            var inner = match.Groups["inner"].Success ? match.Groups["inner"].Value : null;
            if (actual == null) return "<c>" + (inner ?? match.Groups["name"].Value) + "</c>";
            return inner == null
                ? "<paramref name=\"" + actual + "\"/>"
                : "<paramref name=\"" + actual + "\">" + inner + "</paramref>";
        });

        line = PairedSeeCrefRegex.Replace(line, match =>
            IsBadCref(match.Groups["cref"].Value) ? match.Groups["text"].Value : match.Value);
        return SelfClosingSeeCrefRegex.Replace(line, match =>
            IsBadCref(match.Groups["cref"].Value) ? "" : match.Value);
    }

    private static bool IsBadCref(string cref)
    {
        return cref.Contains('/') || cref.Contains('~');
    }
}
