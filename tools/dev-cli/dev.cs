#region Purpose
// Dev CLI entry point - discovers and runs Nuru endpoints
#endregion

using TimeWarp.Nuru;

NuruApp app = NuruApp.CreateBuilder()
  .WithName("dev")
  .WithDescription("Development CLI for timewarp-amuru")
  .DiscoverEndpoints()
  .Build();

return await app.RunAsync(args);
