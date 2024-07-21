// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.md>
//
// SPDX-License-Identifier: MIT

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace TruePath.Benchmarks;

public class NormalizePathBenchmark
{
    private const int N = 10000;

    public IEnumerable<string> ValuesForInput => new[]
    {
        ".", "./foo", "/a/b/c/d/e/f/g/h/i/j/k/l/m/n/o/p/q/r/s/t/u/v/w/x/y/z", "a/../../.",
        "foo/./sdfaadfsf/safd/../fafdasf/asdfads/adsadsa/das/./../.."
    };

    [ParamsSource(nameof(ValuesForInput))] public string Input { get; set; } = null!;

    [Benchmark]
    public string[] Normalize1()
    {
        var pp = new string[N];
        for (var i = 0; i < N; ++i)
        {
            pp[i] = PathStrings.Normalize1(Input);
        }
        return pp;
    }

    [Benchmark]
    public string[] Normalize2()
    {
        var pp = new string[N];
        for (int i = 0; i < N; ++i)
        {
            pp[i] = PathStrings.Normalize2(Input);
        }
        return pp;
    }

    [Benchmark]
    public string[] Normalize3()
    {
        var pp = new string[N];
        for (int i = 0; i < N; ++i)
        {
            pp[i] = PathStrings.Normalize3(Input);
        }
        return pp;
    }

    [Benchmark]
    public string[] Normalize4()
    {
        var pp = new string[N];
        for (int i = 0; i < N; ++i)
        {
            pp[i] = PathStrings.Normalize4(Input);
        }
        return pp;
    }

    [Benchmark]
    public string[] NormalizeWithWindowsDiskDrive()
    {
        var pp = new string[N];
        for (int i = 0; i < N; ++i)
        {
            pp[i] = PathStrings.NormalizeWithWindowsDiskDrive(Input);
        }
        return pp;
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        _ = BenchmarkRunner.Run<DriveLetterBenchmark>();
    }
}
