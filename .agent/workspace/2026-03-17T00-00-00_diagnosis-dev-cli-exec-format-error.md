## Symptom

Running `./tools/dev-cli/dev.cs self-install` fails with:

`Exec format error` (reported via PowerShell as `ResourceUnavailable` while starting `/tools/dev-cli/dev.cs`).

## Root Cause

`tools/dev-cli/dev.cs` is no longer a directly executable script file. It does not contain a Unix shebang (`#!...`) at line 1, so invoking it as `./tools/dev-cli/dev.cs ...` causes the OS loader to reject the file format.

This behavior was introduced when the dev CLI was migrated to Nuru Endpoints and `dev.cs` was simplified; during that migration, the shebang/directive-based script header was removed.

## Evidence Chain

1. **Observed failure mode matches OS loader rejection**
   - Error text includes: `Exec format error` when trying to start `.../tools/dev-cli/dev.cs` directly.

2. **Current file header is not executable script format**
   - `tools/dev-cli/dev.cs` lines 1-5:
     - `#region Purpose`
     - comment
     - `using TimeWarp.Nuru;`
   - There is no shebang on line 1.

3. **Repository convention for directly runnable `.cs` scripts uses shebang**
   - Many scripts in repo start with `#!/usr/bin/dotnet --` (e.g., `Scripts/Build.cs`, numerous test runfiles).
   - This confirms expected direct-exec mechanism in this codebase depends on shebang presence.

4. **Git history confirms execution model change**
   - Commit `a53d606` (`feat(dev-cli): Update to TimeWarp.Nuru 3.0.0-beta.47 with Endpoints API`) removed old script-style header/directives from `tools/dev-cli/dev.cs`.
   - File changed from a long script-style implementation to a short Nuru app entrypoint.

5. **Current CI invokes dev CLI through `dotnet run`, not direct `./...` execution**
   - `.github/workflows/ci-cd.yml` line 48 uses:
     - `dotnet run tools/dev-cli/dev.cs -- self-install`
   - This aligns with the post-migration execution model.

## Affected Scope

- Any direct invocation pattern `./tools/dev-cli/dev.cs <command>` is affected.
- Commands behind this entrypoint (including `self-install`) are not the failing component; failure occurs before CLI argument handling.

## Reproduction Steps

1. From repo root, run: `./tools/dev-cli/dev.cs self-install`
2. Observe `Exec format error` while launching `tools/dev-cli/dev.cs`.

## Contributing Factors

- Historical usage may still assume script-style direct execution.
- Older task notes reference direct execution examples, which can conflict with current Nuru-based invocation pattern.

## Related History

- `a53d606` — migration to Nuru Endpoints API; script header/directive style removed from `tools/dev-cli/dev.cs`.
- CI currently reflects the new invocation pattern via `dotnet run` in `.github/workflows/ci-cd.yml`.

## Resolution

**Status: FIXED (2026-03-17)**

Added shebang `#!/usr/bin/dotnet --` to line 1 of `tools/dev-cli/dev.cs`.

The file is now directly executable as a script. Both invocation patterns now work:
- `./tools/dev-cli/dev.cs self-install` (direct script execution)
- `dotnet run tools/dev-cli/dev.cs -- self-install` (explicit dotnet run)
