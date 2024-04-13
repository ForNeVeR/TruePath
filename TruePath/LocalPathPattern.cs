// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

﻿namespace TruePath;

/// <summary>
/// <para>An opaque pattern that may be checked if it corresponds to a local path.</para>
/// <para>
///     This is a token type, created with an idea that it should be interpreted in usage-specific way by some external
///     means.
/// </para>
/// </summary>
/// <param name="Value">
/// The pattern text. May be of various formats: say, a glob. This library doesn't dictate any particular format and
/// gives no guarantees.
/// </param>
public record struct LocalPathPattern(string Value)
{
    public override string ToString() => Value;
}
