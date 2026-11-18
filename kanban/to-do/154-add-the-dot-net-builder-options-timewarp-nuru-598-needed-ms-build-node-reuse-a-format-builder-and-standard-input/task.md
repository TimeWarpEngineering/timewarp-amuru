# Add the DotNet builder options timewarp-nuru 598 needed: MsBuild node reuse, a Format builder, and standard input

## Description

timewarp-nuru task 598 moved its raw `Shell.Builder("dotnet")` calls to the typed `DotNet.*`
builders for AMURU0001 (3.0.0-beta.9). Three calls could not use a typed builder because the
builder lacks an option. They run on a fallback today:

| Call (timewarp-nuru) | Missing in Amuru.Tools | Fallback in nuru |
|----------------------|------------------------|------------------|
| `dotnet msbuild <proj> -nologo -nodeReuse:false -getProperty:IsPackable,PackageId` (DevCli `packable-project-service.cs`) | `DotNetMsBuildBuilder` has no node-reuse option (`-nodeReuse:false`) | `DotNet.Builder().WithArguments("msbuild", …)` |
| `dotnet format <sln> --severity warn --exclude … [--verify-no-changes]` (`tools/dev-cli/endpoints/format-command.cs`) | no `DotNet.Format()` builder (AMURU0001 reports "dotnet verb has no typed builder") | `DotNet.Builder().WithArguments("format", …)` |
| `dotnet run <file> -- …` with text piped to stdin (`tests/…/routing-33b-json-args-stdin-and-defaults.cs`, `tests/timewarp-nuru-daemon-tests/daemon-03-served-app.cs`) | `DotNetRunBuilder` and `DotNetBuilder` have no `WithStandardInput(string)`; `DotNetRunBuilder` also has no `WithTimeout(TimeSpan)` | `Shell.Run("dotnet", args, options, standardInput)` |

## Requirements

- `DotNetMsBuildBuilder.WithNodeReuse(bool)` (emits `-nodeReuse:false` / `-nodeReuse:true`).
- `DotNet.Format()` builder: project/solution path, `--severity`, `--exclude` (repeatable),
  `--verify-no-changes`, plus the usual execution methods and `Build()` (for `ToCommandString()`).
- `WithStandardInput(string)` on `DotNetRunBuilder` and `DotNetBuilder`; `WithTimeout(TimeSpan)`
  on `DotNetRunBuilder`.

## Checklist

- [ ] `DotNetMsBuildBuilder.WithNodeReuse(bool)`
- [ ] `DotNet.Format()` builder
- [ ] `WithStandardInput` on `DotNetRunBuilder` / `DotNetBuilder`, `WithTimeout` on `DotNetRunBuilder`
- [ ] Released, then timewarp-nuru moves the fallbacks to the typed builders

## Session

- Created: 1966318 (2026-11-18), filed from timewarp-nuru 598

## Notes

Source: timewarp-nuru task 598 (Amuru 3.0.0-beta.9 bump). The nuru fallbacks carry a comment
naming this task.
