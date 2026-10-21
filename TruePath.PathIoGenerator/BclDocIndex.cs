// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;

namespace TruePath.PathIoGenerator;

/// <summary>
///     Documentation lines for the <c>System.IO.File</c>/<c>System.IO.Directory</c> methods, extracted from the BCL
///     XML documentation files passed as additional files. The command-line compiler does not attach XML
///     documentation to referenced assemblies, so <see cref="ISymbol.GetDocumentationCommentXml"/> alone is not
///     enough in command-line builds.
/// </summary>
public sealed class BclDocIndex
{
    private readonly ImmutableArray<AdditionalText> _files;
    private readonly ImmutableDictionary<string, XElement> _members;

    private BclDocIndex(ImmutableArray<AdditionalText> files, ImmutableDictionary<string, XElement> members)
    {
        _files = files;
        _members = members;
    }

    public bool TryGetMethodMember(string docId, out XElement member)
    {
        return _members.TryGetValue(docId, out member!);
    }

    public static BclDocIndex Build(ImmutableArray<AdditionalText> files)
    {
        // netstandard.xml is processed first, so that the framework-specific System.Runtime.xml wins on conflicts.
        var orderedFiles = files.OrderBy(file => Path.GetFileName(file.Path) == "netstandard.xml" ? 0 : 1);

        var builder = ImmutableDictionary.CreateBuilder<string, XElement>();
        foreach (var file in orderedFiles)
        {
            var text = file.GetText();
            if (text == null) continue;

            // Some of the BCL documentation files (netstandard.xml in particular) contain invalid XML, so member
            // elements are extracted line by line and parsed one by one: a broken member is then skipped instead
            // of failing the whole file.
            var current = new StringBuilder();
            string? currentName = null;
            foreach (var textLine in text.Lines)
            {
                var line = textLine.ToString();
                var trimmed = line.TrimStart();
                if (currentName == null)
                {
                    if (!trimmed.StartsWith("<member ", StringComparison.Ordinal)) continue;

                    var nameStart = trimmed.IndexOf('"');
                    var nameEnd = nameStart < 0 ? -1 : trimmed.IndexOf('"', nameStart + 1);
                    if (nameEnd < 0) continue;

                    var name = trimmed.Substring(nameStart + 1, nameEnd - nameStart - 1);
                    if (!name.StartsWith("M:System.IO.File.", StringComparison.Ordinal)
                        && !name.StartsWith("M:System.IO.Directory.", StringComparison.Ordinal)) continue;

                    currentName = name;
                    current.Clear().Append(line);
                }
                else
                {
                    current.Append('\n').Append(line);
                }

                if (trimmed.TrimEnd().EndsWith("</member>", StringComparison.Ordinal))
                {
                    try
                    {
                        builder[currentName] = XElement.Parse(current.ToString());
                    }
                    catch (System.Xml.XmlException)
                    {
                        // Skip a malformed member.
                    }

                    currentName = null;
                }
            }
        }

        return new BclDocIndex(files, builder.ToImmutable());
    }

    public override bool Equals(object? obj)
    {
        return obj is BclDocIndex other && _files.SequenceEqual(other._files);
    }

    public override int GetHashCode()
    {
        return _files.Length;
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
    public static ImmutableArray<string> ToDocLines(XElement member, ImmutableArray<string> parameterNames)
    {
        // Work on a copy: the element may be a cached BclDocIndex entry shared across runs, and this method
        // renames its param elements in place.
        member = new XElement(member);

        var documentedParameters = new HashSet<string>(StringComparer.Ordinal);
        var elements = new List<string>();
        foreach (var element in member.Elements())
        {
            if (element.Name.LocalName == "param")
            {
                var name = element.Attribute("name")?.Value;
                if (name == null) continue;

                var actual = MatchParameter(name, parameterNames);
                if (actual == null) continue; // BCL documentation for a parameter this method does not have.

                if (!string.Equals(actual, name, StringComparison.Ordinal)) element.SetAttributeValue("name", actual);
                documentedParameters.Add(actual);
            }

            elements.Add(element.ToString(SaveOptions.DisableFormatting));
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
    {        line = ParamRefRegex.Replace(line, match =>
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
