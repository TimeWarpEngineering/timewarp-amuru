#region Purpose
// Clean build artifacts via RepoCleanService, then the local NuGet feed cache.
#endregion
#region Design
// Replaces the shared DevCli clean command (excluded in Directory.Build.targets).
// Bin/obj deletion, including reparse and tracked-file guards and preservation of
// dev/dev.exe, stays in RepoCleanService. This command adds the local feed extra
// the shared command does not: nupkgs and package-id folders under artifacts/packages.
#endregion

namespace DevCli;

using TimeWarp.Amuru;
using TimeWarp.Nuru;
using TimeWarp.Terminal;

/// <summary>
/// Clean solution build artifacts and the local package feed.
/// </summary>
[NuruRoute("clean", Description = "Clean solution and build artifacts")]
public sealed class CleanCommand : ICommand<Unit>
{
  public sealed class Handler : ICommandHandler<CleanCommand, Unit>
  {
    private readonly ITerminal Terminal;
    private readonly IRepoCleanService RepoCleanService;

    public Handler(ITerminal terminal, IRepoCleanService repoCleanService)
    {
      Terminal = terminal;
      RepoCleanService = repoCleanService;
    }

    public async Task<Unit> Handle(CleanCommand command, CancellationToken cancellationToken)
    {
      ArgumentNullException.ThrowIfNull(command);

      Terminal.WriteLine("Cleaning repository...");

      CleanResult result = await RepoCleanService
        .CleanAsync(cancellationToken)
        .ConfigureAwait(false);

      Terminal.WriteLine
      (
        $"Deleted {result.ObjDirectoriesDeleted} obj directories, {result.BinDirectoriesDeleted} bin directories"
          .Green()
      );

      if (result.RootBinFilesCleaned > 0)
      {
        Terminal.WriteLine
        (
          $"Cleaned {result.RootBinFilesCleaned} files from root bin/ (preserved dev executable)"
            .Green()
        );
      }

      int feedRemoved = await RepoCleanService
        .CleanLocalFeedAsync(cancellationToken)
        .ConfigureAwait(false);

      if (feedRemoved > 0)
      {
        Terminal.WriteLine
        (
          $"Removed {feedRemoved} local feed entries from artifacts/packages"
            .Green()
        );
      }

      Terminal.WriteLine("\nClean completed successfully!".Green());
      return Unit.Value;
    }
  }
}
