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
// UseTelemetry is on so a CI job or a ganda walk that shells out to dev
// exports one command span per route, including the DevCli routes. The OTLP
// endpoint stays unset here: a non-literal OtlpEndpoint is ignored, and an
// unset OTEL_EXPORTER_OTLP_ENDPOINT exports nothing. ServiceName is the
// literal timewarp-amuru-dev. OTEL_SERVICE_NAME overrides it. This repo has
// no ActivitySource of its own, so none is registered. IncomingTraceContext
// restores TRACEPARENT before RunAsync because TelemetryBehavior parents its
// span to Activity.Current.
//
// Usage:
//   As runfile:  dotnet run --file tools/dev-cli/dev.cs -- <command>
//   As AOT:      ./bin/dev <command>   (bootstrap: dotnet run --file tools/dev-cli/dev.cs -- self-install)
#endregion

using TimeWarp.Nuru;

NuruApp app = NuruApp.CreateBuilder()
  .WithName("dev")
  .WithDescription("Development CLI for timewarp-amuru")
  .UseTelemetry(options => options.ServiceName = "timewarp-amuru-dev")
  .ConfigureServices(services =>
  {
    services.AddSingleton<IRepoCleanService, RepoCleanService>();
    services.AddSingleton<NuGetVersionService>();
    services.AddSingleton<IRepoConfigService, RepoConfigService>();
    services.AddSingleton<IPackableProjectService, PackableProjectService>();
  })
  .DiscoverEndpoints()
  .Build();

using IDisposable? incomingTrace = IncomingTraceContext.Restore();
return await app.RunAsync(args);
