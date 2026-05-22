<!--
Sync Impact Report
==================
Version change: (none) → 1.0.0
Rationale: Initial ratification. All placeholder tokens replaced with concrete content,
so this is treated as a MAJOR establishment release.

Modified principles:
  - [PRINCIPLE_1_NAME]            → I. Security-First Remediation (NON-NEGOTIABLE)
  - [PRINCIPLE_2_NAME]            → II. Minimal-Diff Changes
  - [PRINCIPLE_3_NAME]            → III. Secure-by-Default APIs
  - [PRINCIPLE_4_NAME]            → IV. No Secrets in Source
  - [PRINCIPLE_5_NAME]            → V. Verified by Static Analysis

Added sections:
  - Security Requirements (replaces [SECTION_2_NAME])
  - Development Workflow (replaces [SECTION_3_NAME])
  - Governance (concrete content)

Removed sections: none

Templates requiring updates:
  - .specify/templates/plan-template.md   — ✅ aligned (generic "Constitution Check" gate; no edits required)
  - .specify/templates/spec-template.md   — ✅ aligned (no principle-specific gates)
  - .specify/templates/tasks-template.md  — ✅ aligned (no principle-specific gates)
  - .specify/templates/checklist-template.md — ✅ aligned (generic)

Follow-up TODOs: none. RATIFICATION_DATE set to today (2026-05-21) because no prior
constitution existed.
-->

# DNN Platform Security-Fix Project Constitution

## Core Principles

### I. Security-First Remediation (NON-NEGOTIABLE)

Every code change in this project MUST map to a specific, identified security finding
(SonarQube rule ID + file path + line number, or an equivalent advisory). Changes that
do not resolve a tracked finding MUST NOT be merged.

**Rationale**: The project's sole purpose is to remediate SonarQube findings on a DNN
Platform fork. Scope discipline prevents the security work from being diluted by
unrelated edits and keeps audit trails clear for grading and review.

### II. Minimal-Diff Changes

Fixes MUST be the smallest change that resolves the finding without regressing
behavior. Surrounding refactors, formatting changes, dependency upgrades, and "while
I'm here" cleanups are out of scope and MUST be filed as separate work.

**Rationale**: A small diff is reviewable, reversible, and easy to verify against the
specific rule it claims to clear. Large diffs hide regressions and obscure which line
actually fixed the finding.

### III. Secure-by-Default APIs

When a finding flags an insecure platform API, the fix MUST replace it with the
documented secure equivalent — not merely suppress or annotate the warning.
Specifically:

- `Path.GetTempFileName()` MUST be replaced by composing `Path.GetTempPath()` with
  `Path.GetRandomFileName()`, and the temporary file MUST be created explicitly with
  appropriate `FileOptions` (e.g. `FileOptions.DeleteOnClose` where the lifetime is
  scoped to the operation).
- Suppressions (`#pragma warning disable`, `[SuppressMessage]`) are forbidden as a
  remediation strategy unless accompanied by a written justification reviewed in the PR.

**Rationale**: Suppression hides risk; replacement eliminates it. SonarQube clearing a
finding because the rule was disabled is not a fix.

### IV. No Secrets in Source

Credentials, passwords, tokens, API keys, and connection strings containing real
secrets MUST NOT appear in source code, configuration committed to the repository, OR
documentation/XML-doc examples. Example connection strings in `///` comments and
README snippets MUST use neutral placeholders such as `Password=***`,
`<your-password>`, or `${DB_PASSWORD}`. The word `sa` as a SQL password MUST NOT
appear anywhere in the repository, including doc examples.

**Rationale**: Static-analysis secret scanners (and humans copy-pasting examples)
cannot distinguish a real leaked credential from a doc example. The safe default is
that no example ever contains a value that could plausibly be a real secret.

### V. Verified by Static Analysis

Every fix MUST be verified by re-running the static analyzer of record (SonarQube) and
demonstrating that the originally reported finding is cleared and that no new
findings of equal or higher severity have been introduced. The verification artifact
(SonarQube report excerpt, CLI output, or screenshot) MUST be attached to the PR.

**Rationale**: "I read the code and it looks right" is not verification for security
fixes. The same tool that flagged the issue is the authoritative source for declaring
it resolved.

## Security Requirements

In-scope SonarQube rule families for this project:

- **csharpsquid:S5445** — Insecure temporary file creation. Mitigation pattern:
  `Path.GetTempPath()` + `Path.GetRandomFileName()` with explicit `FileStream`
  creation using restrictive `FileOptions`.
- **secrets:S6703** — Database password / credential disclosure. Mitigation pattern:
  replace literal credentials with placeholders in examples; ensure no real
  credential ever lands in tracked files.
- Any related CWE-377 (insecure temporary file) and CWE-798 (hard-coded credentials)
  findings surfaced by SonarQube during verification.

Analyzer of record: **SonarQube** (rule IDs as published by SonarSource). Other
analyzers (Roslyn analyzers, `dotnet format`, CodeQL) MAY be consulted but are not
authoritative for declaring a finding closed.

## Development Workflow

1. **One finding per Spec Kit cycle.** Each SonarQube finding is treated as a
   self-contained feature: `/speckit-specify` → `/speckit-plan` → `/speckit-tasks` →
   `/speckit-implement`. Cross-cutting work is split.
2. **Feature branch per finding.** Branch name MUST include the SonarQube rule ID
   (e.g. `fix/s5445-temp-file`).
3. **PR requirements.** The PR description MUST contain: (a) the SonarQube rule ID,
   (b) the original file:line reference, (c) the fix summary, (d) the verification
   artifact from Principle V.
4. **Review gate.** A reviewer MUST confirm: the diff is minimal (Principle II), no
   suppressions are used without justification (Principle III), no new secrets are
   introduced (Principle IV), and the verification artifact shows the rule cleared
   (Principle V).
5. **Constitution Check before plan.** Every `plan.md` MUST include a Constitution
   Check section confirming the planned approach satisfies all five principles, or
   documenting any justified deviations in a Complexity Tracking section.

## Governance

This constitution supersedes ad-hoc practices for the duration of this project. Any
behavior that conflicts with these principles MUST be resolved in favor of the
constitution or formalized as an amendment.

**Amendment procedure**: Amendments are proposed via PR editing this file. The PR
MUST include an updated Sync Impact Report (the HTML comment at the top), an updated
version line, and propagated changes to any affected templates under
`.specify/templates/`. Amendments require approval by the project owner.

**Versioning policy** (semantic):

- **MAJOR**: Removing a principle, redefining a principle's intent, or otherwise
  introducing a backward-incompatible governance change.
- **MINOR**: Adding a new principle or materially expanding the guidance of an
  existing principle/section.
- **PATCH**: Clarifications, wording fixes, typo corrections, non-semantic
  refinements.

**Compliance review**: Every PR is reviewed against this constitution. The reviewer
MUST explicitly note compliance with each principle in the review comments or via a
checklist. Non-compliant PRs MUST be revised or accompanied by a documented,
approved deviation.

**Version**: 1.0.0 | **Ratified**: 2026-05-21 | **Last Amended**: 2026-05-21
