// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

namespace TruePath.Tests;

public class GenericInterfaceTests
{
    [Fact]
    public void ParentTests()
    {
        IPath l = new LocalPath("foo/bar");
        IPath a = new AbsolutePath("/foo/bar");

        Assert.Equal("foo", l.Parent?.FileName);
        Assert.Equal("foo", a.Parent?.FileName);
    }

    [Fact]
    public void FileNameTests()
    {
        IPath l = new LocalPath("foo/bar");
        IPath a = new AbsolutePath("/foo/bar");

        Assert.Equal("bar", l.FileName);
        Assert.Equal("bar", a.FileName);
    }

    [Fact]
    public void OperatorTests()
    {
        var l = new LocalPath("foo/bar");
        var a = new AbsolutePath("/foo/bar");
        var fragment = new LocalPath("frog1");

        Assert.Equal("frog1", AppendGeneric(l, fragment).FileName);
        Assert.Equal("frog1", AppendGeneric(a, fragment).FileName);

        Assert.Equal("frog2", AppendGeneric(l, "frog2").FileName);
        Assert.Equal("frog2", AppendGeneric(a, "frog2").FileName);
    }

    private static TPath AppendGeneric<TPath>(TPath basePath, LocalPath appended) where TPath : IPath<TPath> =>
        basePath / appended;

    private static TPath AppendGeneric<TPath>(TPath basePath, string appended) where TPath : IPath<TPath> =>
        basePath / appended;
}
