using System.Runtime.Versioning;
using System.Text;

namespace TruePath;

public static class AbsoluteFile
{
    public static void AppendAllLines(string path, IEnumerable<string> contents) => File.AppendAllLines(path, contents);
    public static void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding) => File.AppendAllLines(path, contents, encoding);
    public static Task AppendAllLinesAsync(AbsolutePath path, IEnumerable<string> contents, CancellationToken cancellationToken = default) => File.AppendAllLinesAsync(path.Value, contents, cancellationToken);
    public static Task AppendAllLinesAsync(AbsolutePath path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default) => File.AppendAllLinesAsync(path.Value, contents, encoding, cancellationToken);
    public static void AppendAllText(AbsolutePath path, string? contents) => File.AppendAllText(path.Value, contents);
    public static void AppendAllText(AbsolutePath path, string? contents, Encoding encoding) => File.AppendAllText(path.Value, contents, encoding);
    public static Task AppendAllTextAsync(AbsolutePath path, string? contents, CancellationToken cancellationToken = default) => File.AppendAllTextAsync(path.Value, contents, cancellationToken);
    public static Task AppendAllTextAsync(AbsolutePath path, string? contents, Encoding encoding, CancellationToken cancellationToken = default) => File.AppendAllTextAsync(path.Value, contents, encoding, cancellationToken);
    public static StreamWriter AppendText(AbsolutePath path) => File.AppendText(path.Value);
    public static StreamWriter CreateText(AbsolutePath path) => File.CreateText(path.Value);
    public static void Copy(AbsolutePath sourceFile, AbsolutePath destFile) => File.Copy(sourceFile.Value, destFile.Value);
    public static void Copy(AbsolutePath sourceFile, AbsolutePath destFile, bool overwrite) => File.Copy(sourceFile.Value, destFile.Value, overwrite);
    public static FileStream Create(AbsolutePath path) => File.Create(path.Value);
    public static FileStream Create(AbsolutePath path, int bufferSize) => File.Create(path.Value, bufferSize);
    public static FileStream Create(AbsolutePath path, int bufferSize, FileOptions options) => File.Create(path.Value, bufferSize, options);
    public static void Delete(AbsolutePath path) => File.Delete(path.Value);
    public static bool Exists(AbsolutePath path) => File.Exists(path.Value);
    public static FileAttributes GetAttributes(AbsolutePath path) => File.GetAttributes(path.Value);
    public static DateTime GetCreationTime(AbsolutePath path) => File.GetCreationTime(path.Value);
    public static DateTime GetCreationTimeUtc(AbsolutePath path) => File.GetCreationTimeUtc(path.Value);
    public static DateTime GetLastAccessTime(AbsolutePath path) => File.GetLastAccessTime(path.Value);
    public static DateTime GetLastAccessTimeUtc(AbsolutePath path) => File.GetLastAccessTimeUtc(path.Value);
    public static DateTime GetLastWriteTime(AbsolutePath path) => File.GetLastWriteTime(path.Value);
    [UnsupportedOSPlatform("windows")]
    public static UnixFileMode GetUnixFileMode(AbsolutePath path) => File.GetUnixFileMode(path.Value);
    public static void Move(AbsolutePath sourceFile, AbsolutePath destFile) => File.Move(sourceFile.Value, destFile.Value);
    public static void Move(AbsolutePath sourceFile, AbsolutePath destFile, bool overwrite) => File.Move(sourceFile.Value, destFile.Value, overwrite);
    public static FileStream Open(AbsolutePath path, FileMode mode) => File.Open(path.Value, mode);
    public static FileStream Open(AbsolutePath path, FileMode mode, FileAccess access) => File.Open(path.Value, mode, access);
    public static FileStream Open(AbsolutePath path, FileMode mode, FileAccess access, FileShare share) => File.Open(path.Value, mode, access, share);
    public static FileStream OpenRead(AbsolutePath path) => File.OpenRead(path.Value);
    public static StreamReader OpenText(AbsolutePath path) => File.OpenText(path.Value);
    public static FileStream OpenWrite(AbsolutePath path) => File.OpenWrite(path.Value);
    public static byte[] ReadAllBytes(AbsolutePath path) => File.ReadAllBytes(path.Value);
    public static Task<byte[]> ReadAllBytesAsync(AbsolutePath path, CancellationToken cancellationToken = default) => File.ReadAllBytesAsync(path.Value, cancellationToken);
    public static string[] ReadAllLines(AbsolutePath path) => File.ReadAllLines(path.Value);
    public static string[] ReadAllLines(AbsolutePath path, Encoding encoding) => File.ReadAllLines(path.Value, encoding);
    public static IEnumerable<string> ReadLines(AbsolutePath path) => File.ReadLines(path.Value);
    public static IEnumerable<string> ReadLines(AbsolutePath path, Encoding encoding) => File.ReadLines(path.Value, encoding);
    public static IAsyncEnumerable<string> ReadLinesAsync(AbsolutePath path, CancellationToken cancellationToken = default) => File.ReadLinesAsync(path.Value, cancellationToken);
    public static IAsyncEnumerable<string> ReadLinesAsync(AbsolutePath path, Encoding encoding, CancellationToken cancellationToken = default) => File.ReadLinesAsync(path.Value, encoding, cancellationToken);
    public static void SetAttributes(AbsolutePath path, FileAttributes fileAttributes) => File.SetAttributes(path.Value, fileAttributes);
    public static void SetCreationTime(AbsolutePath path, DateTime creationTime) => File.SetCreationTime(path.Value, creationTime);
    public static void SetCreationTimeUtc(AbsolutePath path, DateTime creationTimeUtc) => File.SetCreationTimeUtc(path.Value, creationTimeUtc);
    public static void SetLastAccessTime(AbsolutePath path, DateTime lastAccessTime) => File.SetLastAccessTime(path.Value, lastAccessTime);
    public static void SetLastAccessTimeUtc(AbsolutePath path, DateTime lastAccessTimeUtc) => File.SetLastAccessTimeUtc(path.Value, lastAccessTimeUtc);
    public static void SetLastWriteTime(AbsolutePath path, DateTime lastWriteTime) => File.SetLastWriteTime(path.Value, lastWriteTime);
    public static void SetLastWriteTimeUtc(AbsolutePath path, DateTime lastWriteTimeUtc) => File.SetLastWriteTimeUtc(path.Value, lastWriteTimeUtc);
    public static void WriteAllBytes(AbsolutePath path, byte[] bytes) => File.WriteAllBytes(path.Value, bytes);
    public static Task WriteAllBytesAsync(AbsolutePath path, byte[] bytes, CancellationToken cancellationToken = default) => File.WriteAllBytesAsync(path.Value, bytes, cancellationToken);
    public static void WriteAllLines(AbsolutePath path, IEnumerable<string> contents) => File.WriteAllLines(path.Value, contents);
    public static void WriteAllLines(AbsolutePath path, IEnumerable<string> contents, Encoding encoding) => File.WriteAllLines(path.Value, contents, encoding);
    public static void WriteAllLines(AbsolutePath path, string[] contents) => File.WriteAllLines(path.Value, contents);
    public static void WriteAllLines(AbsolutePath path, string[] contents, Encoding encoding) => File.WriteAllLines(path.Value, contents, encoding);
    public static Task WriteAllLinesAsync(AbsolutePath path, IEnumerable<string> contents, CancellationToken cancellationToken = default) => File.WriteAllLinesAsync(path.Value, contents, cancellationToken);
    public static Task WriteAllLinesAsync(AbsolutePath path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default) => File.WriteAllLinesAsync(path.Value, contents, encoding, cancellationToken);
    public static void WriteAllText(AbsolutePath path, string? contents) => File.WriteAllText(path.Value, contents);
    public static void WriteAllText(AbsolutePath path, string? contents, Encoding encoding) => File.WriteAllText(path.Value, contents, encoding);
    public static Task WriteAllTextAsync(AbsolutePath path, string? contents, CancellationToken cancellationToken = default) => File.WriteAllTextAsync(path.Value, contents, cancellationToken);
    public static Task WriteAllTextAsync(AbsolutePath path, string? contents, Encoding encoding, CancellationToken cancellationToken = default) => File.WriteAllTextAsync(path.Value, contents, encoding, cancellationToken);
}
