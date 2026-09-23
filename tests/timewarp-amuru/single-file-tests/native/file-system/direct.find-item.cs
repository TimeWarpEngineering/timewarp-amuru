#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for Direct.FindItem - streaming name match and cancellation
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Direct_
{
  [TestTag("Native")]
  public class FindItem_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<FindItem_Given_>();

    public static async Task NameGlob_Should_StreamMatches()
    {
      string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(Path.Combine(root, "nested"));
      await File.WriteAllTextAsync(Path.Combine(root, "one.cs"), "1");
      await File.WriteAllTextAsync(Path.Combine(root, "nested", "two.cs"), "2");
      await File.WriteAllTextAsync(Path.Combine(root, "skip.txt"), "s");

      try
      {
        List<string> names = [];
        await foreach (FileSystemInfo entry in Direct.FindItem(root, new FindCriteria { Name = "*.cs" }))
        {
          names.Add(entry.Name);
        }

        names.ShouldContain("one.cs");
        names.ShouldContain("two.cs");
        names.ShouldNotContain("skip.txt");
      }
      finally
      {
        Directory.Delete(root, recursive: true);
      }
    }

    public static async Task Cancellation_Should_ThrowOperationCanceledException()
    {
      string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(root);
      for (int i = 0; i < 20; i++)
      {
        await File.WriteAllTextAsync(Path.Combine(root, $"f{i}.txt"), "x");
      }

      try
      {
        using CancellationTokenSource cts = new();
        List<FileSystemInfo> readEntries = [];

        await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
          await foreach (FileSystemInfo entry in Direct.FindItem(root).WithCancellation(cts.Token))
          {
            readEntries.Add(entry);
            if (readEntries.Count == 1)
            {
              await cts.CancelAsync();
            }
          }
        });

        readEntries.Count.ShouldBeGreaterThanOrEqualTo(1);
      }
      finally
      {
        Directory.Delete(root, recursive: true);
      }
    }
  }
}
