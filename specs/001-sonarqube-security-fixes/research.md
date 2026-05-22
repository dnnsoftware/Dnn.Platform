# Phase 0 Research: SonarQube Security Findings Remediation

**Feature**: 001-sonarqube-security-fixes
**Date**: 2026-05-21

This document resolves the unknowns flagged in `plan.md` Technical Context and records the
rationale for each technology / pattern choice.

## R1. Secure replacement for `Path.GetTempFileName()`

**Decision**: Use the following pattern, encapsulated in `SecureTempFile`:

```csharp
string dir  = Path.GetTempPath();
string name = Path.GetRandomFileName();      // 11-char cryptographically-random
string path = Path.Combine(dir, name);

var stream = new FileStream(
    path,
    FileMode.CreateNew,                      // fail loudly if collision (defensive)
    FileAccess.ReadWrite,
    FileShare.None,                          // exclusive while open
    bufferSize: 4096,
    options: FileOptions.DeleteOnClose);     // OS reaps on Dispose / process exit
```

**Rationale**:
- `Path.GetTempFileName()` (the SonarQube target of S5445) has two problems flagged by
  CWE-377: the filename is taken from a small, predictable counter pool (a remote attacker
  who can predict tempdir contents can pre-create or symlink-attack), AND the API creates a
  zero-byte file on disk with default DACL.
- `Path.GetRandomFileName()` returns a cryptographically-random 11-character name (no file
  creation side-effect). Combined with `FileMode.CreateNew` + `FileShare.None`, the file is
  exclusively owned for its lifetime. `FileOptions.DeleteOnClose` removes the cleanup
  responsibility from callers.
- This is the pattern SonarSource explicitly recommends in the S5445 rule remediation
  guidance.

**Alternatives considered**:
- `Path.GetTempFileName` + immediate rename: still creates the predictable file first.
  Rejected.
- A custom GUID-based name: works but uses 36 chars and is no more secure than
  `GetRandomFileName`. Rejected for simplicity.
- `System.IO.Pipelines` / in-memory `MemoryStream`: would eliminate the temp file entirely,
  but `Cake.XdtTransform` and `log4net`'s file consumers require a real file path.
  Rejected.

## R2. Multi-targeting `net8.0;net48`

**Decision**: New library `DotNetNuke.Security.IO` uses SDK-style csproj with
`<TargetFrameworks>net8.0;net48</TargetFrameworks>`. No conditional compilation is expected —
the chosen API surface (`Path.GetTempPath`, `Path.GetRandomFileName`, `Path.Combine`,
`FileStream(..., FileOptions)`) exists identically on both targets since .NET Framework 4.5+.

**Rationale**:
- `net8.0` satisfies the user's "migrar para .NET 8+" requirement for new code.
- `net48` preserves consumer compatibility for `DNN Platform/Library`, `Syndication`, and the
  log4net library, which cannot move off Web Forms in this PR.
- `FileOptions.DeleteOnClose` is available on net48 (since .NET Framework 4.0) — no
  conditional code needed.

**Alternatives considered**:
- `netstandard2.0` only — would also work and is a single TFM, but the user explicitly asked
  for .NET 8 as the modern target, and dual-targeting makes the .NET 8 build artifact a
  first-class deliverable (visible in `bin/net8.0/`).
- `net8.0` only — breaks net48 consumers. Rejected.

## R3. Test framework & multi-target test execution

**Decision**: xUnit 2.x + FluentAssertions 6.x in a multi-target test project
(`net8.0;net48`). Tests run via `dotnet test` from the solution root.

**Rationale**:
- User explicit choice in `/speckit-plan` clarification.
- xUnit's `[Theory]` + `[InlineData]` is the cleanest way to parametrize "verify behavior is
  identical on net8.0 and net48".
- FluentAssertions improves failure messages, which matters because some scenarios assert on
  filesystem state (file presence, file content) where the default xUnit message ("expected
  true but was false") is not actionable.

**Alternatives considered**:
- NUnit (existing DNN convention) — rejected: user picked xUnit. The divergence is
  contained to one new test project.
- MSTest — fewer features, no compelling reason. Rejected.

## R4. SonarQube verification mechanics

**Decision**: Verification is performed by re-running SonarQube against the feature branch
using the same quality profile that produced the original report. The expected output is two
specific findings cleared and no new findings of severity High or Blocker introduced in
touched files. The verification artifact (SonarQube scan output, screenshot of the rule
status, or text export) is attached to the PR.

**Fallback when SonarQube is not available** (e.g., academic environment without a SonarQube
instance): a manual rule-by-rule check is acceptable:

- For **S5445**: grep the modified files for `Path.GetTempFileName(` — must return zero hits
  outside the new library's own implementation.
- For **S6703**: grep the modified files for `Password=sa`, `Password=<sa>`, and `Password=`
  followed by a non-placeholder value — must return zero hits.

The fallback's limitations (no AST analysis, possible false negatives on transformed code)
are documented in the PR.

**Rationale**: per Constitution Principle V, static-analysis verification is required.
Academic environments may not have SonarQube; the grep fallback retains the spirit of the
principle while staying realistic.

## R5. Where the new library lives in the solution

**Decision**: Add `DotNetNuke.Security.IO` under `DNN Platform/` (matching existing layout)
and reference it from each consumer csproj. Add it to [DNN_Platform.sln](../../DNN_Platform.sln).
The test project goes under `DNN Platform/Tests/DotNetNuke.Security.IO.Tests/` to match the
existing test-project convention.

**Rationale**: minimum surprise for future maintainers, no new top-level directory,
discoverable via solution explorer.

## R6. Backward-compatibility risk for existing call sites

**Decision**: After migration, existing call sites consume `using var temp =
factory.Create();` and pass `temp.Path` to whatever API used to receive the
`GetTempFileName()` return value. The file lifecycle is the same (created up-front, populated
by caller, deleted on dispose) so observable behavior is preserved.

**Rationale**: each migrated method already had a temp file with a known path and contents;
the only thing changing is *how* the path was produced and how cleanup happens. Callers
written with `using` blocks (or `try/finally` for older code) need no semantic change beyond
swapping the call.

**Risk to monitor**: `Cake.XdtTransform`'s `XdtTransformConfig(file, transformFile, file)`
treats `transformFile` as a path string. Our `SecureTempFile.Path` returns the same string,
so this works. A unit test covers this exact contract.
