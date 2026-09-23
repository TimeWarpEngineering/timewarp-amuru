#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for Commands.GetItemProperty - size, type, missing path, symlink target
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Commands_
{
  [TestTag("Native")]
  public class GetItemProperty_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<GetItemProperty_Given_>();

    public static async Task File_Should_ReportSizeAndType()
    {
      string path = Path.GetTempFileName();
      await File.WriteAllTextAsync(path, "hello");

      try
      {
        CommandOutput result = Commands.GetItemProperty(path);

        result.Success.ShouldBeTrue();
        result.Stdout.ShouldContain("Type: File");
        result.Stdout.ShouldContain("Size: 5");
        result.Stdout.ShouldContain("Modified:");
      }
      finally
      {
        File.Delete(path);
      }
    }

    public static async Task Directory_Should_ReportType()
    {
      string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(path);

      try
      {
        CommandOutput result = Commands.Stat(path);

        result.Success.ShouldBeTrue();
        result.Stdout.ShouldContain("Type: Directory");
      }
      finally
      {
        Directory.Delete(path);
      }

      await Task.CompletedTask;
    }

    public static async Task MissingPath_Should_Fail()
    {
      string missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

      CommandOutput result = Commands.GetItemProperty(missing);

      result.Success.ShouldBeFalse();
      result.Stderr.ShouldContain("No such file");

      await Task.CompletedTask;
    }

    public static async Task FileSymlink_Should_ReportLinkTarget()
    {
      string target = Path.GetTempFileName();
      string link = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".link");
      await File.WriteAllTextAsync(target, "target");

      try
      {
        try
        {
          File.CreateSymbolicLink(link, target);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
          throw new InvalidOperationException(
            "Symlink creation is required for this test. Run on Unix or enable Windows Developer Mode.",
            ex);
        }

        CommandOutput result = Commands.GetItemProperty(link);

        result.Success.ShouldBeTrue();
        result.Stdout.ShouldContain("LinkTarget:");
        result.Stdout.ShouldContain(Path.GetFileName(target));
      }
      finally
      {
        if (File.Exists(link))
        {
          File.Delete(link);
        }

        File.Delete(target);
      }
    }
  }
}
