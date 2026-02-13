using TimeWarp.Nuru;

namespace DevCli.Endpoints;

[NuruRoute("help", Description = "Show help information")]
public sealed class HelpQuery : IQuery<Unit>
{
  public sealed class Handler : IQueryHandler<HelpQuery, Unit>
  {
    public ValueTask<Unit> Handle(HelpQuery query, CancellationToken ct)
    {
      Console.WriteLine("dev - Development CLI for TimeWarp.Amuru");
      Console.WriteLine("");
      Console.WriteLine("Usage: dev <command>");
      Console.WriteLine("");
      Console.WriteLine("Commands:");
      Console.WriteLine("  build          Build the TimeWarp.Amuru project");
      Console.WriteLine("  test           Run the integration test suite");
      Console.WriteLine("  clean          Clean build artifacts and caches");
      Console.WriteLine("  self-install   AOT compile this CLI to ./bin/dev");
      Console.WriteLine("  check-version  Check if current version exists on NuGet.org");
      Console.WriteLine("  help           Show this help message");
      Console.WriteLine("  capabilities   Output JSON for AI agent discovery");
      Console.WriteLine("");
      Console.WriteLine("Run 'dev <command> --help' for more information on a command.");

      return ValueTask.FromResult(Unit.Value);
    }
  }
}
