namespace TruePath;

public static class AbsoluteDirectory
{
    public static void CreateDirectory(AbsolutePath path) => Directory.CreateDirectory(path.Value);
    public static void Delete(AbsolutePath path) => Directory.Delete(path.Value);
    public static void Delete(AbsolutePath path, bool recursive) => Directory.Delete(path.Value, recursive);
    public static AbsolutePath GetCurrentDirectory() => new(Directory.GetCurrentDirectory(), false);
    public static void SetCurrentDirectory(AbsolutePath path) => Directory.SetCurrentDirectory(path.Value);
    public static DateTime GetCreationTime(AbsolutePath path) => Directory.GetCreationTime(path.Value);
    public static DateTime GetCreationTimeUtc(AbsolutePath path) => Directory.GetCreationTimeUtc(path.Value);
    public static DateTime GetLastAccessTime(AbsolutePath path) => Directory.GetLastAccessTime(path.Value);
    public static DateTime GetLastAccessTimeUtc(AbsolutePath path) => Directory.GetLastAccessTimeUtc(path.Value);
    public static DateTime GetLastWriteTime(AbsolutePath path) => Directory.GetLastWriteTime(path.Value);
    public static void SetCreationTime(AbsolutePath path, DateTime creationTime) => Directory.SetCreationTime(path.Value, creationTime);
    public static void SetCreationTimeUtc(AbsolutePath path, DateTime creationTimeUtc) => Directory.SetCreationTimeUtc(path.Value, creationTimeUtc);
    public static void SetLastAccessTime(AbsolutePath path, DateTime lastAccessTime) => Directory.SetLastAccessTime(path.Value, lastAccessTime);
    public static void SetLastAccessTimeUtc(AbsolutePath path, DateTime lastAccessTimeUtc) => Directory.SetLastAccessTimeUtc(path.Value, lastAccessTimeUtc);
    public static void SetLastWriteTime(AbsolutePath path, DateTime lastWriteTime) => Directory.SetLastWriteTime(path.Value, lastWriteTime);
    public static void SetLastWriteTimeUtc(AbsolutePath path, DateTime lastWriteTimeUtc) => Directory.SetLastWriteTimeUtc(path.Value, lastWriteTimeUtc);
}
