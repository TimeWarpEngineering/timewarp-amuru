# Declare and validate AOT and trimming compatibility

## Description

AOT compatibility was the entire motivation for disabling StreamJsonRpc (task 084), yet the package still declares neither `IsAotCompatible` nor `IsTrimmable`, and no trim analyzer has ever run over this code (grep confirms zero hits across `source/` and `msbuild/`). Downstream `dotnet publish /p:PublishAot=true` warns on the assembly and latent trim issues ship unnoticed.

## Checklist

- [x] `IsAotCompatible` declared on BOTH packages (core + Tools) — 2026-07-05
- [x] Zero trim/AOT analyzer warnings (the `MockSetup.Throws` Activator issue was already fixed in 089)
- [x] Validated: scratch console app referencing both projects published with `PublishAot=true` — zero warnings, native binary executes Shell/Git APIs correctly
- [x] Verified: zero Newtonsoft strings in the published native binary (084's verification item)

## Notes

Found by multi-agent release review (2026-07-04). Good news already verified: no reflection-based `JsonSerializer` use anywhere (only `JsonDocument` DOM in `nu-get/`), the only `System.Reflection` use is `Installer.cs:383-387` (AOT-safe assembly attribute read), and the dependency graph (CliWrap, NuGet.Versioning, TimeWarp.Terminal) carries no Newtonsoft.
