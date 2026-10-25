// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace TruePath.PathIoGenerator.Tests;

public sealed class IncrementalityTests
{
    private const string ReadAllTextDeclaration = "public static string ReadAllText(this AbsolutePath path)";
    private const string ReadAllTextId = "M:System.IO.File.ReadAllText(System.String)";

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnrelatedAndMarkedCommentEditsReuseTemplatesAndRenderedSource(bool editMarkedFile)
    {
        var compilation = TestHelper.CreateCompilation(TestHelper.DefaultSources);
        var unrelated = Parse("class Other { int M() => 0; }");
        compilation = compilation.AddSyntaxTrees(unrelated);
        var driver = CreateDriver().RunGenerators(compilation, TestContext.Current.CancellationToken);
        var original = Source(driver);
        var tree = editMarkedFile ? compilation.SyntaxTrees.First() : unrelated;

        for (var i = 0; i < 10; i++)
        {
            var replacement = tree.WithChangedText(SourceText.From(editMarkedFile
                ? TestHelper.DefaultSources + $"\n// {i}"
                : $"class Other {{ int M() => {i + 1}; }}"));
            compilation = compilation.ReplaceSyntaxTree(tree, replacement);
            tree = replacement;
            driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

            Assert.Equal(original, Source(driver));
            AssertCached(driver, "MethodTemplates", "RenderedSource");
        }
    }

    [Fact]
    public void ExclusionCommentsOrderingAndDiagnosticsDoNotInvalidateSource()
    {
        var compilation = TestHelper.CreateCompilation(TestHelper.DefaultSources);
        AdditionalText exclusions = new InMemoryAdditionalText("path-io-exclusions.txt",
            "M:System.IO.File.Encrypt(System.String)\nM:System.IO.File.Decrypt(System.String)");
        var driver = CreateDriver(exclusions).RunGenerators(compilation, TestContext.Current.CancellationToken);
        var original = Source(driver);
        var replacement = new InMemoryAdditionalText(exclusions.Path,
            "# comment\nM:System.IO.File.Decrypt(System.String)\nM:System.IO.File.Encrypt(System.String)\ninvalid");

        driver = driver.ReplaceAdditionalText(exclusions, replacement)
            .RunGenerators(compilation, TestContext.Current.CancellationToken);

        Assert.Equal(original, Source(driver));
        AssertCached(driver, "MethodTemplates", "RenderedSource");
        Assert.Contains(driver.GetRunResult().Diagnostics, diagnostic => diagnostic.Id == "TPG003");

        driver = driver.ReplaceAdditionalText(replacement, new InMemoryAdditionalText(exclusions.Path, ""))
            .RunGenerators(compilation, TestContext.Current.CancellationToken);
        Assert.Contains("public static void Encrypt(", Source(driver));
        Assert.Empty(driver.GetRunResult().Diagnostics);
        AssertCached(driver, "MethodTemplates");
    }

    [Fact]
    public void OtherPartialDeclarationsChangeHandWrittenOverrides()
    {
        var compilation = TestHelper.CreateCompilation(TestHelper.DefaultSources);
        var driver = CreateDriver().RunGenerators(compilation, TestContext.Current.CancellationToken);
        Assert.Contains(ReadAllTextDeclaration, Source(driver));
        var partial = Parse("""
            namespace TruePath.SystemIo;
            public static partial class PathIo
            {
                public static string ReadAllText(TruePath.AbsolutePath path) => "custom";
            }
            """);

        driver = driver.RunGenerators(compilation.AddSyntaxTrees(partial), TestContext.Current.CancellationToken);
        Assert.DoesNotContain(ReadAllTextDeclaration, Source(driver));
        AssertCached(driver, "MethodTemplates");

        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);
        Assert.Contains(ReadAllTextDeclaration, Source(driver));
        AssertCached(driver, "MethodTemplates");
    }

    [Fact]
    public void GlobalAliasChangesRebindUnchangedForwardingBodies()
    {
        var compilation = TestHelper.CreateCompilation(TestHelper.SourcesWithHandWrittenMembers(
            "public static void Custom(TruePath.AbsolutePath path) => Io.Delete(path.Value);"));
        var alias = Parse("global using Io = System.IO.File;");
        compilation = compilation.AddSyntaxTrees(alias);
        var driver = CreateDriver().RunGenerators(compilation, TestContext.Current.CancellationToken);
        Assert.Contains("public static void Delete(this AbsolutePath path) => Directory.Delete(path.Value);", Source(driver));

        compilation = compilation.ReplaceSyntaxTree(alias, Parse("global using Io = System.IO.Directory;"));
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        Assert.Contains("public static void Delete(this AbsolutePath path) => File.Delete(path.Value);", Source(driver));
        AssertCached(driver, "MethodTemplates");
    }

    [Fact]
    public void UnchangedDocumentationMembersStopBeforeFormattingAndOtherFilesAreNotReadAgain()
    {
        var compilation = TestHelper.CreateCompilation(TestHelper.DefaultSources);
        var first = new CountingText("netstandard.xml", Docs("standard"));
        var second = new CountingText("System.Runtime.xml", Docs("runtime"));
        var driver = CreateDriver(second, first).RunGenerators(compilation, TestContext.Current.CancellationToken);
        Assert.Contains("runtime", Source(driver));

        var replacement = new CountingText(second.Path, Docs("runtime") + "\n<!-- unrelated -->");
        driver = driver.ReplaceAdditionalText(second, replacement)
            .RunGenerators(compilation, TestContext.Current.CancellationToken);

        Assert.Equal(1, first.Reads);
        Assert.Equal(1, replacement.Reads);
        AssertCached(driver, "DocumentationIndex", "MethodTemplates", "RenderedSource");

        driver = driver.ReplaceAdditionalText(replacement, new CountingText(second.Path, Docs("updated")))
            .RunGenerators(compilation, TestContext.Current.CancellationToken);
        Assert.Contains("updated", Source(driver));
        Assert.Equal(1, first.Reads);
    }

    [Fact]
    public void RemovingFrameworkDocumentationRevealsTheStandardDocumentation()
    {
        var compilation = TestHelper.CreateCompilation(TestHelper.DefaultSources);
        var standard = new InMemoryAdditionalText("netstandard.xml", Docs("standard"));
        var framework = new InMemoryAdditionalText("System.Runtime.xml", Docs("framework"));
        var driver = CreateDriver(framework, standard).RunGenerators(compilation, TestContext.Current.CancellationToken);

        driver = driver.RemoveAdditionalTexts(ImmutableArray.Create<AdditionalText>(framework))
            .RunGenerators(compilation, TestContext.Current.CancellationToken);

        Assert.Contains("standard", Source(driver));
        Assert.DoesNotContain("<summary>framework</summary>", Source(driver));
    }

    [Theory]
    [InlineData("\u0085")]
    [InlineData("\u2028")]
    [InlineData("\u2029")]
    public void UnicodeDocumentationLineBreaksKeepEveryGeneratedLineCommented(string newline)
    {
        var compilation = TestHelper.CreateCompilation(TestHelper.DefaultSources);
        var docs = new InMemoryAdditionalText("System.Runtime.xml", Docs("first" + newline + "second"));
        var driver = CreateDriver(docs).RunGenerators(compilation, TestContext.Current.CancellationToken);

        Assert.Contains("/// <summary>first" + Environment.NewLine + "    /// second</summary>", Source(driver));
        Assert.DoesNotContain(newline, Source(driver));
        var updated = compilation.AddSyntaxTrees(driver.GetRunResult().GeneratedTrees);
        Assert.Empty(updated.GetDiagnostics(TestContext.Current.CancellationToken)
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
    }

    [Theory]
    [InlineData("<!-- first -->", "<!-- second -->")]
    [InlineData("<?example first?>", "<?example second?>")]
    public void CommentsAndProcessingInstructionsInsideDocumentationInvalidateOutput(string before, string after)
    {
        var compilation = TestHelper.CreateCompilation(TestHelper.DefaultSources);
        var original = new InMemoryAdditionalText("System.Runtime.xml", Docs("text " + before));
        var driver = CreateDriver(original).RunGenerators(compilation, TestContext.Current.CancellationToken);

        driver = driver.ReplaceAdditionalText(original,
                new InMemoryAdditionalText(original.Path, Docs("text " + after)))
            .RunGenerators(compilation, TestContext.Current.CancellationToken);

        Assert.Contains(after, Source(driver));
        Assert.DoesNotContain(before, Source(driver));
    }

    [Fact]
    public void SymbolDocumentationChangesInvalidateTemplatesEvenWhenApiSignaturesAreUnchanged()
    {
        var compilation = TestHelper.CreateCompilation(TestHelper.DefaultSources);
        var fileAssembly = compilation.GetTypeByMetadataName("System.IO.File")!.ContainingAssembly;
        var originalReference = (PortableExecutableReference)compilation.GetMetadataReference(fileAssembly)!;
        using var metadata = (AssemblyMetadata)originalReference.GetMetadata();
        var reference = metadata.GetReference(documentation: new MethodDocumentation("first"));
        compilation = compilation.ReplaceReference(originalReference, reference);
        var driver = CreateDriver().RunGenerators(compilation, TestContext.Current.CancellationToken);
        Assert.Contains("<summary>first</summary>", Source(driver));

        compilation = compilation.ReplaceReference(reference,
            metadata.GetReference(documentation: new MethodDocumentation("second")));
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        Assert.Contains("<summary>second</summary>", Source(driver));
        Assert.DoesNotContain("<summary>first</summary>", Source(driver));
    }

    [Fact]
    public void PartialValidationAndDiagnosticLocationsFollowEdits()
    {
        var text = TestHelper.DefaultSources.Replace("static partial class", "static class");
        var compilation = TestHelper.CreateCompilation(text);
        var driver = CreateDriver().RunGenerators(compilation, TestContext.Current.CancellationToken);
        var before = Assert.Single(driver.GetRunResult().Diagnostics, diagnostic => diagnostic.Id == "TPG002");
        Assert.Empty(driver.GetRunResult().GeneratedTrees);

        var tree = compilation.SyntaxTrees.Single();
        compilation = compilation.ReplaceSyntaxTree(tree, tree.WithChangedText(SourceText.From("\n\n" + text)));
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);
        var after = Assert.Single(driver.GetRunResult().Diagnostics, diagnostic => diagnostic.Id == "TPG002");
        Assert.Equal(before.Location.GetLineSpan().StartLinePosition.Line + 2,
            after.Location.GetLineSpan().StartLinePosition.Line);
        AssertCached(driver, "RenderedSource");

        driver = driver.RunGenerators(TestHelper.CreateCompilation(TestHelper.DefaultSources), TestContext.Current.CancellationToken);
        Assert.Empty(driver.GetRunResult().Diagnostics);
        Assert.NotEmpty(driver.GetRunResult().GeneratedTrees);
    }

    [Fact]
    public void RemovingTheMarkerRemovesGeneratedSource()
    {
        var compilation = TestHelper.CreateCompilation(TestHelper.DefaultSources);
        var driver = CreateDriver().RunGenerators(compilation, TestContext.Current.CancellationToken);
        Assert.NotEmpty(driver.GetRunResult().GeneratedTrees);

        compilation = TestHelper.CreateCompilation(TestHelper.DefaultSources.Replace("[GenerateSystemIoExtensions]", ""));
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        Assert.Empty(driver.GetRunResult().GeneratedTrees);
    }

    [Fact]
    public void RemovingARequiredTypeInvalidatesTheCachedApi()
    {
        var compilation = TestHelper.CreateCompilation(TestHelper.DefaultSources);
        var driver = CreateDriver().RunGenerators(compilation, TestContext.Current.CancellationToken);

        driver = driver.RunGenerators(TestHelper.CreateCompilation(
            TestHelper.DefaultSources.Replace("AbsolutePath", "OtherPath")), TestContext.Current.CancellationToken);

        Assert.Empty(driver.GetRunResult().GeneratedTrees);
        Assert.Contains(driver.GetRunResult().Diagnostics, diagnostic => diagnostic.Id == "TPG001");
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);
        Assert.NotEmpty(driver.GetRunResult().GeneratedTrees);
        Assert.Empty(driver.GetRunResult().Diagnostics);
    }

    private static string Docs(string summary) => $"""
        <doc><members>
        <member name="{ReadAllTextId}">
        <summary>{summary}</summary>
        <param name="path">The path.</param>
        </member>
        </members></doc>
        """;

    private static SyntaxTree Parse(string text) => CSharpSyntaxTree.ParseText(text, new CSharpParseOptions(LanguageVersion.Latest));

    private static GeneratorDriver CreateDriver(params AdditionalText[] texts) => CSharpGeneratorDriver.Create(
        new[] { new PathIoSourceGenerator().AsSourceGenerator() }, texts,
        driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));

    private static string Source(GeneratorDriver driver) => driver.GetRunResult().GeneratedTrees.Single().GetText().ToString();

    private static void AssertCached(GeneratorDriver driver, params string[] names)
    {
        var steps = driver.GetRunResult().Results.Single().TrackedSteps;
        foreach (var name in names)
            Assert.All(steps[name].SelectMany(step => step.Outputs), output => Assert.Equal(IncrementalStepRunReason.Cached, output.Reason));
    }

    private sealed class CountingText(string path, string text) : AdditionalText
    {
        public int Reads { get; private set; }
        public override string Path => path;
        public override SourceText GetText(CancellationToken cancellationToken = default)
        {
            Reads++;
            return SourceText.From(text);
        }
    }

    private sealed class MethodDocumentation(string summary) : DocumentationProvider
    {
        protected override string GetDocumentationForSymbol(string documentationMemberID, CultureInfo preferredCulture,
            CancellationToken cancellationToken = default) => documentationMemberID == ReadAllTextId
            ? $"<member name=\"{ReadAllTextId}\"><summary>{summary}</summary></member>" : "";

        public override bool Equals(object? obj) => obj is MethodDocumentation other && summary == other.Summary;
        public override int GetHashCode() => summary.GetHashCode();
        private string Summary => summary;
    }
}
