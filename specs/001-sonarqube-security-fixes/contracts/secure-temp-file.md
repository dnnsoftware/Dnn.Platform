# Contract: `DotNetNuke.Security.IO.SecureTempFile`

**Feature**: 001-sonarqube-security-fixes
**Library**: `DotNetNuke.Security.IO` (multi-target `net8.0;net48`)

## Public surface

```csharp
namespace DotNetNuke.Security.IO;

public interface ISecureTempFileFactory
{
    SecureTempFile Create();
}

public sealed class SecureTempFileFactory : ISecureTempFileFactory
{
    public SecureTempFile Create();
}

public sealed class SecureTempFile : IDisposable
{
    public string Path { get; }
    public FileStream Stream { get; }
    public bool IsDisposed { get; }

    public void Dispose();
}
```

## Behavioral contract

### `SecureTempFileFactory.Create()`

| Aspect | Contract |
| --- | --- |
| Return value | A `SecureTempFile` whose `IsDisposed` is `false` and whose `Path` points to an existing zero-byte file. |
| Path uniqueness | Two consecutive calls MUST return instances whose `Path` values differ. |
| Path location | Inside the directory returned by `Path.GetTempPath()`. |
| Path name format | Produced by `Path.GetRandomFileName()` (8 chars + `.` + 3 chars). |
| Failure mode | Throws `IOException` if the temp directory is unwritable or a name collision occurs. |
| Thread-safety | Safe to call concurrently from multiple threads. |
| Side-effects | Exactly one new file created on disk per call. |

### `SecureTempFile.Path`

- Read-only string property; always non-null.
- Value is stable from construction until disposal.

### `SecureTempFile.Stream`

- Exclusive read/write `FileStream` opened with `FileShare.None` and
  `FileOptions.DeleteOnClose`.
- Callers MAY read and write through this stream during the `Open` state.
- Callers MAY also access the file by `Path` from a separate process if and only if no
  exclusive access is required — but the holding stream itself prevents shared access; doing
  so would fail. Documented limitation.

### `SecureTempFile.IsDisposed`

- `false` immediately after construction.
- `true` after the first call to `Dispose()`.

### `SecureTempFile.Dispose()`

- Idempotent. Calling it multiple times MUST NOT throw.
- Closes `Stream`, which triggers OS deletion of the file (because of `DeleteOnClose`).
- After return, all public members other than `IsDisposed` and `Dispose` MUST throw
  `ObjectDisposedException` when accessed.

## Exception contract

| Exception | When |
| --- | --- |
| `IOException` | At construction, if temp directory is unwritable, or if a random-name collision occurs (vanishingly rare; safe to retry). |
| `UnauthorizedAccessException` | At construction, if the current user lacks write permission on the temp directory. |
| `ObjectDisposedException` | If `Path` or `Stream` is accessed after `Dispose`. |

The library MUST NOT swallow these exceptions and MUST NOT throw any other custom exception
type for the listed failure modes.

## Consumer migration contract

For every call site of the form:

```csharp
// before
var tempPath = Path.GetTempFileName();
// ... use tempPath ...
File.Delete(tempPath);   // (sometimes — many call sites leak the file)
```

The migration MUST produce:

```csharp
// after
using var temp = factory.Create();   // factory is injected or new'd
// ... use temp.Path ...
// no explicit delete: handled by Dispose
```

Where the consumer's API requires only a path string (e.g.,
`Cake.XdtTransform.XdtTransformConfig(file, transformFile, file)`), pass `temp.Path` — the
existing path-string contract is preserved.

## SonarQube acceptance

After migration, a SonarQube re-scan MUST report:

- **csharpsquid:S5445** — 0 findings repo-wide (currently 1 in `Build/Tasks/UpdateDnnManifests.cs:39`).
- **secrets:S6703** — 0 findings repo-wide (currently 1 in
  `DNN Platform/DotNetNuke.Log4net/log4net/Appender/AdoNetAppender.cs`).
- No new findings of severity **High** or **Blocker** introduced in any touched file
  relative to the pre-change baseline scan.
