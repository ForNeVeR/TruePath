// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace TruePath.PathIoGenerator.Tests;

public sealed class CancellationTests
{
    [Fact]
    public void CancellationDuringDocumentationReadStopsTheRunAndAllowsRetry()
    {
        using var cancellation = new CancellationTokenSource();
        var reads = 0;
        var docs = new CallbackText("System.Runtime.xml", token =>
        {
            if (++reads == 1)
            {
                Assert.Equal(cancellation.Token, token);
                cancellation.Cancel();
            }
        });
        var compilation = TestHelper.CreateCompilation(TestHelper.DefaultSources);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new[] { new PathIoSourceGenerator().AsSourceGenerator() }, new[] { docs });

        Assert.Throws<OperationCanceledException>(() => driver.RunGenerators(compilation, cancellation.Token));

        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);
        Assert.NotEmpty(driver.GetRunResult().GeneratedTrees);
        Assert.Empty(driver.GetRunResult().Diagnostics);
    }

    [Fact]
    public void CancellationDuringExclusionReadStopsTheRun()
    {
        using var cancellation = new CancellationTokenSource();
        var exclusions = new CallbackText("path-io-exclusions.txt", token =>
        {
            Assert.Equal(cancellation.Token, token);
            cancellation.Cancel();
        });
        var compilation = TestHelper.CreateCompilation(TestHelper.DefaultSources);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new[] { new PathIoSourceGenerator().AsSourceGenerator() }, new[] { exclusions });

        Assert.Throws<OperationCanceledException>(() => driver.RunGenerators(compilation, cancellation.Token));
    }

    [Fact]
    public void CoreOperationsHonorAnAlreadyCancelledToken()
    {
        var token = new CancellationToken(canceled: true);
        var compilation = TestHelper.CreateCompilation(TestHelper.DefaultSources);
        var markedType = compilation.GetTypeByMetadataName("TruePath.SystemIo.PathIo")!;

        Assert.Throws<OperationCanceledException>(() => BclDocIndex.Build(ImmutableArray<AdditionalText>.Empty, token));
        Assert.Throws<OperationCanceledException>(() => Exclusions.Parse(ImmutableArray<string>.Empty, token));
        Assert.Throws<OperationCanceledException>(() => PathIoGeneratorCore.CollectHandWrittenInfo(compilation, markedType, token));
        Assert.Throws<OperationCanceledException>(() => PathIoGeneratorCore.Generate(
            compilation, "TruePath.SystemIo.PathIo", false, HandWrittenInfo.Empty,
            ImmutableHashSet<string>.Empty, BclDocIndex.Build(ImmutableArray<AdditionalText>.Empty, TestContext.Current.CancellationToken), token));
    }

    private sealed class CallbackText(string path, Action<CancellationToken> callback) : AdditionalText
    {
        public override string Path => path;

        public override SourceText GetText(CancellationToken cancellationToken = default)
        {
            callback(cancellationToken);
            return SourceText.From("\n");
        }
    }
}
