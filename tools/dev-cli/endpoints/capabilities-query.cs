using TimeWarp.Nuru;

namespace DevCli.Endpoints;

[NuruRoute("capabilities", Description = "Output JSON for AI agent discovery")]
public sealed class CapabilitiesQuery : IQuery<Unit>
{
  public sealed class Handler : IQueryHandler<CapabilitiesQuery, Unit>
  {
    public ValueTask<Unit> Handle(CapabilitiesQuery query, CancellationToken ct)
    {
      // Output JSON directly without reflection-based serialization (AOT-compatible)
      Console.WriteLine("""
        {
          "name": "dev",
          "version": "0.1.0",
          "description": "Development CLI for TimeWarp.Amuru",
          "commands": [
            {
              "pattern": "build",
              "description": "Build the TimeWarp.Amuru project",
              "options": [
                { "name": "verbose", "shortName": "v", "description": "Show detailed output", "type": "bool" }
              ]
            },
            { "pattern": "test", "description": "Run the integration test suite", "options": [] },
            { "pattern": "clean", "description": "Clean build artifacts and caches", "options": [] },
            { "pattern": "self-install", "description": "AOT compile this CLI to ./bin/dev", "options": [] },
            { "pattern": "check-version", "description": "Check if current version exists on NuGet.org", "options": [] }
          ]
        }
        """);

      return ValueTask.FromResult(Unit.Value);
    }
  }
}
