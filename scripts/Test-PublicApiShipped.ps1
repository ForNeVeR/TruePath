# SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
#
# SPDX-License-Identifier: MIT

<#
.SYNOPSIS
    This script will verify that the public API baselines have no entries left in the Unshipped files. With the Autofix
    switch, it will apply such entries to the Shipped files instead.
#>
param (
    # Path to the repository root.
    $SourceRoot = "$PSScriptRoot/..",

    # Makes the script to move the entries from the Unshipped files to the Shipped files.
    [switch] $Autofix
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# For PowerShell to properly process the UTF-8 output from git ls-files we need to set up the output encoding:
[Console]::OutputEncoding = [Text.Encoding]::UTF8

$nullableEnable = '#nullable enable'
$removedPrefix = '*REMOVED*'

function Get-ApiEntries($Path) {
    $lineNumber = 0
    foreach ($line in [IO.File]::ReadAllLines((Resolve-Path -LiteralPath $Path).Path)) {
        $lineNumber++
        if (![string]::IsNullOrWhiteSpace($line) -and $line -cne $nullableEnable) {
            [pscustomobject] @{ LineNumber = $lineNumber; Text = $line }
        }
    }
}

function Write-ApiFile($Path, $Entries) {
    $text = (@($nullableEnable) + @($Entries) -join "`n") + "`n"
    [IO.File]::WriteAllText((Resolve-Path -LiteralPath $Path).Path, $text, [Text.UTF8Encoding]::new($false))
}

try {
    Push-Location $SourceRoot

    $gitOutput = git ls-files -z --cached -- ':(glob)**/PublicAPI.Unshipped*.txt'
    if ($LASTEXITCODE -ne 0) {
        throw "git ls-files failed with exit code $LASTEXITCODE."
    }

    # Tracked files deleted from the working tree are still listed by Git, so filter them out.
    [array] $files = "$gitOutput".Split("`0", [StringSplitOptions]::RemoveEmptyEntries) |
        Where-Object { Test-Path -LiteralPath $_ }
    if (!$files) {
        throw "No PublicAPI.Unshipped files found in $(Get-Location)."
    }
    Write-Output "Unshipped public API files in the repository: $($files.Length)"

    $errors = @()
    $updates = @()
    foreach ($file in $files) {
        $entries = @(Get-ApiEntries $file)
        if (!$entries.Length) {
            continue
        }

        if (!$Autofix) {
            $errors += $entries | ForEach-Object { "${file}:$($_.LineNumber): $($_.Text)" }
            continue
        }

        $shippedFile = $file -replace '(^|/)PublicAPI\.Unshipped([^/]*)$', '$1PublicAPI.Shipped$2'
        if (!(Test-Path -LiteralPath $shippedFile)) {
            $errors += "${file}: file $shippedFile does not exist."
            continue
        }

        $api = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        foreach ($entry in @(Get-ApiEntries $shippedFile)) {
            [void] $api.Add($entry.Text)
        }

        $removedEntries = @($entries | Where-Object { $_.Text.StartsWith($removedPrefix, [StringComparison]::Ordinal) })
        $addedEntries = @($entries | Where-Object { !$_.Text.StartsWith($removedPrefix, [StringComparison]::Ordinal) })

        foreach ($entry in $removedEntries) {
            if (!$api.Remove($entry.Text.Substring($removedPrefix.Length))) {
                $errors += "${file}:$($entry.LineNumber): no matching entry in ${shippedFile}: $($entry.Text)"
            }
        }
        foreach ($entry in $addedEntries) {
            [void] $api.Add($entry.Text)
        }

        # The same order as the one used by the code fixes of the public API analyzer.
        $sortedApi = [Collections.Generic.List[string]]::new($api)
        $sortedApi.Sort([Comparison[string]] {
            param($x, $y)
            $result = [StringComparer]::OrdinalIgnoreCase.Compare($x, $y)
            if ($result -ne 0) { $result } else { [StringComparer]::Ordinal.Compare($x, $y) }
        })

        $updates += [pscustomobject] @{
            UnshippedFile = $file
            ShippedFile = $shippedFile
            Api = $sortedApi
            Added = $addedEntries.Length
            Removed = $removedEntries.Length
        }
    }

    if ($Autofix -and $errors.Length) {
        throw "Found $($errors.Length) errors, no files were changed:`n" + ($errors -join "`n")
    }
    if ($errors.Length) {
        throw "Found $($errors.Length) unshipped public API entries. Move them to the Shipped files before the release " +
            "by running this script with the -Autofix switch, see MAINTAINING.md:`n" + ($errors -join "`n")
    }

    foreach ($update in $updates) {
        Write-ApiFile $update.ShippedFile $update.Api
        Write-ApiFile $update.UnshippedFile @()
        Write-Output "$($update.ShippedFile): $($update.Added) added, $($update.Removed) removed."
    }
} finally {
    Pop-Location
}
