// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using TruePath.Comparers;

namespace TruePath.Tests;

public class PlatformDefaultPathComparerTests
{
    [Fact]
    public void DefaultStringComparisonMatchesRuntime()
    {
        var isCaseInsensitive = OperatingSystem.IsWindows()
                                || OperatingSystem.IsMacOS()
                                || OperatingSystem.IsIOS()
                                || OperatingSystem.IsTvOS();
        var expected = isCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        Assert.Equal(expected, PlatformDefaultPathComparer.DefaultStringComparison);
    }
}
