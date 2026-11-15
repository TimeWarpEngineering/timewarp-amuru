# Round 1 — general
**Date:** 2026-11-15
**Scope reviewed:** branch `task/128-cscheck-pilot-quote-then-parse-round-trip-property` vs merge-base `63a9997` (`master`), including the implementer restatement in `task.md`

## Summary

The change adds a test-only CsCheck 4.9.1 property that builds a small argv-echo child and asserts `Shell.Builder(...).WithArguments(...)` returns those strings unchanged. That matches the groomed contract in the task (CliWrap escaping, checked through a real child) and the implementer restatement. The opening description's "parse back" wording is the same outcome: CliWrap quotes into one command line, and the runtime parses it into the child's argv. No misread.

Generators cover empty lists, empty strings, spaces, quotes, backslashes, unicode (no NUL, no lone surrogates), and shell metacharacters. The report is a count, Base64 lines, and an `end` marker, so newlines and trailing empty arguments survive `CommandOutput`'s line capture. `CsCheck_Seed` is set on the CI step that runs `dev workflow` → `dotnet run` the aggregate suite, which inherits the process environment. There is no `Faster`. CsCheck is not referenced under `source/`. Recorded gates (`./bin/dev build`, `./bin/dev test` 878 passed / 1 skipped, `ganda repo audit` clean) and `### How to validate` cover this delta. This review did not re-run those gates. Windows was not run; the checklist stays open and Results say so, which is what the task asked for when no Windows host is available.

## Issues
