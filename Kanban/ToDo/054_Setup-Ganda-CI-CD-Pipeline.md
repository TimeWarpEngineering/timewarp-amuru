# Setup Ganda CI/CD Pipeline

## Description

Create GitHub Actions workflow for the private Ganda repository. The pipeline should build both TimeWarp.Zana and TimeWarp.Ganda, run tests, and publish packages to GitHub Packages on release.

## Parent

Migration Analysis: `.agent/workspace/2025-12-06T18-30-00_ganda-migration-analysis.md`

## Requirements

- Build both Zana (library) and Ganda (CLI tool)
- Publish to GitHub Packages (private NuGet feed)
- Run on push to master, PRs, and releases

## Checklist

### Workflow Setup
- [ ] Create `.github/workflows/ci-cd.yml`
- [ ] Configure .NET 10 preview SDK
- [ ] Add build steps for both projects

### Package Publishing
- [ ] Configure GitHub Packages as NuGet source
- [ ] Publish TimeWarp.Zana package
- [ ] Publish TimeWarp.Ganda package
- [ ] Use `--skip-duplicate` to handle re-runs

### Permissions
- [ ] Set `contents: write` permission
- [ ] Set `packages: write` permission

## Notes

Publishing to GitHub Packages:
```yaml
dotnet nuget push "packages/*.nupkg" \
  --api-key ${{ secrets.GITHUB_TOKEN }} \
  --source "https://nuget.pkg.github.com/TimeWarpEngineering/index.json"
```

Consuming from GitHub Packages requires authentication in nuget.config.
