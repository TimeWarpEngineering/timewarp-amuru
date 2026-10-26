# Rename amuru skill to tw-amuru for naming consistency

## Description

Every TimeWarp-owned skill in the ganda skill set carries the `tw-` prefix (`tw-nuru`, `tw-jaribu`, `tw-kanban`, `tw-git`, …). The only exception is this repo's `skills/amuru/SKILL.md`, registered and installed as plain `amuru`. The unprefixed skills left in the set (`herdr`, `roslynk`) are third-party, so `amuru` reads as external when it is not. Steven asked for the rename 2026-10-26.

Repo side (this task): move the folder, rename the skill, fix in-repo references. Operator side (not a repo change, recorded in Notes): re-register the skill source under the new name and sync.

## Requirements

- `git mv skills/amuru skills/tw-amuru`. The folder holds only `SKILL.md`.
- `skills/tw-amuru/SKILL.md` frontmatter: `name: tw-amuru`. Description unchanged. Body unchanged except any self-reference to the old name or path.
- Update every in-repo reference to `skills/amuru` or the skill name `amuru` outside `kanban/done/` and `kanban/archived/` (those are history and stay). Known: `AGENTS.md` and `readme.md` if they point at the skill path; check `documentation/` and `.github/` too. The mascot asset paths `assets/mascot/amuru-*.png` are not skill references and stay.
- `./bin/dev build` and `ganda repo audit` clean. Commit whatever `ganda repo audit --fix` changes.
- No version bump; the skill is not part of the NuGet packages.

## Checklist

- [ ] Folder moved to `skills/tw-amuru/`, frontmatter `name: tw-amuru`
- [ ] In-repo references updated; `grep -rn "skills/amuru"` outside `kanban/done` and `kanban/archived` returns nothing
- [ ] `./bin/dev build` and `ganda repo audit` clean
- [ ] PR open and CI green
- [ ] Operator: skill source re-registered as `tw-amuru` and synced (after merge, see Notes)

## Notes

- Local skill source today (`~/.timewarp/skills/config.json`): `"amuru": "https://timewarp.software/"`. That URI is the TimeWarp Software showcase site, not a repo tree URL, although the installed copy matches `skills/amuru/SKILL.md` byte for byte. The replacement should follow the pattern the other repo-owned skills use. After this PR merges, run from any shell:

  ```bash
  ganda skills remove amuru
  ganda skills add tw-amuru https://github.com/TimeWarpEngineering/timewarp-amuru/tree/master/skills/tw-amuru
  ganda skills sync
  ganda skills audit
  ```

- Downstream references to the old skill name live in timewarp-flow and are out of scope here: `source/plugins/timewarp-opencode-plugin/services/skill-auto-injector.cs` lists `"amuru"`, and `opencode/overview.md` names `amuru` under library-owned skills. File a flow task to follow the rename once this merges.

## Session

- Created: 3213244 (2026-10-26)
