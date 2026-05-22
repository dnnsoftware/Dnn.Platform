# Feature Specification: SonarQube Security Findings Remediation

**Feature Branch**: `001-sonarqube-security-fixes`

**Created**: 2026-05-21

**Status**: Draft

**Input**: User description: "preciso que a correção contemple boa práticas de código se necessario, como solid, yagn, dp, a refatoração pode reajustar o problema em vários arquivos para uma melhor manutenção caso necessário"

## Overview

The DNN Platform fork has two SonarQube security findings that block release: an insecure
temporary-file API (rule **csharpsquid:S5445**, severity High) in the build task code, and a
disclosed database password (rule **secrets:S6703**, severity Blocker) inside the log4net
appender's XML documentation. The user authorized the remediation work to apply sound design
practices (SOLID, YAGNI, DRY) and to refactor across multiple files when doing so improves
maintainability — i.e. the minimal-diff principle MAY be relaxed where a small, well-justified
refactor reduces the chance the same finding reappears elsewhere.

A repository scan confirms the insecure pattern (`Path.GetTempFileName`) is used in **6
production files**, not only the one SonarQube flagged. A shared helper that produces a secure
temporary file in one place is therefore justified by DRY (it eliminates copy/paste of the same
unsafe pattern) without violating YAGNI (the consumers already exist today).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Clear the Blocker-severity password disclosure (Priority: P1)

As a **security reviewer** running SonarQube against the fork, I need rule **secrets:S6703** to
report zero findings so the release pipeline is no longer blocked.

**Why this priority**: S6703 is Blocker severity. Until it is cleared, the pipeline gate fails
and no other security work matters.

**Independent Test**: Re-run SonarQube on the branch; the S6703 finding on
`DNN Platform/DotNetNuke.Log4net/log4net/Appender/AdoNetAppender.cs` no longer appears, and no
new S6703 finding is introduced anywhere in the repo.

**Acceptance Scenarios**:

1. **Given** the AdoNetAppender XML-doc examples currently contain `Password=sa` and
   `Password=`, **When** SonarQube re-scans the branch, **Then** rule secrets:S6703 reports zero
   findings for that file.
2. **Given** a developer reads the AdoNetAppender XML-doc examples, **When** they copy the
   example connection string into their own configuration, **Then** the value they paste is
   visibly a placeholder (e.g. `Password=***`, `<your-password>`) that cannot be mistaken for a
   real credential.
3. **Given** the repository's full source tree, **When** a reviewer greps for `Password=sa`,
   **Then** zero hits are returned.

---

### User Story 2 - Clear the High-severity insecure temp-file finding (Priority: P1)

As a **security reviewer**, I need rule **csharpsquid:S5445** to report zero findings on
`Build/Tasks/UpdateDnnManifests.cs` so the High-severity issue is closed.

**Why this priority**: S5445 is the second of the two findings the user explicitly listed and
is the trigger for the cross-cutting refactor described in Story 3.

**Independent Test**: Re-run SonarQube on the branch; the original S5445 finding at
`Build/Tasks/UpdateDnnManifests.cs:39` no longer appears.

**Acceptance Scenarios**:

1. **Given** the build task currently calls `Path.GetTempFileName()` at line 39,
   **When** SonarQube re-scans the branch, **Then** rule csharpsquid:S5445 reports zero findings
   on that file.
2. **Given** the build task is executed against representative DNN manifest files, **When** the
   XDT transformation step runs, **Then** the transformed output is identical to the output
   produced before the fix (no behavioral regression).
3. **Given** the temporary file created during the transformation, **When** the task finishes
   (normally or with an exception), **Then** the temporary file no longer exists on disk.

---

### User Story 3 - Eliminate the insecure pattern across the whole repository (Priority: P2)

As a **maintainer**, I need every existing call to the insecure temp-file API to be replaced by
a single secure helper so that the same finding does not reappear in another file the next time
SonarQube scans the repo.

**Why this priority**: P2 — not a SonarQube blocker today, but the user explicitly authorized a
cross-file refactor for maintainability, and 5 other production files use the same insecure
pattern. Doing this now is cheaper than fixing it six more times. (If the project decides to
descope this, Stories 1 and 2 still deliver a valid MVP.)

**Independent Test**: Grep the production source tree for `GetTempFileName`; the only
occurrences are inside the new secure helper's implementation (or its tests). Re-running
SonarQube produces zero S5445 findings repo-wide.

**Acceptance Scenarios**:

1. **Given** the 5 additional production files that currently use `Path.GetTempFileName`
   (`RssDownloadManager.cs`, `OpmlDownloadManager.cs`, `FileResponseFilter.cs`,
   `FileProvider.cs`, `UtilTest.cs`'s subject under test), **When** each call site is migrated
   to the secure helper, **Then** their existing behavior — temp file is writable, readable,
   and deleted on disposal — is preserved.
2. **Given** the secure helper, **When** any future call site needs a temporary file, **Then**
   it can obtain one without re-introducing the insecure pattern.
3. **Given** the repository after the refactor, **When** SonarQube re-scans, **Then** rule
   csharpsquid:S5445 reports zero findings anywhere in the repo.

---

### Edge Cases

- **Concurrent build tasks**: if two build tasks run in parallel on the same machine, each
  MUST obtain a unique temporary file path — i.e. random-name generation MUST be collision-safe
  enough that real-world parallel use does not produce filename clashes.
- **Crash before cleanup**: if the process is killed mid-transformation, the temporary file
  MAY be left on disk for the OS to reap from the temp directory; this is acceptable and
  matches the platform's existing behavior.
- **Read-only temp directory**: behavior on locked-down build agents (e.g., CI runners where
  the temp path is non-writable) MUST surface a clear, actionable error rather than a silent
  failure — same expectation as today.
- **Existing XML-doc examples elsewhere**: other files MAY contain similar example connection
  strings with placeholder passwords; if SonarQube flags any of them in the verification scan,
  they MUST be normalized to the same placeholder convention chosen for AdoNetAppender.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The repository MUST NOT contain any literal value that SonarQube classifies as a
  database password (rule secrets:S6703), including in `///` XML-doc examples, README files, or
  configuration samples checked into source.
- **FR-002**: Example connection strings in documentation MUST use neutral placeholder values
  (e.g., `Password=***`, `<your-password>`, `${DB_PASSWORD}`) that cannot be mistaken for, or
  scanned as, real credentials.
- **FR-003**: The build task at `Build/Tasks/UpdateDnnManifests.cs` MUST create its temporary
  files via a mechanism that SonarQube does not flag under rule csharpsquid:S5445, and MUST
  delete the temporary file when the task completes (normal or exceptional path).
- **FR-004**: The secure temporary-file mechanism MUST be exposed as a single, reusable
  abstraction so that the same insecure pattern is not re-implemented at each call site (DRY).
  The abstraction MUST have a single, well-defined responsibility — producing a securely-named,
  exclusive, auto-cleanup temporary file — and no unrelated concerns (SRP).
- **FR-005**: All existing call sites of the insecure temp-file pattern in the production
  source tree MUST be migrated to the new abstraction, OR explicitly justified in the PR if a
  call site is intentionally left behind (e.g. third-party vendored code).
- **FR-006**: The refactor MUST NOT introduce speculative extension points, configuration
  knobs, or abstractions beyond what the listed call sites need today (YAGNI).
- **FR-007**: Every change MUST be traceable to one of the two SonarQube rules in scope (S5445
  or S6703), or to the cross-cutting refactor authorized by the user input. Drive-by changes
  unrelated to either MUST be rejected at review.
- **FR-008**: After the changes, re-running the project's existing test suite MUST produce the
  same pass/fail outcome as before the changes (no regression in tests that previously passed).
- **FR-009**: SonarQube verification MUST be performed and the resulting report (showing S5445
  and S6703 cleared, with no new findings of equal or greater severity) MUST be attached to
  the PR per the project constitution's Principle V.

### Key Entities

- **Secure temporary file abstraction**: represents a uniquely-named, isolated, auto-cleaned
  scratch file in the OS temporary directory. Key attributes: a filesystem path safe from
  predictable-name attacks, exclusive ownership for the lifetime of the operation, and a
  guaranteed cleanup contract (deleted when the consumer disposes of it or when the operation
  ends). It replaces direct calls to the platform's insecure temp-file API across the
  repository.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: After remediation, a SonarQube scan of the branch reports **zero** findings for
  rule secrets:S6703 across the entire repository.
- **SC-002**: After remediation, a SonarQube scan reports **zero** findings for rule
  csharpsquid:S5445 across the entire repository.
- **SC-003**: After remediation, no new SonarQube findings of severity **High** or **Blocker**
  are introduced in any file touched by the change set, as measured by comparing the
  pre-change baseline scan to the post-change scan.
- **SC-004**: A repository-wide search for `Password=sa` (case-insensitive) returns **zero
  hits** in tracked files.
- **SC-005**: A repository-wide search for `Path.GetTempFileName` returns hits only inside the
  new secure-temp-file abstraction's own implementation and tests (≤ 2 files).
- **SC-006**: The existing test suite's pass/fail result is unchanged: every test that passed
  before the change still passes after the change.
- **SC-007**: The DNN manifest update build task produces byte-identical (or semantically
  identical) output before and after the change when run on the same input manifests.

## Assumptions

- The SonarQube ruleset used in CI is the same one that produced the original report (rule IDs
  csharpsquid:S5445 and secrets:S6703 remain enabled at their current severities). If the
  ruleset diverges, the verification step (SC-001..SC-003) must be performed against the same
  ruleset that flagged the issue.
- The user's mention of "DP" is interpreted as **DRY** (Don't Repeat Yourself). If instead
  the user meant "Design Patterns" generically, the intent is the same: introduce a small,
  well-known pattern (a disposable resource wrapper) only because there is already real
  duplication to eliminate. This assumption is recorded so the reviewer can correct it during
  `/speckit-clarify` if wrong.
- The 5 additional production call sites of `Path.GetTempFileName` listed in Story 3 are all
  candidates for migration. If any of them turns out to live in third-party / vendored code
  that the project does not own, it will be excluded with a one-line justification in the PR
  per FR-005.
- "Refactoring across files for better maintenance" granted in the user input authorizes a
  controlled relaxation of the constitution's **Minimal-Diff** principle for this feature
  only, limited to the cross-cutting introduction and adoption of the secure temp-file helper.
  The relaxation MUST be documented as a justified deviation in the plan's Complexity Tracking
  section.
- The project's CI is able to run SonarQube on the feature branch (or the developer can run
  the SonarQube scanner locally with the project's quality profile). If neither is true, a
  manual line-by-line check against the two rules' detection logic will be accepted as a
  fallback, with the limitation noted in the verification artifact.
- "Insecure" for SonarQube S5445 refers to the predictability/uniqueness of the temp-file name
  and the file's existence at creation time. The accepted remediation is the
  `Path.GetTempPath()` + `Path.GetRandomFileName()` pair combined with explicit `FileStream`
  creation, per the project constitution's Principle III.
