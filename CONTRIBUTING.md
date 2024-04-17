<!--
SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

Contributor Guide
=================

Prerequisites
-------------
To work with the project, you'll need [.NET SDK 8][dotnet-sdk] or later.

Build
-----
Use the following shell command:

```console
$ dotnet build
```

Pack
----
To prepare a NuGet package with the tool, use the following shell command:

```console
$ dotnet pack
```

License Automation
------------------
If the CI asks you to update the file licenses, follow one of these:
1. Update the headers manually (look at the existing files), something like this:
   ```fsharp
   // SPDX-FileCopyrightText: %year% %your name% <%your contact info, e.g. email%>
   //
   // SPDX-License-Identifier: MIT
   ```
   (accommodate to the file's comment style if required).
2. Alternately, use [REUSE][reuse] tool:
   ```console
   $ reuse annotate --license MIT --copyright '%your name% <%your contact info, e.g. email%>' %file names to annotate%
   ```

(Feel free to attribute the changes to "TruePath Authors <https://github.com/ForNeVeR/TruePath>" instead of your name in a multi-author file, or if you don't want your name to be mentioned in the project's source: this doesn't mean you'll lose the copyright.)

GitHub Actions
--------------
If you want to update the GitHub Actions used in the project, edit the file that generated them: `scripts/github-actions.fsx`.

Then run the following shell command:
```console
$ dotnet fsi scripts/github-actions.fsx
```

[dotnet-sdk]: https://dotnet.microsoft.com/en-us/download
[reuse]: https://reuse.software/
