// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;

namespace TruePath.PathIoGenerator;

// Locations follow the current syntax tree in a separate diagnostic branch, never the source rendering branch.
internal sealed class MarkedTypeAnalysis(MarkedType type, Location location)
{
    public MarkedType Type { get; } = type;
    public Location Location { get; } = location;
}
