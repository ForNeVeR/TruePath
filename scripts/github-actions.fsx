let licenseHeader = """
# SPDX-FileCopyrightText: 2024-2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
#
# SPDX-License-Identifier: MIT

# This file is auto-generated.""".Trim()

#r "nuget: Generaptor, 1.11.0"

open System
open Generaptor
open Generaptor.GitHubActions
open type Generaptor.GitHubActions.Commands

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

    let checkOut = step(
        name = "Check out the sources",
        usesSpec = Auto "actions/checkout"
    )

    let dotNetJob id steps =
        job id [
            setEnv "DOTNET_CLI_TELEMETRY_OPTOUT" "1"
            setEnv "DOTNET_NOLOGO" "1"
            setEnv "NUGET_PACKAGES" "${{ github.workspace }}/.github/nuget-packages"

            checkOut
            step(
                name = "Set up .NET SDK",
                usesSpec = Auto "actions/setup-dotnet"
            )
            step(
                name = "Cache NuGet packages",
                usesSpec = Auto "actions/cache",
                options = Map.ofList [
                    "key", "${{ runner.os }}.nuget.${{ hashFiles('**/*.*proj', '**/*.props') }}"
                    "path", "${{ env.NUGET_PACKAGES }}"
                ]
            )

            yield! steps
        ]

    workflow "main" [
        name "Main"
        yield! mainTriggers

        dotNetJob "verify-workflows" [
            runsOn "ubuntu-24.04"
            step(run = "dotnet fsi ./scripts/github-actions.fsx verify")
        ]

        dotNetJob "check" [
            strategy(failFast = false, matrix = [
                "image", [
                    "macos-15"
                    "ubuntu-24.04"
                    "ubuntu-24.04-arm"
                    "windows-11-arm"
                    "windows-2025"
                ]
            ])
            runsOn "${{ matrix.image }}"

            step(
                name = "Build",
                run = "dotnet build"
            )
            step(
                name = "Test",
                run = "dotnet test",
                timeoutMin = 10
            )
        ]

        job "licenses" [
            runsOn ubuntu
            checkOut

            step(name = "REUSE license check", usesSpec = Auto "fsfe/reuse-action")
        ]

        job "encoding" [
            runsOn ubuntu
            checkOut

            step(name = "Verify encoding", shell = "pwsh", run = "scripts/Test-Encoding.ps1")
        ]

        dotNetJob "nowarn-empty" [
            runsOn ubuntu
            checkOut

            step(name = "Verify with NoWarn as empty", run = "dotnet build /p:NoWarn='' --no-incremental")
        ]
    ]

    workflow "release" [
        name "Release"
        yield! mainTriggers
        onPushTags "v*"
        dotNetJob "nuget" [
            runsOn ubuntu
            jobPermission(PermissionKind.Contents, AccessKind.Write)

            let configuration = "Release"

            let versionStepId = "version"
            let versionField = "${{ steps." + versionStepId + ".outputs.version }}"
            step(
                id = "version",
                name = "Get version",
                shell = "pwsh",
                run = "echo \"version=$(scripts/Get-Version.ps1 -RefName $env:GITHUB_REF)\" >> $env:GITHUB_OUTPUT"
            )
            step(
                run = "dotnet pack --configuration Release -p:Version=${{ steps.version.outputs.version }}"
            )

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

            step(
                name = "Upload artifacts",
                usesSpec = Auto "actions/upload-artifact",
                options = Map.ofList [
                    "path", [ releaseNotes; yield! allArtifacts ] |> String.concat "\n"
                ]
            )

            step(
                condition = "startsWith(github.ref, 'refs/tags/v')",
                name = "Create a release",
                usesSpec = Auto "softprops/action-gh-release",
                options = Map.ofList [
                    "body_path", "./release-notes.md"
                    "files", allArtifacts |> String.concat "\n"
                    "name", $"TruePath v{versionField}"
                ]
            )
            step(
                condition = "startsWith(github.ref, 'refs/tags/v')",
                name = "Push TruePath to NuGet",
                run = $"dotnet nuget push ./TruePath/bin/Release/TruePath.{versionField}.nupkg --source https://api.nuget.org/v3/index.json --api-key ${{{{ secrets.NUGET_TOKEN_TRUE_PATH }}}}"
            )
            step(
                condition = "startsWith(github.ref, 'refs/tags/v')",
                name = "Push TruePath.SystemIo to NuGet",
                run = $"dotnet nuget push ./TruePath.SystemIo/bin/Release/TruePath.SystemIo.{versionField}.nupkg --source https://api.nuget.org/v3/index.json --api-key ${{{{ secrets.NUGET_TOKEN_TRUE_PATH_SYSTEM_IO }}}}"
            )
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
        dotNetJob "publish-docs" [
            environment(name = "github-pages", url = "${{ steps.deployment.outputs.page_url }}")
            runsOn "ubuntu-24.04"
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
