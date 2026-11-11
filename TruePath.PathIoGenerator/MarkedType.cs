// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath.PathIoGenerator;

/// <summary>
///     The equatable payload the pipeline computes for every marked class: its display name, whether it is nested
///     (rejected by <see cref="PathIoGeneratorCore.Generate"/> with a diagnostic), and the user-defined member
///     analysis. Being equatable lets the incremental pipeline cache it across unrelated edits.
/// </summary>
internal sealed class MarkedType(string typeName, bool isNested, UserDefinedMembersInfo userDefinedMembers)
{
    public string TypeName { get; } = typeName;
    public bool IsNested { get; } = isNested;
    public UserDefinedMembersInfo UserDefinedMembers { get; } = userDefinedMembers;

    public override bool Equals(object? obj)
    {
        return obj is MarkedType other
               && TypeName == other.TypeName
               && IsNested == other.IsNested
               && UserDefinedMembers.Equals(other.UserDefinedMembers);
    }

    public override int GetHashCode()
    {
        return TypeName.GetHashCode();
    }
}
