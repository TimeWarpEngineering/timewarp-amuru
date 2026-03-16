#region Purpose
// Dev CLI entry point - discovers and runs Nuru endpoints
#endregion

using TimeWarp.Nuru;

NuruApp app = NuruApp.CreateBuilder()
  .DiscoverEndpoints()
  .Build();

return await app.RunAsync(args);
