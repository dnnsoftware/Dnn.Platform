---

description: "Task list for feature 001-sonarqube-security-fixes"
---

# Tasks: SonarQube Security Findings Remediation

**Input**: Design documents from [/specs/001-sonarqube-security-fixes/](./)

**Prerequisites**:
[plan.md](plan.md), [spec.md](spec.md), [research.md](research.md),
[data-model.md](data-model.md), [contracts/secure-temp-file.md](contracts/secure-temp-file.md),
[quickstart.md](quickstart.md).

**Tests**: Tests are **included and written first (TDD)** per the user's explicit request in
`/speckit-plan` ("crie cenários de testes para essas issues encontradas primeiro, e outros
cenários para garantir o funcionamento da feature no novo framework .NET 8+"). Test stack:
**xUnit + FluentAssertions** in a multi-target (`net8.0;net48`) test project.

**Organization**: Tasks are grouped by user story (US1, US2, US3 from
[spec.md](spec.md)). Each story is independently testable and delivers a viable increment.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks).
- **[Story]**: `[US1]` / `[US2]` / `[US3]` for user-story tasks; absent for Setup, Foundational, Polish.

## Path Conventions

Multi-project solution. New code lives under:

- New library: `DNN Platform/DotNetNuke.Security.IO/`
- New tests: `DNN Platform/Tests/DotNetNuke.Security.IO.Tests/`

Existing call sites that get migrated are listed by full path in their tasks.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the new multi-target library and test-project skeletons, register them in
the solution, and confirm the empty projects build on both TFMs before any production logic is
written.

- [X] T001 Create directory `DNN Platform/DotNetNuke.Security.IO/` and add a new SDK-style csproj `DNN Platform/DotNetNuke.Security.IO/DotNetNuke.Security.IO.csproj` with `<TargetFrameworks>net8.0;net48</TargetFrameworks>`, `LangVersion=latest`, `TreatWarningsAsErrors=true`, no package references (BCL only).
- [X] T002 Create directory `DNN Platform/Tests/DotNetNuke.Security.IO.Tests/` and add csproj `DNN Platform/Tests/DotNetNuke.Security.IO.Tests/DotNetNuke.Security.IO.Tests.csproj` with `<TargetFrameworks>net8.0;net48</TargetFrameworks>`, `<IsPackable>false</IsPackable>`, and package references: `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `FluentAssertions`. Add a project reference to `DotNetNuke.Security.IO.csproj`. (xunit/FluentAssertions/xunit.runner.visualstudio added to Directory.Packages.props for central package management.)
- [X] T003 [P] Register both new projects in `DNN_Platform.sln` (added via `dotnet sln add`).
- [X] T004 [P] Stylecop link included in the library csproj (test projects in DNN historically omit it; matches existing convention).
- [X] T005 Both projects build clean on `net8.0` and `net48` with zero source files. Baseline established.

**Checkpoint**: Both new projects exist, are wired into the solution, and build empty on both TFMs.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Implement the `SecureTempFile` abstraction (the foundation that **US2** and
**US3** consume). **US1** does NOT depend on this phase and can run in parallel.

**⚠️ CRITICAL**: US2 and US3 cannot start until this phase is complete. Tests are written
first, then implementation makes them pass.

### Foundational — tests first (TDD)

- [X] T006 [P] Tests written first (8 xUnit facts including bonus `Stream_Is_ReadWritable`). Initial state: red (31 compile errors).
- [X] T007 [P] Factory tests written first (2 facts). Initial state: red.
- [X] T008 Red TDD state confirmed: 31 compile errors, all due to missing types.

### Foundational — implementation

- [X] T009 `ISecureTempFileFactory.cs` added — matches the contract exactly.
- [X] T010 `SecureTempFile.cs` added — `FileStream(path, CreateNew, ReadWrite, None, 4096, DeleteOnClose)`. Used fully-qualified `System.IO.Path` to avoid shadowing the `Path` property. Constructor doc uses SA1642 standard text.
- [X] T011 `SecureTempFileFactory.cs` added — stateless, thread-safe.
- [X] T012 All 10 foundational tests pass on both `net8.0` and `net48` (20/20 total).

**Checkpoint**: Foundation is green. US2 and US3 may now begin. US1 has been runnable since Setup.

---

## Phase 3: User Story 1 — Clear the Blocker password disclosure (Priority: P1) 🎯 MVP

**Goal**: Remove SonarQube finding **secrets:S6703** by replacing real-looking password
literals in the AdoNetAppender XML-doc examples with neutral placeholders.

**Independent test**: Re-run SonarQube (or the grep fallback from
[quickstart.md](quickstart.md) §4b); rule `secrets:S6703` reports zero hits, and `git grep
'Password=sa'` returns zero hits.

**Note**: This story is **independent of Phases 1 & 2** (no library dependency). It can be
done in parallel with all other work.

### US1 — tests first

- [X] T013 [P] [US1] `RepoSecretsGuardTests.cs` added with `Repo_Contains_No_Password_Equals_Sa_Literal`. Used substring-based relative path (no `Path.GetRelativePath` — not available on net48). Initial state: red, correctly identifying `AdoNetAppender.cs` as the offender.

### US1 — implementation

- [X] T014 [US1] AdoNetAppender.cs line 93: `User ID=sa;Password=sa` → `User ID=<your-user>;Password=<your-password>` (HTML-encoded as `&lt;…&gt;` since this is inside an XML-doc `value=` attribute). First attempt used `Password=***` but SonarQube IDE still flagged it; angle-bracket placeholder matches SonarSource's recognized placeholder pattern and clears the warning.
- [X] T015 [US1] AdoNetAppender.cs line 168: `User Id=;Password=;` → `User Id=<your-user>;Password=<your-password>` in the OLE-DB example.
- [X] T016 [US1] Guard test green on both TFMs. Full suite: 11/11 pass on net8.0 and net48 (22/22 total).

**Checkpoint**: US1 complete; SonarQube S6703 finding cleared; repository-wide grep for
`Password=sa` returns zero hits.

---

## Phase 4: User Story 2 — Clear the High insecure-temp-file finding (Priority: P1)

**Goal**: Remove SonarQube finding **csharpsquid:S5445** on `Build/Tasks/UpdateDnnManifests.cs`
by migrating its `Path.GetTempFileName()` call to `SecureTempFile`.

**Independent test**: Re-run SonarQube (or grep fallback); rule `csharpsquid:S5445` reports
zero hits on `Build/Tasks/UpdateDnnManifests.cs`. Smoke-test from
[quickstart.md](quickstart.md) §5 produces identical manifest output to the pre-change run.

**Depends on**: Phase 2 (Foundational — `SecureTempFile` library must exist).

### US2 — tests first

- [ ] T017 [US2] Add reference from `Build/Build.csproj` to `DNN Platform/DotNetNuke.Security.IO/DotNetNuke.Security.IO.csproj` (the library's `net8.0` TFM is compatible with Build's `net10.0` target).
- [ ] T018 [P] [US2] Create `DNN Platform/Tests/DotNetNuke.Security.IO.Tests/XdtTransformIntegrationTests.cs` with fact `UpdateDnnManifests_Uses_SecureTempFile_And_Produces_Same_Output` that: (a) prepares a tiny in-memory `.dnn` manifest fixture, (b) runs the same XDT transform logic used by `UpdateDnnManifests.Run` via `SecureTempFile`, (c) asserts the resulting transformed XML matches a golden fixture. Test MUST initially fail (no migration yet).
- [ ] T019 [P] [US2] Add a guard test in `RepoSecretsGuardTests.cs` (from T013) — extend with `Build_Tasks_Folder_Contains_No_GetTempFileName_Calls` that greps `Build/Tasks/*.cs` and asserts zero hits for `Path.GetTempFileName(`.

### US2 — implementation

- [ ] T020 [US2] Edit `Build/Tasks/UpdateDnnManifests.cs`: import `DotNetNuke.Security.IO`. Inside `Run`, replace line 39 (`var transformFile = context.File(System.IO.Path.GetTempFileName());`) with a `using var temp = new SecureTempFileFactory().Create();` block and pass `temp.Path` to `context.FileAppendText` and `context.XdtTransformConfig`. Ensure the `using` scope covers both calls.
- [ ] T021 [US2] Run the test from T018 — MUST now pass. Run the guard test from T019 — MUST now pass.
- [ ] T022 [US2] Smoke-test per [quickstart.md](quickstart.md) §5: `dotnet run --project Build/Build.csproj -- --target=UpdateDnnManifests` and diff `*.dnn` output against the pre-change baseline. MUST be identical.

**Checkpoint**: US2 complete; SonarQube S5445 finding cleared on `UpdateDnnManifests.cs`; the
two P1 stories together form the deliverable MVP.

---

## Phase 5: User Story 3 — Eliminate the insecure pattern repo-wide (Priority: P2)

**Goal**: Migrate the 5 remaining production call sites of `Path.GetTempFileName()` to
`SecureTempFile`, so a future SonarQube scan finds zero S5445 hits anywhere in the repo.

**Independent test**: `git grep 'Path.GetTempFileName'` on the production source tree returns
hits **only** inside `DNN Platform/DotNetNuke.Security.IO/` and its tests (≤ 2 files).
SonarQube full-repo scan reports zero S5445 findings.

**Depends on**: Phase 2 (Foundational). Does NOT depend on Phases 3 or 4 — can run in parallel
with them once Foundational is done.

### US3 — tests first

- [ ] T023 [P] [US3] Extend `RepoSecretsGuardTests.cs` with `Repo_Contains_No_GetTempFileName_Outside_Security_IO` that greps all `*.cs` under `DNN Platform/` and `Build/` for `Path.GetTempFileName(`, excluding `DNN Platform/DotNetNuke.Security.IO/` and `*/bin/`,`*/obj/`. Test MUST initially fail (5 hits expected).
- [ ] T024 [P] [US3] Add unit test `RssDownloadManagerSecureTempTest` in the existing `DNN Platform/Tests/DotNetNuke.Tests.Core/` (or a new sibling test class), asserting the migrated `RssDownloadManager` still produces a parseable RSS file when fed a known input. (Reuses existing test infrastructure of that project.)
- [ ] T025 [P] [US3] Add equivalent regression test for `OpmlDownloadManager`, `FileResponseFilter`, and `FileProvider` — one assertion per migrated file in their respective existing test projects.

### US3 — implementation (each file independent of the others)

- [ ] T026 [P] [US3] Add project reference from `DNN Platform/Syndication/DotNetNuke.Syndication.csproj` to `DotNetNuke.Security.IO.csproj`. Migrate `DNN Platform/Syndication/RSS/RssDownloadManager.cs`: replace `Path.GetTempFileName()` calls with `using var temp = factory.Create();` consuming `temp.Path`. Inject `ISecureTempFileFactory` via constructor (DIP) — if the class has no DI today, instantiate `new SecureTempFileFactory()` inline and add a TODO for DI to a future task.
- [ ] T027 [P] [US3] Migrate `DNN Platform/Syndication/OPML/OpmlDownloadManager.cs` analogously to T026.
- [ ] T028 [P] [US3] Add project reference from `DNN Platform/Library/Library.csproj` (or whichever csproj owns `Services/OutputCache`) to `DotNetNuke.Security.IO.csproj`. Migrate `DNN Platform/Library/Services/OutputCache/Providers/FileResponseFilter.cs`.
- [ ] T029 [P] [US3] Migrate `DNN Platform/Library/Services/OutputCache/Providers/FileProvider.cs` analogously to T028.
- [ ] T030 [US3] Migrate the test-subject usage in `DNN Platform/Tests/DotNetNuke.Tests.Core/Services/UtilTest.cs` — replace `Path.GetTempFileName()` with `SecureTempFile` in the production code under test (NOT in the test itself, which is exercising the SUT). Adjust assertions if the test was previously inspecting the predictable filename pattern.
- [ ] T031 [US3] Run the full `DNN Platform/Tests/DotNetNuke.Security.IO.Tests/` suite — `Repo_Contains_No_GetTempFileName_Outside_Security_IO` (T023) MUST now pass. Run T024–T025 regression tests — MUST pass.

**Checkpoint**: US3 complete; the insecure API is gone from production code repo-wide.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Verify SonarQube acceptance criteria, capture artifacts, no-regression check,
documentation.

- [ ] T032 [P] Re-run the entire DNN solution test suite: `dotnet test DNN_Platform.sln -c Release --filter "Category!=Integration"`. Compare pass/fail against the pre-change `develop` baseline. Zero new failures permitted.
- [ ] T033 [P] Run SonarQube full-branch scan per [quickstart.md](quickstart.md) §4a (preferred) OR the grep fallback §4b. Export the rule-filtered report for `csharpsquid:S5445` (expect 0) and `secrets:S6703` (expect 0). Save the artifact under `specs/001-sonarqube-security-fixes/verification/sonarqube-report.{pdf|md}` and link from the PR description.
- [ ] T034 [P] Run the multi-target test suite explicitly on both TFMs: `dotnet test "DNN Platform/Tests/DotNetNuke.Security.IO.Tests/" -c Release -f net8.0` and `... -f net48`. Both MUST be green.
- [ ] T035 Edit `DNN Platform/DotNetNuke.Security.IO/DotNetNuke.Security.IO.csproj`: ensure `<GenerateDocumentationFile>true</GenerateDocumentationFile>` is set and that all public APIs (`ISecureTempFileFactory`, `SecureTempFile`, `SecureTempFileFactory`) carry XML-doc comments matching [contracts/secure-temp-file.md](contracts/secure-temp-file.md). Build MUST emit zero `CS1591` warnings.
- [ ] T036 Update PR description per the constitution: include rule IDs `csharpsquid:S5445` and `secrets:S6703`, link to the verification artifact from T033, list the migrated files, and confirm Constitution Principles I–V compliance.

---

## Dependencies & Story Completion Order

```text
Setup (Phase 1)
   │
   ├──► Foundational (Phase 2) ──► US2 (Phase 4) ──┐
   │                            └► US3 (Phase 5) ──┤
   │                                               ├──► Polish (Phase 6)
   └─────────────────────────────► US1 (Phase 3) ──┘
```

- **Setup** must complete before **Foundational** (foundation lives in the libraries Setup
  created).
- **US1** depends only on Setup (no library needed) — can run in parallel with Foundational.
- **US2** and **US3** depend on Foundational.
- **US2** and **US3** are independent of each other and can run in parallel.
- **Polish** runs after all user stories are done.

## Parallel Execution Examples

Within a single developer / agent:

- T001 alongside `git checkout`-time inspection of any existing tests.
- T003 + T004 in parallel (different .sln entries / different csproj files).
- T006 + T007 in parallel (different test files).
- T013 in parallel with all of Foundational (independent of library).
- T023 + T024 + T025 in parallel (all new test files).
- T026 + T027 + T028 + T029 in parallel (each file is an independent migration target).
- T032 + T033 + T034 in parallel during Polish (independent verification commands).

## Implementation Strategy: MVP First

- **MVP scope (P1 only)**: complete Phase 1 + Phase 2 + Phase 3 + Phase 4 + the Polish items
  T032, T033, T036. This clears both originally-reported SonarQube findings and is a valid
  release.
- **Full delivery (adds P2)**: also complete Phase 5 + T034. This eliminates the insecure API
  from all production sites, fulfilling the user-authorized cross-file refactor.
- If schedule pressure forces a descope: ship MVP first, file a follow-up issue for Phase 5
  referencing the deferred call sites.

## Task Count Summary

| Phase | Task Count | Of which [P] parallel |
| --- | --- | --- |
| 1 — Setup | 5 (T001–T005) | 2 |
| 2 — Foundational | 7 (T006–T012) | 2 |
| 3 — US1 | 4 (T013–T016) | 1 |
| 4 — US2 | 6 (T017–T022) | 2 |
| 5 — US3 | 9 (T023–T031) | 7 |
| 6 — Polish | 5 (T032–T036) | 3 |
| **Total** | **36** | **17** |
