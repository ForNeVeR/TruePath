// SPDX-FileCopyrightText: 2026 TruePath contributors <https://github.com/ForNeVeR/TruePath>
//
// SPDX-License-Identifier: MIT

namespace TruePath.Tests;

/// <summary>
/// A collection for tests that change the process current directory. Tests in it run with nothing else running in
/// parallel, so tests anywhere else can safely read the current directory.
/// </summary>
/// <remarks>
/// Tests that only read the current directory don't belong here, even if they depend on its exact value: the tests in
/// this collection restore the current directory after themselves, so it stays the same for any other test.
/// </remarks>
[CollectionDefinition(DisableParallelization = true)]
public class CurrentDirectoryCollection;
