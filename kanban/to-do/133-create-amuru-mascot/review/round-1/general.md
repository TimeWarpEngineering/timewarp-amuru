# Round 1 — general
**Date:** 2026-10-23
**Scope reviewed:** `task/133-create-amuru-mascot` vs `master` (e96789b): `assets/mascot/*.png`, `readme.md`, `.gitignore`, `Directory.Packages.props`, `.github/workflows/workflow.yml`, task file.

## Summary

The change adds the five Amuru kit PNGs and puts a centered hero at the top of the readme. All other diff lines come from `ganda repo audit --fix`, which is policy. Risk is low. I checked each claim in the task against the repo and found no defects.

Verified:

- `cmp` against `/home/steve/masoko/mascot-kit/` shows all five PNGs are byte-identical to the kit, with the same file names.
- The task's "How to validate" smoke script prints `ok`. It checks dimensions, color type, sha256, and corner colors (white, Night #06161E, alpha 0), plus readme lines 1-3.
- The `readme.md` diff adds two lines only: the ASCII `<p align="center"><img …></p>` line and a blank line. Every existing line is unchanged. The alt text and width match the implementation notes.
- The hero art shows a single chest letter A. The readme adds no new character claims and no caption.
- Checklist items 1-5 are ticked, and "Steven approves the final art" is still open.
- The only `TimeWarp.Nuru` consumer is `tools/dev-cli`. The Nuru 3→4 major bump is policy and was checked only for breakage: `dotnet build tools/dev-cli/dev.cs` succeeds, and `bin/dev --help` runs.
- The audit-fix changes to the workflow trigger and `.gitignore` are additive.
- No library source changed.

Observation for Steven's art approval, not a finding: the requirement asks for a "blue eye mask", but the kit's mask is violet. The implementer was told not to redraw, so this belongs to the open approval item, not to this diff.

## Issues

None.
