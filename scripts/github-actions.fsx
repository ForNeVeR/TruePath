// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

#r "nuget: Generaptor.Library, 1.2.0"

open System
open Generaptor
open Generaptor.GitHubActions
open type Generaptor.GitHubActions.Commands
open type Generaptor.Library.Actions
open type Generaptor.Library.Patterns

let mainBranch = "main"
let ubuntu = "ubuntu-22.04"
let images = [
    "macos-12"
    ubuntu
    "windows-2022"
]


let workflows = [
    let mainTriggers = [
        onPushTo mainBranch
        onPullRequestTo mainBranch
        onSchedule(day = DayOfWeek.Saturday)
        onWorkflowDispatch
    ]

    workflow "main" [
        name "Main"
        yield! mainTriggers

        job "licenses" [
            runsOn ubuntu
            checkout

            step(name = "REUSE license check", uses = "fsfe/reuse-action@v3")
        ]
        job "encoding" [
            runsOn ubuntu
            checkout

            step(name = "Verify encoding", shell = "pwsh", run = "scripts/Test-Encoding.ps1")
        ]

    ]
]

EntryPoint.Process fsi.CommandLineArgs workflows
