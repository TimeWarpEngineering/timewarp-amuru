#!/usr/bin/env -S dotnet --
#region Purpose
// Dev CLI entry point - discovers and runs Nuru endpoints
#endregion
#region Design
// Thin Nuru wrapper. Shared endpoints (clean, check-version, release, self-install)
// come from TimeWarp.Nuru.DevCli and need the services registered below. Local
// endpoints: build, test, verify-samples, workflow (pr/merge builds and tests;
// release promotes the CI-built Packages-* artifact — see
// documentation/developer/guides/releasing.md).
//
// Usage:
//   As runfile:  dotnet run --file tools/dev-cli/dev.cs -- <command>
//   As AOT:      ./bin/dev <command>   (bootstrap: dotnet run --file tools/dev-cli/dev.cs -- self-install)
#endregion

using TimeWarp.Nuru;

NuruApp app = NuruApp.CreateBuilder()
  .WithName("dev")
  .WithDescription("Development CLI for timewarp-amuru")
  .ConfigureServices(services =>
  {
    services.AddSingleton<IRepoCleanService, RepoCleanService>();
    services.AddSingleton<NuGetVersionService>();
    services.AddSingleton<IRepoConfigService, RepoConfigService>();
    services.AddSingleton<IPackableProjectService, PackableProjectService>();
  })
  .DiscoverEndpoints()
  .Build();

return await app.RunAsync(args);
