#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for Direct.SetLocation and its Cd alias.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: Direct
// Action: SetLocation
// SetLocation assigns the process-global working directory. Each test restores it.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Direct_
{
  [TestTag("Native")]
  public class SetLocation_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<SetLocation_Given_>();

    public static async Task ValidDirectory_Should_ChangeCurrentDirectory()
    {
      string originalDirectory = Environment.CurrentDirectory;
      string directory = Directory.CreateTempSubdirectory("amuru-direct-set-location-").FullName;
      try
      {
        Direct.SetLocation(directory);

        Direct.GetLocation().ShouldBe(directory);
        Environment.CurrentDirectory.ShouldBe(directory);
      }
      finally
      {
        Environment.CurrentDirectory = originalDirectory;
        Directory.Delete(directory);
      }

      await Task.CompletedTask;
    }

    public static async Task Cd_Should_ChangeCurrentDirectory()
    {
      string originalDirectory = Environment.CurrentDirectory;
      string directory = Directory.CreateTempSubdirectory("amuru-direct-cd-").FullName;
      try
      {
        Direct.Cd(directory);

        Direct.Pwd().ShouldBe(directory);
        Environment.CurrentDirectory.ShouldBe(directory);
      }
      finally
      {
        Environment.CurrentDirectory = originalDirectory;
        Directory.Delete(directory);
      }

      await Task.CompletedTask;
    }

    public static async Task MissingDirectory_Should_ThrowDirectoryNotFoundException()
    {
      string originalDirectory = Environment.CurrentDirectory;
      string missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

      Should.Throw<DirectoryNotFoundException>(() => Direct.SetLocation(missing));
      Environment.CurrentDirectory.ShouldBe(originalDirectory);

      await Task.CompletedTask;
    }
  }
}
