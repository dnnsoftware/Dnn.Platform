# Phase 1 Data Model: SonarQube Security Findings Remediation

**Feature**: 001-sonarqube-security-fixes
**Date**: 2026-05-21

## Entity: `SecureTempFile`

A disposable wrapper around a single, exclusively-owned, randomly-named temporary file on the
host operating system. Replaces direct calls to `System.IO.Path.GetTempFileName()`.

### Fields

| Field | Type | Notes |
| --- | --- | --- |
| `Path` | `string` (read-only) | Absolute filesystem path of the temp file. Stable for the lifetime of the instance. |
| `Stream` | `FileStream` (read-only) | The exclusive, read/write stream opened against `Path`. Closed automatically on `Dispose`. |
| `IsDisposed` | `bool` (read-only) | `true` once `Dispose()` has been called. Subsequent operations throw `ObjectDisposedException`. |

### Lifecycle (states)

```text
   [Created] -- construction succeeds --> [Open] -- Dispose() --> [Disposed]
                                            |
                                            +-- process exits --> [Disposed] (OS reclaims via DeleteOnClose)
```

### Invariants

- **I1. Unique path**: `Path` is produced by `Path.GetRandomFileName()` joined to
  `Path.GetTempPath()` and is guaranteed to be a freshly-created file at construction time
  (constructor uses `FileMode.CreateNew`). If a collision occurs (vanishingly rare), the
  constructor MUST throw `IOException`; the caller MAY retry.
- **I2. Exclusive ownership**: while `IsDisposed` is `false`, the file is held with
  `FileShare.None`; no other handle can open it for read or write.
- **I3. Guaranteed cleanup**: `Dispose()` closes `Stream`, which triggers the OS to delete
  the file because the stream was opened with `FileOptions.DeleteOnClose`. If the process
  crashes mid-use, the OS still deletes on handle close at process exit.
- **I4. No silent fallback to insecure API**: the implementation MUST NOT call
  `Path.GetTempFileName()` under any code path. SonarQube re-scan must produce zero hits for
  the insecure API name inside `DotNetNuke.Security.IO`.

### Validation rules

- Constructor MUST validate that `Path.GetTempPath()` returns a writable directory. If not,
  it MUST throw `IOException` with a message identifying the temp directory; callers MUST NOT
  receive a partially-constructed instance.
- All public members MUST throw `ObjectDisposedException` if invoked after `Dispose`.

### State transitions

| From | Trigger | To | Side-effects |
| --- | --- | --- | --- |
| (none) | `new SecureTempFile()` | `Open` | File created on disk under `Path`; `Stream` opened. |
| `Open` | `Dispose()` | `Disposed` | `Stream.Dispose()`; file removed via `DeleteOnClose`. |
| `Open` | process exit | `Disposed` | OS-level `DeleteOnClose` cleanup. |
| `Disposed` | any public call | `Disposed` | Throws `ObjectDisposedException`. |
| `Disposed` | `Dispose()` (idempotent) | `Disposed` | No-op. |

## Entity: `ISecureTempFileFactory`

A thin factory interface that decouples consumers from the concrete `SecureTempFile`
constructor, enabling test doubles (DIP) and adhering to ISP (one method).

### Operations

| Operation | Signature | Notes |
| --- | --- | --- |
| `Create` | `SecureTempFile Create()` | Returns a fresh `Open`-state instance. Each call returns a distinct file. |

### Default implementation: `SecureTempFileFactory`

Stateless, thread-safe, has a parameterless constructor so it can be instantiated by
build-task DI or new'd directly by legacy net48 call sites that don't use a container.

## Non-entities (intentionally absent)

- **Configuration object**: no buffer size, encoding, or naming knobs. YAGNI — none of the
  current call sites need them. If a future caller does, that's a separate change.
- **Async API**: callers today use synchronous IO. YAGNI.
- **Logger dependency**: the abstraction does not log. SRP — logging is the caller's
  responsibility.
