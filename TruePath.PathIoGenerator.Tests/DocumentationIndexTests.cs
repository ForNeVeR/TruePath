// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace TruePath.PathIoGenerator.Tests;

public sealed class DocumentationIndexTests
{
    private const string Id = "M:System.IO.File.ReadAllText(System.String)";

    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("\r")]
    public void ReadsRelevantMembersWithAnyLineEndingsAndTrailingWhitespace(string newline)
    {
        var text = string.Join(newline, new[]
        {
            "<doc><members>",
            "  <member name=\"M:System.String.Unrelated\"><invalid unrelated XML",
            $"\t<member name=\"{Id}\">",
            "  <summary>first", "second</summary>", "  </member> \t",
            "</members></doc>"
        });
        var index = Build(new InMemoryAdditionalText("System.Runtime.xml", text));

        Assert.True(index.TryGetMethodMember(Id, out var member));
        Assert.Equal("first\nsecond", member.Element("summary")!.Value);
        Assert.False(index.TryGetMethodMember("M:System.String.Unrelated", out _));
    }

    [Theory]
    [InlineData(61)]
    [InlineData(128)]
    public void ReadsMembersWhoseOpeningTagCrossesOrFollowsThePrefixBuffer(int indentation)
    {
        var index = Build(new InMemoryAdditionalText("System.Runtime.xml", new string(' ', indentation)
            + $"<member name=\"{Id}\"><summary>valid</summary></member>"));

        Assert.True(index.TryGetMethodMember(Id, out var member));
        Assert.Equal("valid", member.Element("summary")!.Value);
    }

    [Fact]
    public void ReadsSingleLineMembersAndSkipsMalformedMembers()
    {
        var index = Build(new InMemoryAdditionalText("System.Runtime.xml", $"""
            <doc><members>
            <member name="M:System.IO.File.Broken"><summary></member>
            <member name="{Id}"><summary>valid</summary></member>
            <member name="M:System.IO.FileStream.Unrelated"><summary>ignored</summary></member>
            <member name="M:System.IO.Directory.Delete(System.String)"><summary>directory</summary></member>
            </members></doc>
            """));

        Assert.False(index.TryGetMethodMember("M:System.IO.File.Broken", out _));
        Assert.False(index.TryGetMethodMember("M:System.IO.FileStream.Unrelated", out _));
        Assert.True(index.TryGetMethodMember(Id, out _));
        Assert.True(index.TryGetMethodMember("M:System.IO.Directory.Delete(System.String)", out _));
    }

    [Fact]
    public void MalformedFrameworkMemberDoesNotReplaceValidStandardDocumentation()
    {
        var index = Build(
            new InMemoryAdditionalText("System.Runtime.xml", $"<member name=\"{Id}\"><summary></member>"),
            new InMemoryAdditionalText("netstandard.xml", $"<member name=\"{Id}\"><summary>valid</summary></member>"));

        Assert.True(index.TryGetMethodMember(Id, out var member));
        Assert.Equal("valid", member.Element("summary")!.Value);
    }

    [Fact]
    public void PublicLookupCannotMutateTheCachedMember()
    {
        var index = Build(new InMemoryAdditionalText("System.Runtime.xml",
            $"<member name=\"{Id}\"><summary>original</summary></member>"));
        Assert.True(index.TryGetMethodMember(Id, out var first));
        first.ReplaceAll(new XElement("summary", "changed"));

        Assert.True(index.TryGetMethodMember(Id, out var second));
        Assert.Equal("original", second.Element("summary")!.Value);
    }

    [Fact]
    public void FormattingPreservesIndependentFragmentsEscapingAndNamespaces()
    {
        var member = XElement.Parse("""
            <member>
              <summary>A &amp; B <paramref name="sourceFileName" /></summary>
              <param name="sourceFileName">Path &lt;value&gt;</param>
              <returns><d:item xmlns:d="urn:docs">result</d:item></returns>
              <remarks><d:item xmlns:d="urn:docs">details</d:item></remarks>
            </member>
            """);

        var lines = DocFormatting.ToDocLines(member, ImmutableArray.Create("sourceFile"), TestContext.Current.CancellationToken);

        Assert.Equal(new[]
        {
            "<summary>A &amp; B <paramref name=\"sourceFile\"/></summary>",
            "<param name=\"sourceFile\">Path &lt;value&gt;</param>",
            "<returns><d:item xmlns:d=\"urn:docs\">result</d:item></returns>",
            "<remarks><d:item xmlns:d=\"urn:docs\">details</d:item></remarks>"
        }, lines);
        Assert.Equal("sourceFileName", member.Element("param")!.Attribute("name")!.Value);
    }

    private static BclDocIndex Build(params AdditionalText[] files) =>
        BclDocIndex.Build(files.ToImmutableArray(), TestContext.Current.CancellationToken);
}
