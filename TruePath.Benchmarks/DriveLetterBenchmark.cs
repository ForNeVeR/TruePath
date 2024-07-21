using BenchmarkDotNet.Attributes;

namespace TruePath.Benchmarks;

public class DriveLetterBenchmark
{
    public static IEnumerable<string> ValuesForInput =>
    [
        "A://", "Z://", "a://", "z://",
        "K://", "k://", "R://", "r://",
        "//", "foobar", 
    ];

    [ParamsSource(nameof(ValuesForInput))] public string Input { get; set; } = null!;

    [Benchmark]
    public bool UseLatinLetterRange()
    {
        return DriveLetter.UseLatinLetterRange(Input);
    }

    [Benchmark]
    public bool UseSearchValues()
    {
        return DriveLetter.UseSearchValues(Input);
    }
}
