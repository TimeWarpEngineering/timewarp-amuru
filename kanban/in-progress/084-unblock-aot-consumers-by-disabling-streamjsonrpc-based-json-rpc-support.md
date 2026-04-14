# Unblock AOT consumers by disabling StreamJsonRpc-based JSON-RPC support

## Description

Temporarily disable Amuru's current `StreamJsonRpc`-based JSON-RPC feature slice so downstream AOT consumers no longer inherit the `Newtonsoft.Json` dependency and related trimming/native AOT issues.

This is an intentional interim step. The JSON-RPC feature should preserve design intent in commented/disabled code where useful, while a replacement is developed separately in task 083.

## Checklist

- [x] Remove or comment out the `StreamJsonRpc` package reference from `source/timewarp-amuru/timewarp-amuru.csproj`
- [x] Remove or comment out `global using StreamJsonRpc` from `source/timewarp-amuru/global-usings.cs`
- [x] Disable `source/timewarp-amuru/json-rpc/IJsonRpcClient.cs`
- [x] Disable `source/timewarp-amuru/json-rpc/JsonRpcClient.cs`
- [x] Disable `source/timewarp-amuru/json-rpc/JsonRpcClientBuilder.cs`
- [x] Disable `ShellBuilder.AsJsonRpcClient()` in `source/timewarp-amuru/core/shell-builder.cs`
- [ ] Add clear temporary comments explaining JSON-RPC is disabled pending task 083
- [x] Ensure the rest of Amuru builds without `StreamJsonRpc`
- [ ] Verify AOT consumers no longer inherit `Newtonsoft.Json` through Amuru
- [ ] Update any docs/samples that incorrectly imply JSON-RPC support is active

## Session

- Created: ses_27dd18c7effe1K4rnFRhnQezjn (2026-04-14)

## Notes

### Why this task exists

`StreamJsonRpc` still carries a direct `Newtonsoft.Json` dependency in current releases and prereleases. This dependency pollutes downstream consumers of Amuru and creates NativeAOT/trimming pain, even when Amuru does not use the legacy Newtonsoft formatter path at runtime.

### Scope

This task is about **temporarily disabling** the current JSON-RPC feature slice so Amuru's core package remains AOT-friendly.

It is **not** the task to implement the replacement. That work belongs in task 083:

- `083-replace-streamjsonrpc-with-amuru-native-minimal-json-rpc-client`

### What the spike proved

A local build experiment showed that Amuru builds successfully without `StreamJsonRpc` once the following are disabled:

- `global-usings.cs` reference to `StreamJsonRpc`
- `json-rpc/` source files
- `ShellBuilder.AsJsonRpcClient()`
- `timewarp-amuru.csproj` package reference

This means the dependency is isolated to the JSON-RPC feature slice and can be temporarily turned off without breaking the rest of the package.

### Current progress

- The package reference and global using are commented out
- The three `json-rpc/` source files are commented out
- `ShellBuilder.AsJsonRpcClient()` is commented out
- `dotnet build source/timewarp-amuru/timewarp-amuru.csproj` succeeds without `StreamJsonRpc`
- Remaining follow-up for this task is mainly cleanup/documentation and downstream AOT validation

### Intent

Because this is still beta and AOT matters more than preserving unfinished API surface, temporarily disabled code is acceptable here as a staging move. The goal is to unblock consumers now, while preserving enough intent to guide the native replacement.
