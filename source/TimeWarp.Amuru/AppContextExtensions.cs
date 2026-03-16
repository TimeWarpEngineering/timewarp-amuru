namespace TimeWarp.Amuru;

#pragma warning disable CA1034 // Nested types should not be visible
public static class AppContextExtensions
{
  extension(AppContext)
  {
    public static string? EntryPointFilePath() 
      => AppContext.GetData("EntryPointFilePath") as string;
    
    public static string? EntryPointFileDirectoryPath() 
      => AppContext.GetData("EntryPointFileDirectoryPath") as string;
  }
}
#pragma warning restore CA1034