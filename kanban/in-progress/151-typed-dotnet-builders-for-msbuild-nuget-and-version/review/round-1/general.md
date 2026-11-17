# Round 1 — general
**Date:** 2026-11-18
**Scope reviewed:** branch `task/151-typed-dotnet-builders-for-msbuild-nuget-and` vs merge-base `bfee4147`. `dot-net.ms-build.cs`, `dot-net.nu-get.verify.cs`, the `partial` split of `DotNetNuGetBuilder`, `DotNetBuildTelemetry.Tag("msbuild", ...)`, `PublicAPI.Unshipped.txt`, `dot-net.md`, release notes `3.0.0-beta.7.md`, `Directory.Build.props`, and the Jaribu runfiles `dot-net.ms-build.cs` and `dot-net.nuget.cs`. Compared with `dot-net.build.cs`, `DotNetNuGetPushBuilder`, `ICommandBuilder`, and `ThrowIfStrictNonZero`.

## Summary

`DotNet.MsBuild()` / `DotNet.MsBuild(project)` and `DotNet.NuGet().Verify(packagePath)` match the task and the implementer restatement. Repeatable `-t:`, `-p:`, and `-get*` switches keep call order; a repeated property name replaces in place, which `dot-net.md` and the msbuild runfile both state. The builder calls `DotNetBuildTelemetry.Tag` and `Shell.Run("dotnet", ..., Options)`, so the existing build tags and start-time `TRACEPARENT` path apply. Verify follows the other nuget sub-builders (parent options copied in, no span tags). The push-key test asserts the key is on the command string and absent from activity tags, events, and the display name. No `DotNet.Version()` or `DotNet.Info()` was added. Public API, docs, and the `3.0.0-beta.7` bump are present.

The restatement matches the task. Putting `Verify` in `dot-net.nu-get.verify.cs` is the line-limit split the session records: `dot-net.nu-get.cs` stays at 1801 lines, and the new files are under the 500-line cap. That is not a misread.

`## Results` and `### How to validate` cover this diff: `./bin/dev build` pass, msbuild runfile 12/12, nuget runfile 16/16, build-telemetry runfile 14/14 after the comment-only telemetry edit, and `ganda repo audit` pass. Those gates were not re-run.

One targeted check, not a finding: CliWrap 3.10.5 `CommandExecutionException` takes the caller message as the exception message (`strings -el` on the package plus the 3.10.5 source). `ThrowIfStrictNonZero` passes a message that contains only the exit code, so a strict-exit failure does not put the push api key in the exception message. The key remains on the process argv, which `dotnet nuget push` requires.

## Issues

<!-- None. -->
