// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

#r "nuget: Generaptor.Library, 1.2.0"

open Generaptor
open Generaptor.GitHubActions
open type Generaptor.GitHubActions.Commands
open type Generaptor.Library.Actions
open type Generaptor.Library.Patterns

let mainBranch = "main"

let workflows = [
    workflow "main" [
        name "Main"
        onPushTo mainBranch
        onPullRequestTo mainBranch
        onWorkflowDispatch

        job "licenses" [
            runsOn "ubuntu-22.04"
            checkout

            step(name = "REUSE license check", uses = "fsfe/reuse-action@v3")
        ]
    ]
]

EntryPoint.Process fsi.CommandLineArgs workflows
