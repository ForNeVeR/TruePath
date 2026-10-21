// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath.SystemIo;

/// <summary>
///     Marks a <see langword="partial"/> <see langword="static"/> class as a generation target: the
///     TruePath.PathIoGenerator source generator will fill the other partial part with forwarding extension methods
///     for every public static method of <c>System.IO.File</c> and <c>System.IO.Directory</c> available in the
///     current target framework. Hand-written members of the marked class win over generated ones.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal sealed class GenerateSystemIoExtensionsAttribute : Attribute;
