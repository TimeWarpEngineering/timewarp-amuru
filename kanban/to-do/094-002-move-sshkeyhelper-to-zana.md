# Move SshKeyHelper to Zana

## Description

`native/utilities/SshKeyHelper.cs` moves out of Amuru into Zana (timewarp-ganda repo). Ganda's `ssh-key-helper` endpoint is the ONLY consumer, ganda is private and already depends on Zana, and the class is broken as shipped — it must not be part of any public 1.0 surface. No `InternalsVisibleTo` bridge; a clean move.

## Checklist

### Amuru side (this repo)
- [ ] Remove `native/utilities/SshKeyHelper.cs` from the package (delete once the Zana copy lands; coordinate so ganda never has a gap)
- [ ] Build + suite green; confirm nothing else references it

### Ganda side (track on ganda's board — create the task there)
- [ ] Port implementation into Zana `native/utilities/`, FIXING the confirmed bugs during the port:
  - `GenerateKeyPairAsync` passes the entire flag string as one argv element — ssh-keygen always fails. Use discrete `WithArguments` args
  - `ChangePassphraseAsync` feeds passphrases via stdin but `ssh-keygen -p` prompts on `/dev/tty` — hangs in CI. Use `-P`/`-N` args
  - `ConvertKeyFormatAsync` destructively rewrites the INPUT key in place (strips passphrase), then copies input over output. Operate on a copy
- [ ] Update ganda `endpoints/ssh-key-helper-command.cs` and `global-usings.cs` (drops `TimeWarp.Amuru.Native.Utilities` import for this)
- [ ] Tests for the three fixed methods

## Notes

Sequencing: ganda-side port lands first, then ganda bumps its Amuru reference past the version that removes the class. Consumer map from `094-scope-the-10-public-api-surface/review-findings.md`.

## Session

- Created: 2026-07-05 (decision record in parent 094 task.md)
