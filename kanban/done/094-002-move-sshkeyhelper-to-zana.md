# Move SshKeyHelper to Zana

## Description

`native/utilities/SshKeyHelper.cs` moves out of Amuru into Zana (timewarp-ganda repo). Ganda's `ssh-key-helper` endpoint is the ONLY consumer, ganda is private and already depends on Zana, and the class is broken as shipped — it must not be part of any public 1.0 surface. No `InternalsVisibleTo` bridge; a clean move.

## Checklist

### Amuru side (this repo)
- [x] `native/utilities/SshKeyHelper.cs` deleted from Amuru (2026-07-05; ganda keeps working on its pinned beta.34 package until it bumps)
- [x] Build + suite green (416/417); no remaining references

### Ganda side — DONE 2026-07-05 in worktree Cramer-2026-06-29-dev, commit a7001fc
- [x] Ported to `source/timewarp-zana/native/utilities/ssh-key-helper.cs` with all three bugs FIXED, each verified against real ssh-keygen:
  - `GenerateKeyPairAsync` passes the entire flag string as one argv element — ssh-keygen always fails. Use discrete `WithArguments` args
  - `ChangePassphraseAsync` feeds passphrases via stdin but `ssh-keygen -p` prompts on `/dev/tty` — hangs in CI. Use `-P`/`-N` args
  - `ConvertKeyFormatAsync` destructively rewrites the INPUT key in place (strips passphrase), then copies input over output. Operate on a copy
- [x] ganda `global-usings.cs` drops `TimeWarp.Amuru.Native.Utilities`; endpoint resolves `SshKeyHelper` from `TimeWarp.Zana` unchanged; full ganda solution builds clean
- [ ] Tests for the three fixed methods (ganda-repo follow-up; invocation patterns verified manually end-to-end)

## Notes

Sequencing: ganda-side port lands first, then ganda bumps its Amuru reference past the version that removes the class. Consumer map from `094-scope-the-10-public-api-surface/review-findings.md`.

## Session

- Created: 2026-07-05 (decision record in parent 094 task.md)
