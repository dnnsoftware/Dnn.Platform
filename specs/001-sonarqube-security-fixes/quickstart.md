# Quickstart: Verify SonarQube Security Findings Remediation

**Feature**: 001-sonarqube-security-fixes
**Branch**: `001-sonarqube-security-fixes`

This is the end-to-end verification recipe. Run these steps in order on the feature branch
before opening the PR.

## Prerequisites

- .NET SDK 10.0.300 (pinned in [global.json](../../global.json)).
- Windows host with the existing DNN Platform build chain working (MSBuild, Cake.Frosting).
- Optional: a SonarQube instance with the project's quality profile. If unavailable, the
  grep fallback in step 4 is acceptable.

## 1. Build the new library and tests

```powershell
dotnet build "DNN Platform/DotNetNuke.Security.IO/DotNetNuke.Security.IO.csproj" -c Release
dotnet build "DNN Platform/Tests/DotNetNuke.Security.IO.Tests/DotNetNuke.Security.IO.Tests.csproj" -c Release
```

Expected: both projects build for **both** TFMs (`net8.0` and `net48`). Verify the artifacts:

```powershell
ls "DNN Platform/DotNetNuke.Security.IO/bin/Release/net8.0/"
ls "DNN Platform/DotNetNuke.Security.IO/bin/Release/net48/"
```

Both directories MUST contain `DotNetNuke.Security.IO.dll`.

## 2. Run the security-fix tests (TDD scenarios — Story 1 & 2)

```powershell
dotnet test "DNN Platform/Tests/DotNetNuke.Security.IO.Tests/DotNetNuke.Security.IO.Tests.csproj" -c Release
```

Expected: all tests pass on both TFMs. Key tests that MUST be present and green:

- `SecureTempFileTests.Create_Produces_File_Inside_TempDirectory`
- `SecureTempFileTests.Create_Returns_Unique_Paths_For_Concurrent_Callers`
- `SecureTempFileTests.Path_Format_Matches_GetRandomFileName_Pattern`
- `SecureTempFileTests.Dispose_Deletes_File_From_Disk`
- `SecureTempFileTests.Dispose_Is_Idempotent`
- `SecureTempFileTests.Access_After_Dispose_Throws_ObjectDisposedException`
- `SecureTempFileTests.Stream_Has_Exclusive_Lock_While_Open`
- `SecureTempFileTests.Constructor_Throws_IOException_On_Unwritable_TempDir` (skipped on CI
  if temp dir is always writable; documented as a guard test).
- `XdtTransformIntegrationTests.UpdateDnnManifests_Produces_Same_Output_As_Before` (regression
  test against a golden file — Story 2 acceptance scenario 2).

## 3. Re-run the existing test suite (no-regression check)

```powershell
dotnet test DNN_Platform.sln -c Release --filter "Category!=Integration"
```

Expected: same pass/fail ratio as the `develop` baseline. Specifically, none of the previously
passing tests turn red. Use the baseline captured before this branch (or `git stash` + run +
restore) if a saved baseline isn't available.

## 4. Verify SonarQube findings are cleared

### 4a. With SonarQube (preferred)

Trigger a scan against the feature branch using the project's existing pipeline (CI) or run
the scanner locally:

```powershell
dotnet sonarscanner begin /k:"<project-key>" /d:sonar.host.url="<sonar-url>" /d:sonar.login="<token>"
dotnet build DNN_Platform.sln -c Release
dotnet sonarscanner end /d:sonar.login="<token>"
```

In the SonarQube UI for this branch:

- Filter rule **`csharpsquid:S5445`** → expect 0 issues.
- Filter rule **`secrets:S6703`** → expect 0 issues.
- Open the **New Code** tab → no new High or Blocker issues introduced.

Export the scan summary as a PDF or copy the rule-filter URL and attach it to the PR.

### 4b. Without SonarQube (grep fallback)

```powershell
# S6703: no real-looking DB password literals in tracked files.
git grep -nE 'Password=sa\b' -- ':!.specify' ':!specs'

git grep -nE 'Password=[^*<&$\s][^"<&\s]*' -- ':!.specify' ':!specs'

# S5445: no Path.GetTempFileName calls outside the new library.
git grep -n 'Path\.GetTempFileName' -- ':!DNN Platform/DotNetNuke.Security.IO/**' ':!**/*.Tests/**' ':!.specify' ':!specs'
```

Expected: each command returns **zero hits**. Save the command output and attach to the PR.

## 5. Smoke-test the migrated Cake build task

```powershell
dotnet run --project Build/Build.csproj -- --target=UpdateDnnManifests
```

Expected: the task completes without error and updates the `version` attribute in all `.dnn`
manifest files exactly as the pre-change version did. Diff the manifest files against the
output produced before the change — they MUST be identical except for the expected version
bump.

## 6. PR checklist

Before requesting review, confirm each item:

- [ ] Step 1 build succeeds on both TFMs.
- [ ] Step 2 tests are green.
- [ ] Step 3 no-regression check is clean.
- [ ] Step 4 SonarQube (or grep fallback) shows S5445 and S6703 cleared.
- [ ] Step 5 build-task smoke test produces identical manifest output.
- [ ] PR description references rules `csharpsquid:S5445` and `secrets:S6703` and links to
      the verification artifact from step 4.
- [ ] Constitution Check section of `plan.md` is up to date; Complexity Tracking entries
      still hold.
