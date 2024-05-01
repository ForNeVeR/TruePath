using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TruePath.Tests;
public class PathExtensionsTests
{
    [Fact]
    public void GetExtensionTests()
    {
        IPath l = new LocalPath("foo/bar.txt");
        IPath a = new AbsolutePath("/foo/bar.txt");

        Assert.Equal(".txt", l.GetExtension());
        Assert.Equal(".txt", a.GetExtension());
    }

    [Fact]
    public void GetExtension_OnInvalidFile_ThrowsArgumentException()
    {
        IPath a = new AbsolutePath("/foo/bar");
        string expectedMessage = "bar is not a file";
        var ex = Assert.Throws<ArgumentException>(() => a.GetExtension());
        Assert.Equal(expectedMessage, ex.Message);


        IPath l = new LocalPath("foo/bar");
        ex = Assert.Throws<ArgumentException>(() => l.GetExtension());
        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void GetExtensionWithoutDotTests()
    {
        IPath l = new LocalPath("foo/bar.txt");
        IPath a = new AbsolutePath("/foo/bar.txt");

        Assert.Equal("txt", l.GetExtensionWithoutDot());
        Assert.Equal("txt", a.GetExtensionWithoutDot());
    }

    [Fact]
    public void GetExtensionWithoutDot_FileWithoutExtension_ThrowsArgumentException()
    {
        IPath a = new AbsolutePath("/foo/bar");
        string expectedMessage = "bar is not a file";
        var ex = Assert.Throws<ArgumentException>(() => a.GetExtensionWithoutDot());
        Assert.Equal(expectedMessage, ex.Message);

        IPath l = new LocalPath("foo/bar");
        ex = Assert.Throws<ArgumentException>(() => l.GetExtensionWithoutDot());
        Assert.Equal(expectedMessage, ex.Message);
    }
}
