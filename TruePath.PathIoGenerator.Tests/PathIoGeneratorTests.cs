// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using TruePath.PathIoGenerator;
using Xunit;

namespace TruePath.PathIoGenerator.Tests;

public sealed class InMemoryAdditionalText(string path, string text) : AdditionalText
{
    public override string Path { get; } = path;
    public override SourceText? GetText(CancellationToken cancellationToken = default) => SourceText.From(text, Encoding.UTF8);
}

public static class TestHelper
{
    public const string DefaultSources = """
        using System;
        #nullable enable
        namespace TruePath
        {
            public readonly struct AbsolutePath
            {
                public AbsolutePath(string value) { }
                public string Value => "";
            }
        }

        namespace TruePath.SystemIo
        {
            [AttributeUsage(AttributeTargets.Class)]
            internal sealed class GenerateSystemIoExtensionsAttribute : Attribute { }

            [GenerateSystemIoExtensions]
            public static partial class PathIo
            {
            }
        }
        """;

    public static string SourcesWithHandWrittenMembers(string members) => DefaultSources
        .Replace(
            """
                [GenerateSystemIoExtensions]
                public static partial class PathIo
                {
                }
            """,
            """
                [GenerateSystemIoExtensions]
                public static partial class PathIo
                {
                """ + members + """
                }
            """);

    public static CSharpCompilation CreateCompilation(string sources)
    {
        // Reference the .NET reference pack (the same contract assemblies a real build uses). Runtime-directory
        // assemblies cannot be used instead: most public types are implemented in System.Private.CoreLib, whose
        // members are not visible when it is referenced directly.
        var coreLibPath = typeof(object).Assembly.Location;
        var dotnetRoot = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(coreLibPath)!, "..", "..", ".."));
        var refDirectory = Directory.GetDirectories(Path.Combine(dotnetRoot, "packs", "Microsoft.NETCore.App.Ref"))
            .SelectMany(versionDirectory => Directory.GetDirectories(Path.Combine(versionDirectory, "ref")))
            .SelectMany(tfmDirectory => Directory.EnumerateFiles(tfmDirectory, "*.dll"))
            .ToArray();
        var references = refDirectory
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToArray();

        return CSharpCompilation.Create(
            "TestAssembly",
            new[] { CSharpSyntaxTree.ParseText(sources, new CSharpParseOptions(LanguageVersion.Latest)) },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    public static string Generate(CSharpCompilation compilation, params AdditionalText[] additionalTexts)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new[] { new PathIoSourceGenerator().AsSourceGenerator() },
            additionalTexts.ToImmutableArray());
        driver = driver.RunGenerators(compilation);
        return driver.GetRunResult().GeneratedTrees
            .Single(tree => tree.FilePath.Contains("PathIo.Generated"))
            .GetText().ToString();
    }
}

public sealed class PathIoGeneratorTests
{
    private const string BclDocs = """
        <doc>
            <assembly><name>System.Runtime</name></assembly>
            <members>
                <member name="M:System.IO.File.ReadAllText(System.String)">
                    <summary>OPENS A TEXT FILE, READS ALL THE TEXT IN THE FILE.</summary>
                    <param name="path">The file to open for reading.</param>
                </member>
                <member name="M:System.IO.Directory.Delete(System.String,System.Boolean)">
                    <summary>SHOULD NEVER BE GENERATED BECAUSE OF THE EXCLUSION RULES.</summary>
                </member>
            </members>
        </doc>
        """;

    [Fact]
    public void PureForwardMethodIsGenerated()
    {
        var source = TestHelper.Generate(TestHelper.CreateCompilation(TestHelper.DefaultSources));

        Assert.Contains(
            "public static string ReadAllText(this AbsolutePath path) => File.ReadAllText(path.Value);",
            source);
    }

    [Fact]
    public void BclDocIndexFindsBclMembers()
    {
        var index = BclDocIndex.Build(
            ImmutableArray.Create<AdditionalText>(new InMemoryAdditionalText("/ref/System.Runtime.xml", BclDocs)));

        Assert.True(index.TryGetMethodXml("M:System.IO.File.ReadAllText(System.String)", out var memberXml));
        Assert.Contains("OPENS A TEXT FILE, READS ALL THE TEXT IN THE FILE.", memberXml);
    }

    [Fact]
    public void CoreUsesBclDocIndexDirectly()
    {
        var compilation = TestHelper.CreateCompilation(TestHelper.DefaultSources);
        var index = BclDocIndex.Build(
            ImmutableArray.Create<AdditionalText>(new InMemoryAdditionalText("/ref/System.Runtime.xml", BclDocs)));

        var result = PathIoGeneratorCore.Generate(
            compilation, "TruePath.SystemIo.PathIo", ImmutableHashSet<string>.Empty, index);

        Assert.Contains("/// <summary>OPENS A TEXT FILE, READS ALL THE TEXT IN THE FILE.</summary>", result.Source);
    }

    [Fact]
    public void DocumentationFromBclXmlIsUsedWhenAvailable()
    {
        var compilation = TestHelper.CreateCompilation(TestHelper.DefaultSources);
        var source = TestHelper.Generate(compilation, new InMemoryAdditionalText("/ref/System.Runtime.xml", BclDocs));

        Assert.Contains("/// <summary>OPENS A TEXT FILE, READS ALL THE TEXT IN THE FILE.</summary>", source);
        Assert.Contains("/// <param name=\"path\">The file to open for reading.</param>", source);
    }

    [Fact]
    public void FallbackDocumentationIsUsedWhenNoXmlIsAvailable()
    {
        var source = TestHelper.Generate(TestHelper.CreateCompilation(TestHelper.DefaultSources));

        Assert.Contains(
            "/// <summary>Equivalent to <see cref=\"System.IO.File.ReadAllText(System.String)\"/>.</summary>",
            source);
    }

    [Fact]
    public void HandWrittenSignatureSuppressesGeneration()
    {
        const string members = """
            public static bool Exists(TruePath.AbsolutePath path) => true;
            """;
        var source = TestHelper.Generate(
            TestHelper.CreateCompilation(TestHelper.SourcesWithHandWrittenMembers(members)));

        Assert.DoesNotContain("Exists(", source);
    }

    [Fact]
    public void HandWrittenForwardingCallSuppressesItsBclTarget()
    {
        const string members = """
            public static void DeleteEmptyDirectory(TruePath.AbsolutePath path) => System.IO.Directory.Delete(path.Value, recursive: false);
            public static void DeleteDirectoryRecursively(TruePath.AbsolutePath path) => System.IO.Directory.Delete(path.Value, recursive: true);
            """;
        var source = TestHelper.Generate(
            TestHelper.CreateCompilation(TestHelper.SourcesWithHandWrittenMembers(members)));

        Assert.DoesNotContain("Delete(AbsolutePath path, bool", source);
        Assert.Contains(
            "public static void Delete(this AbsolutePath path) => File.Delete(path.Value);",
            source);
    }

    [Fact]
    public void ExplicitExclusionListSuppressesGeneration()
    {
        var compilation = TestHelper.CreateCompilation(TestHelper.DefaultSources);
        var exclusions = new InMemoryAdditionalText(
            "/project/path-io-exclusions.txt",
            "M:System.IO.File.Encrypt(System.String)\n");
        var source = TestHelper.Generate(compilation, exclusions);

        Assert.DoesNotContain("Encrypt(", source);
    }

    [Fact]
    public void FileWinsOverDirectoryForDuplicateSignatures()
    {
        var source = TestHelper.Generate(TestHelper.CreateCompilation(TestHelper.DefaultSources));

        Assert.Contains(
            "public static DateTime GetCreationTime(this AbsolutePath path) => File.GetCreationTime(path.Value);",
            source);
        Assert.DoesNotContain("=> Directory.GetCreationTime(", source);
    }

    [Fact]
    public void PlatformAttributesAreForwarded()
    {
        var source = TestHelper.Generate(TestHelper.CreateCompilation(TestHelper.DefaultSources));

        Assert.Contains("[UnsupportedOSPlatformAttribute(\"windows\")]", source);
    }

    [Fact]
    public void HistoricalParameterNamesAreKeptForFileNameParameters()
    {
        var source = TestHelper.Generate(TestHelper.CreateCompilation(TestHelper.DefaultSources));

        Assert.Contains(
            "public static void Copy(this AbsolutePath sourceFile, AbsolutePath destFile) => File.Copy(sourceFile.Value, destFile.Value);",
            source);
        Assert.Contains(
            "public static void Copy(this AbsolutePath sourceFile, AbsolutePath destFile, bool overwrite)",
            source);
    }

    [Fact]
    public void BclDocumentationTextFollowsRenamedParameters()
    {
        var source = TestHelper.Generate(
            TestHelper.CreateCompilation(TestHelper.DefaultSources),
            new InMemoryAdditionalText("/ref/System.Runtime.xml", """
                <doc>
                    <members>
                        <member name="M:System.IO.File.Copy(System.String,System.String)">
                            <summary>Copies an existing file to a new file.</summary>
                            <param name="sourceFileName">The file to copy.</param>
                            <param name="destFileName">The name of the destination file, see <paramref name="sourceFileName" />.</param>
                        </member>
                    </members>
                </doc>
                """));

        Assert.Contains("/// <summary>Copies an existing file to a new file.</summary>", source);
        Assert.Contains("/// <param name=\"sourceFile\">The file to copy.</param>", source);
        Assert.Contains("<paramref name=\"sourceFile\"/>", source);
        Assert.DoesNotContain("sourceFileName", source);
    }

    [Fact]
    public void GeneratedCodeCompilesWithoutErrors()
    {
        var compilation = TestHelper.CreateCompilation(TestHelper.DefaultSources);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new[] { new PathIoSourceGenerator().AsSourceGenerator() },
            ImmutableArray<AdditionalText>.Empty);
        driver = driver.RunGenerators(compilation, Xunit.TestContext.Current.CancellationToken);
        var updated = compilation.AddSyntaxTrees(driver.GetRunResult().GeneratedTrees);

        Assert.Empty(updated.GetDiagnostics(Xunit.TestContext.Current.CancellationToken)
            .Where(d => d.Severity == DiagnosticSeverity.Error));
    }

    [Fact]
    public void MethodsWithoutPathParametersAreListedAsSkipped()
    {
        var source = TestHelper.Generate(TestHelper.CreateCompilation(TestHelper.DefaultSources));

        Assert.Contains("M:System.IO.Directory.GetCurrentDirectory", source);
        Assert.Contains("M:System.IO.Directory.GetLogicalDrives", source);
    }

    private static int CountOccurrences(string text, string pattern)
    {
        var count = 0;
        var position = 0;
        while ((position = text.IndexOf(pattern, position, StringComparison.Ordinal)) >= 0)
        {
            count++;
            position += pattern.Length;
        }
        return count;
    }
}
