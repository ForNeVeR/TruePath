<!--
SPDX-FileCopyrightText: 2024-2026 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

Maintainer Guide
================

Publish a New Version
---------------------
1. Update the project's status in the `README.md` file, if required.
2. Update the copyright statement in the `LICENSE.txt` file, if required.
3. Update the copyright statement in the `Directory.Build.props` file, if required.
4. Prepare a corresponding entry in the `CHANGELOG.md` file (usually by renaming the "Unreleased" section).
5. Set `<Version>` in the `Directory.Build.props` file.
6. Merge the aforementioned changes via a pull request.
7. If you want to publish a new package (not a new version, but a new project/package) change the NuGet Trusted Publishing Policy: switch from **push only new packages versions** to **push new packages and package versions** (see below for more details).
8. Push a tag in form of `v<VERSION>`, e.g. `v0.0.0`. GitHub Actions will do the rest (push a NuGet packages).
9. If you were publishing a new package, revert back the change you did at step 8.

NuGet Publishing Policy
-----------------------
This repository relies on [NuGet Trusted Publishing][docs.nuget-trusted-publishing] policy. In case you need to create it again, follow these steps:

1. Sign in to nuget.org.
2. Go to the [Trusted Publishing][nuget.trusted-publishing] section.
3. Create a new policy:
    - **Policy Name:** `github.truepath`.
    - **Repository Owner:** `ForNeVeR`.
    - **Repository**: `TruePath`.
    - **Workflow File**: `release.yml`.
    - **Environment:** _leave empty_.
    - **Scope:** **push only new packages versions**.
    - **Glob Patterns and Packages**: `TruePath*`.
4. Put your nuget.org username into the `NUGET_USER` variable on the [action secrets][github.secrets] section of GitHub settings.

When introducing a new package, you can adjust the policy temporarily to allow it to **push new packages and package versions**.

[docs.nuget-trusted-publishing]: https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing
[github.secrets]: https://github.com/ForNeVeR/TruePath/settings/secrets/actions
[nuget.trusted-publishing]: https://www.nuget.org/account/trustedpublishing
