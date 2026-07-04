# Declare and validate AOT and trimming compatibility

## Description

AOT compatibility was the entire motivation for disabling StreamJsonRpc (task 084), yet the package still declares neither `IsAotCompatible` nor `IsTrimmable`, and no trim analyzer has ever run over this code (grep confirms zero hits across `source/` and `msbuild/`). Downstream `dotnet publish /p:PublishAot=true` warns on the assembly and latent trim issues ship unnoticed.

## Checklist

- [ ] Add `<IsAotCompatible>true</IsAotCompatible>` (implies `IsTrimmable` + trim/AOT analyzers) to `source/timewarp-amuru/timewarp-amuru.csproj`
- [ ] Fix resulting analyzer warnings; known one: `testing/MockSetup.cs:65` — `Throws<TException>(message)` uses `Activator.CreateInstance(typeof(TException), message)`, not trim-safe (string ctor can be trimmed)
- [ ] Validate with a real `PublishAot=true` consumer (the AOT sample in `samples/`) — zero AOT warnings from TimeWarp.Amuru
- [ ] Complete task 084's remaining verification item: AOT consumers no longer inherit Newtonsoft.Json through Amuru

## Notes

Found by multi-agent release review (2026-07-04). Good news already verified: no reflection-based `JsonSerializer` use anywhere (only `JsonDocument` DOM in `nu-get/`), the only `System.Reflection` use is `Installer.cs:383-387` (AOT-safe assembly attribute read), and the dependency graph (CliWrap, NuGet.Versioning, TimeWarp.Terminal) carries no Newtonsoft.
