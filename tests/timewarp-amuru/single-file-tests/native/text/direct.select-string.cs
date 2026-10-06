#!/usr/bin/env -S dotnet --

#region Purpose
// Direct.SelectString: streaming TextMatch, context, and exceptions.
#endregion

#region Design
// SUT: Text.Direct. Bad patterns throw at the call. Missing files throw when enumerated.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace TextDirect_
{
using Text = TimeWarp.Amuru.Native.Text;

[TestTag("Native")]
public class SelectString_Given_
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<SelectString_Given_>();

  public static async Task RegexAndLiteral_Should_SelectTheMatchingLines()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "a.b\naxb\n");
      List<Text.TextMatch> regex = await Collect(Text.Direct.SelectString("a.b", path));
      regex.Count.ShouldBe(2);
      regex[0].Line.ShouldBe("a.b");
      regex[0].LineNumber.ShouldBe(1);
      regex[0].Match.Success.ShouldBeTrue();
      regex[0].Match.Value.ShouldBe("a.b");

      Text.SelectStringOptions literal = new() { SimpleMatch = true };
      List<Text.TextMatch> simple = await Collect(Text.Direct.SelectString("a.b", path, literal));
      simple.Count.ShouldBe(1);
      simple[0].LineNumber.ShouldBe(1);
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task CaseInsensitive_Should_MatchEitherCase()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "Alpha\n");
      Text.SelectStringOptions options = new() { CaseInsensitive = true };
      List<Text.TextMatch> matches = await Collect(Text.Direct.SelectString("alpha", path, options));

      matches.Count.ShouldBe(1);
      matches[0].Line.ShouldBe("Alpha");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task Invert_Should_YieldLinesThatDoNotMatch()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "keep\ndrop\n");
      Text.SelectStringOptions options = new() { InvertMatch = true };
      List<Text.TextMatch> matches = await Collect(Text.Direct.SelectString("drop", path, options));

      matches.Count.ShouldBe(1);
      matches[0].Line.ShouldBe("keep");
      matches[0].Match.Success.ShouldBeFalse();
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task Context_Should_KeepBeforeAndAfterInOrder()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "a\nb\nhit\nc\nhit\nd\n");
      Text.SelectStringOptions options = new() { ContextBefore = 1, ContextAfter = 1 };
      List<Text.TextMatch> matches = await Collect(Text.Direct.SelectString("hit", path, options));

      matches.Count.ShouldBe(2);
      matches[0].LineNumber.ShouldBe(3);
      matches[0].ContextBefore.ShouldBe(["b"]);
      matches[0].ContextAfter.ShouldBe(["c"]);
      matches[1].LineNumber.ShouldBe(5);
      matches[1].ContextBefore.ShouldBe(["c"]);
      matches[1].ContextAfter.ShouldBe(["d"]);
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task SameLineMatches_Should_StayInLeftToRightOrder()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "aa\nzz\n");
      Text.SelectStringOptions options = new() { ContextAfter = 1 };
      List<Text.TextMatch> matches = await Collect(Text.Direct.SelectString("a", path, options));

      matches.Count.ShouldBe(2);
      matches[0].Match.Index.ShouldBe(0);
      matches[1].Match.Index.ShouldBe(1);
      matches[0].ContextAfter.ShouldBe(["zz"]);
      matches[1].ContextAfter.ShouldBe(["zz"]);
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task MaxMatches_Should_StopPerFile()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "x\nx\n");
      Text.SelectStringOptions options = new() { MaxMatches = 1 };
      List<Text.TextMatch> matches = await Collect(Text.Direct.SelectString("x", path, options));

      matches.Count.ShouldBe(1);
      matches[0].LineNumber.ShouldBe(1);
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task Glob_Should_SkipOtherExtensions()
  {
    string directory = NewDirectory();
    try
    {
      string text = await WriteAsync(directory, "hit.txt", "needle\n");
      await WriteAsync(directory, "hit.log", "needle\n");
      List<Text.TextMatch> matches = await Collect(Text.Direct.SelectString("needle", directory, "*.txt"));

      matches.Count.ShouldBe(1);
      matches[0].Path.ShouldBe(text);
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task Exclude_Should_SkipTheNamedFile()
  {
    string directory = NewDirectory();
    try
    {
      string kept = await WriteAsync(directory, "keep.txt", "needle\n");
      await WriteAsync(directory, "skip.txt", "needle\n");
      Text.SelectStringOptions options = new() { Exclude = "skip.txt" };
      List<Text.TextMatch> matches = await Collect(Text.Direct.SelectString("needle", directory, options));

      matches.Count.ShouldBe(1);
      matches[0].Path.ShouldBe(kept);
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task Groups_Should_BeOnTheMatch()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "name=value\n");
      List<Text.TextMatch> matches = await Collect(
        Text.Direct.SelectString("(?<name>\\w+)=(?<value>\\w+)", path));

      matches.Count.ShouldBe(1);
      matches[0].Match.Groups["name"].Value.ShouldBe("name");
      matches[0].Match.Groups["value"].Value.ShouldBe("value");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task Reader_Should_UseDashWhenPathIsOmitted()
  {
    using StringReader reader = new("beta\n");
    List<Text.TextMatch> matches = await Collect(Text.Direct.SelectString("beta", reader));

    matches.Count.ShouldBe(1);
    matches[0].Path.ShouldBe("-");
    matches[0].LineNumber.ShouldBe(1);
  }

  public static async Task InvalidPattern_Should_ThrowBeforeEnumeration()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "alpha\n");
      Should.Throw<ArgumentException>(() => Text.Direct.SelectString("[", path));
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task MissingFile_Should_ThrowFileNotFound()
  {
    string missing = Path.Combine(Path.GetTempPath(), "amuru-text-missing-" + Guid.NewGuid().ToString("N"));
    await Should.ThrowAsync<FileNotFoundException>(async () =>
    {
      await foreach (Text.TextMatch match in Text.Direct.SelectString("a", missing))
      {
        _ = match;
      }
    });
  }

  public static async Task MissingRoot_Should_ThrowDirectoryNotFound()
  {
    string missing = Path.Combine(Path.GetTempPath(), "amuru-text-missing-" + Guid.NewGuid().ToString("N"));
    Should.Throw<DirectoryNotFoundException>(() => Text.Direct.SelectString("a", missing, "*.txt"));
    await Task.CompletedTask;
  }

  private static async Task<List<Text.TextMatch>> Collect(IAsyncEnumerable<Text.TextMatch> matches)
  {
    List<Text.TextMatch> list = [];
    await foreach (Text.TextMatch match in matches)
    {
      list.Add(match);
    }

    return list;
  }

  private static string NewDirectory()
  {
    string path = Path.Combine(Path.GetTempPath(), "amuru-text-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(path);
    return path;
  }

  private static async Task<string> WriteAsync(string directory, string name, string content)
  {
    string path = Path.Combine(directory, name);
    await File.WriteAllTextAsync(path, content);
    return path;
  }
}
}
