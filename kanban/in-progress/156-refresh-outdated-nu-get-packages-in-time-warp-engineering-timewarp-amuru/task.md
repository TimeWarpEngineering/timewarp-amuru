# Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-amuru

## Description

Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-amuru.
Filter: (all)
Outdated:
  TimeWarp.Jaribu 1.0.0-beta.17 -> 1.0.0-beta.18 (dev-only: tests/timewarp-amuru/Directory.Build.props)
  TimeWarp.Amuru 3.0.0-beta.9 -> 3.0.0-beta.10 (dev-only: this repo produces this package)
  TimeWarp.Amuru.Tools 3.0.0-beta.9 -> 3.0.0-beta.10 (dev-only: this repo produces this package)
Every outdated pin is dev-only. Bump them. Do not change <Version> in source/Directory.Build.props: the wave merges this task and does not release this repo.
Edit Directory.Packages.props in this repo only.

## Session

- Minted by ganda task wave run 20261119T023451Z-133310
