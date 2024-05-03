using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TruePath.Tests;
public class PathExtensionsTests
{
    [Theory]
    [InlineData("foo/bar.txt", ".txt")]
    [InlineData("/foo/bar.txt", ".txt")]
    [InlineData("foo/bar.", "")]
    [InlineData("foo/bar", null)]
    public void GetExtensionWithDotTests(string path, string? expected)
    {
        IPath sut = new LocalPath(path);
        Assert.Equal(expected,  sut.GetExtensionWithDot());

        //IPath a = new AbsolutePath(path);
        //Assert.Equal(expected, a.GetExtensionWithDot());
    }


    [Theory]
    [InlineData("foo/bar.txt", "txt")]
    [InlineData("/foo/bar.txt", "txt")]
    [InlineData("foo/bar.", "")]
    [InlineData("foo/bar", null)]
    public void GetExtensionWithoutDotTests(string path, string? expected)
    {
        IPath l = new LocalPath(path);
        Assert.Equal(expected, l.GetExtensionWithoutDot());

        //IPath a = new AbsolutePath(path);
        //Assert.Equal(expected, a.GetExtensionWithoutDot());
    }

}
