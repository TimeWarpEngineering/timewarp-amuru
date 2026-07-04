# Scope the 1.0 public API surface

## Description

The package currently exposes **123 public types**; each is a semver compatibility commitment at 1.0. Several are app-level utilities, placeholders, or implementation details that don't belong in a core process-execution library's frozen contract. Decide keep/internalize/split/delete for each before freezing.

**BLOCKER for 1.0** (contains two items that must not ship as-is: `Post` and `SshKeyHelper`).

## Checklist

### Must not ship as-is
- [ ] `native/utilities/Post.cs:36-54` — `ToNostr`/`ToX` are placeholder no-ops that print "Would post..." and return `true` (success); `ToAll` composes them. Delete (or move to an experimental package) — shipping a no-op that reports success is a trust bug
- [ ] `native/utilities/SshKeyHelper.cs` — broken and dangerous:
  - `GenerateKeyPairAsync` (:52) passes the entire flag string as ONE argv element via `WithArguments(params string[])` — ssh-keygen always fails; the method can never succeed as written
  - `ChangePassphraseAsync` (:143-159) feeds passphrases via stdin but `ssh-keygen -p` prompts on `/dev/tty` — hangs in scripted/CI use
  - `ConvertKeyFormatAsync` (:195-201) destructively rewrites the INPUT key in place (stripping any passphrase), then copies input over output
  - Fix all three, or remove/mark `[Experimental]` for 1.0

### Decide keep/internalize/split
- [ ] `Installer.cs` — downloads GitHub release binaries, interactive prompts; app-level, not library-level. If kept public, fix: PowerShell extraction via string interpolation permits injection for paths containing `'` (:304-309, use `ZipFile` instead); fixed predictable temp path enables TOCTOU (:63-95, use random temp file); hardcoded x64 fails on arm64/Apple Silicon (:207-214); cleanup not in `finally` (:172-174)
- [ ] `repo/` services (`RepoCleanService`, `RepoCheckVersionService`) — TimeWarp-internal conventions (hardcoded `source/Directory.Build.props` path); awkward to support under semver (bug fixes tracked in task 098 regardless)
- [ ] `nu-get/` `NuGetPackageService` — same question
- [ ] `native/utilities/GenerateColor`, `ConvertTimestamp` — keep or split to utilities package
- [ ] `git-commands/Git.WorktreePorcelainParser.cs:10` — public parsing implementation detail; make internal (`WorktreeEntry` also lacks the `Git` prefix its sibling records use)
- [ ] `extensions/CommandBuilderExtensions` (`When`/`WhenNotNull`/`Unless`/`Apply`/`ForEach`/`Tap`) — zero usage in tests or samples, and only works on 14 of ~74 builders. Cut or commit (existing task 043 tracks the evaluation; task 100 tracks the rollout if kept)

### Wrap-up
- [ ] Public-API diff after scoping; record the accepted 1.0 surface

## Notes

Found by multi-agent release review (2026-07-04). Do this FIRST — tasks 093 (XML docs) and 096 (AOT) get cheaper for every type removed from the surface. Paths relative to `source/timewarp-amuru/`.
