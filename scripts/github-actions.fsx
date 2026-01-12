let licenseHeader = """
# SPDX-FileCopyrightText: 2024-2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
#
# SPDX-License-Identifier: MIT

# This file is auto-generated.""".Trim()

#r "nuget: Generaptor.Library, 1.9.0"

open System
open Generaptor
open Generaptor.GitHubActions
open type Generaptor.GitHubActions.Commands
open type Generaptor.Library.Actions
open type Generaptor.Library.Patterns

let mainBranch = "main"
let ubuntu = "ubuntu-24.04"
let images = [
    "macos-15"
    ubuntu
    "windows-2025"
]

let workflows = [
    let mainTriggers = [
        onPushTo mainBranch
        onPushTo "renovate/**"
        onPullRequestTo mainBranch
        onSchedule(day = DayOfWeek.Saturday)
        onWorkflowDispatch
    ]

    let workflow name actions = workflow name [
        header licenseHeader
        yield! actions
    ]

    workflow "main" [
        name "Main"
        yield! mainTriggers

        job "verify-workflows" [
            runsOn "ubuntu-24.04"

            checkout
            step(
                name = "Set up .NET SDK",
                usesSpec = Auto "actions/setup-dotnet"
            )
            step(run = "dotnet fsi ./scripts/github-actions.fsx verify")
        ]

        job "check" [
            checkout
            yield! dotNetBuildAndTest(projectFileExtensions = [".csproj"])
        ] |> addMatrix images

        job "licenses" [
            runsOn ubuntu
            checkout

            step(name = "REUSE license check", usesSpec = Auto "fsfe/reuse-action")
        ]

        job "encoding" [
            runsOn ubuntu
            checkout

            step(name = "Verify encoding", shell = "pwsh", run = "scripts/Test-Encoding.ps1")
        ]

        job "nowarn-empty" [
            runsOn ubuntu
            checkout

            step(name = "Verify with NoWarn as empty", run = "dotnet build /p:NoWarn='' --no-incremental")
        ]
    ]

    workflow "release" [
        name "Release"
        yield! mainTriggers
        onPushTags "v*"
        job "nuget" [
            runsOn ubuntu
            checkout
            jobPermission(PermissionKind.Contents, AccessKind.Write)

            let configuration = "Release"

            let versionStepId = "version"
            let versionField = "${{ steps." + versionStepId + ".outputs.version }}"
            getVersionWithScript(stepId = versionStepId, scriptPath = "scripts/Get-Version.ps1")
            dotNetPack(version = versionField)

            let releaseNotes = "./release-notes.md"
            step(
                name = "Read changelog",
                usesSpec = Auto "ForNeVeR/ChangelogAutomation.action",
                options = Map.ofList [
                    "output", releaseNotes
                ]
            )

            let artifacts projectName includeSNuPkg = [
                let packageId = projectName
                $"./{projectName}/bin/{configuration}/{packageId}.{versionField}.nupkg"
                if includeSNuPkg then $"./{projectName}/bin/{configuration}/{packageId}.{versionField}.snupkg"
            ]
            let allArtifacts = [
                yield! artifacts "TruePath" true
                yield! artifacts "TruePath.SystemIo" true
            ]
            uploadArtifacts [
                releaseNotes
                yield! allArtifacts
            ]
            yield! ifCalledOnTagPush [
                createRelease(
                    name = $"TruePath v{versionField}",
                    releaseNotesPath = releaseNotes,
                    files = allArtifacts
                )
                yield! pushToNuGetOrg "NUGET_TOKEN_TRUE_PATH" (
                    artifacts "TruePath" false
                )
                yield! pushToNuGetOrg "NUGET_TOKEN_TRUE_PATH_SYSTEM_IO" (
                    artifacts "TruePath.SystemIo" false
                )
            ]
        ]
    ]

    workflow "docs" [
        name "Docs"
        onPushTo "main"
        onWorkflowDispatch
        workflowPermission(PermissionKind.Actions, AccessKind.Read)
        workflowPermission(PermissionKind.Pages, AccessKind.Write)
        workflowPermission(PermissionKind.IdToken, AccessKind.Write)
        workflowConcurrency(
            group = "pages",
            cancelInProgress = false
        )
        job "publish-docs" [
            environment(name = "github-pages", url = "${{ steps.deployment.outputs.page_url }}")
            runsOn "ubuntu-24.04"
            step(
                name = "Checkout",
                usesSpec = Auto "actions/checkout"
            )
            step(
                name = "Set up .NET SDK",
                usesSpec = Auto "actions/setup-dotnet",
                options = Map.ofList [
                    "dotnet-version", "8.x"
                ]
            )
            step(
                run = "dotnet tool restore"
            )
            step(
                run = "dotnet docfx docs/docfx.json"
            )
            step(
                name = "Upload artifact",
                usesSpec = Auto "actions/upload-pages-artifact",
                options = Map.ofList [
                    "path", "docs/_site"
                ]
            )
            step(
                name = "Deploy to GitHub Pages",
                id = "deployment",
                usesSpec = Auto "actions/deploy-pages"
            )
        ]
    ]
]

exit <| EntryPoint.Process fsi.CommandLineArgs workflows
