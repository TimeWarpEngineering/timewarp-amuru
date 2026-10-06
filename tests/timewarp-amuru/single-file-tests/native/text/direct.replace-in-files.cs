#!/usr/bin/env -S dotnet --

#region Purpose
// Direct.ReplaceInFiles: groups, limits, options, dry run, backup, encoding, newlines,
// binary and invalid text, symlinks, and atomic write.
#endregion

#region Design
// SUT: Text.Direct. Bytes and mtime are compared so an unchanged file is proven unwritten.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace TextDirect_
{
using System.Text;
using Text = TimeWarp.Amuru.Native.Text;

[TestTag("Native")]
public class ReplaceInFiles_Given_
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<ReplaceInFiles_Given_>();

  public static async Task Groups_Should_ExpandNumberedAndNamedCaptures()
  {
    string directory = NewDirectory();
    try
    {
      string numbered = await WriteAsync(directory, "numbered.txt", "name=value\n");
      List<Text.ReplaceResult> byNumber = await Collect(
        Text.Direct.ReplaceInFiles("(\\w+)=(\\w+)", "$2:$1", numbered));
      byNumber.Count.ShouldBe(1);
      byNumber[0].ReplacementCount.ShouldBe(1);
      byNumber[0].Changed.ShouldBeTrue();
      (await File.ReadAllTextAsync(numbered)).ShouldBe("value:name\n");

      string named = await WriteAsync(directory, "named.txt", "name=value\n");
      await Collect(Text.Direct.ReplaceInFiles("(?<name>\\w+)=(?<value>\\w+)", "${value}:${name}", named));
      (await File.ReadAllTextAsync(named)).ShouldBe("value:name\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task Literal_Should_NotTreatThePatternAsRegex()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "a.b axb\n");
      Text.ReplaceInFilesOptions options = new() { SimpleMatch = true };
      await Collect(Text.Direct.ReplaceInFiles("a.b", "hit", path, options));
      (await File.ReadAllTextAsync(path)).ShouldBe("hit axb\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task MaxReplacements_Should_StopInsideTheFile()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "aaa");
      Text.ReplaceInFilesOptions options = new() { MaxReplacements = 2 };
      List<Text.ReplaceResult> results = await Collect(Text.Direct.ReplaceInFiles("a", "b", path, options));

      results[0].ReplacementCount.ShouldBe(2);
      (await File.ReadAllTextAsync(path)).ShouldBe("bba");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task MaxReplacements_Should_ApplyToEachFile()
  {
    string directory = NewDirectory();
    try
    {
      string first = await WriteAsync(directory, "a.txt", "aaa");
      string second = await WriteAsync(directory, "b.txt", "aaa");
      Text.ReplaceInFilesOptions options = new() { MaxReplacements = 1 };
      List<Text.ReplaceResult> results = await Collect(Text.Direct.ReplaceInFiles("a", "b", directory, options));

      results.Count.ShouldBe(2);
      results.ShouldAllBe(result => result.ReplacementCount == 1);
      (await File.ReadAllTextAsync(first)).ShouldBe("baa");
      (await File.ReadAllTextAsync(second)).ShouldBe("baa");
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
      string path = await WriteAsync(directory, "notes.txt", "FOO foo\n");
      Text.ReplaceInFilesOptions options = new() { CaseInsensitive = true };
      await Collect(Text.Direct.ReplaceInFiles("foo", "bar", path, options));
      (await File.ReadAllTextAsync(path)).ShouldBe("bar bar\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task Singleline_Should_LetDotMatchNewline()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "a\nb\n");
      Text.ReplaceInFilesOptions options = new() { Singleline = true };
      await Collect(Text.Direct.ReplaceInFiles("a.b", "X", path, options));
      (await File.ReadAllTextAsync(path)).ShouldBe("X\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task Multiline_Should_MatchEachLineStart()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "foo\nbar\n");
      Text.ReplaceInFilesOptions options = new() { Multiline = true };
      await Collect(Text.Direct.ReplaceInFiles("^bar", "baz", path, options));
      (await File.ReadAllTextAsync(path)).ShouldBe("foo\nbaz\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task DryRun_Should_LeaveBytesAndReportPreview()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "alpha\n");
      byte[] before = await File.ReadAllBytesAsync(path);
      DateTime stamp = Stamp(path);
      Text.ReplaceInFilesOptions options = new() { DryRun = true, Backup = true };
      List<Text.ReplaceResult> results = await Collect(
        Text.Direct.ReplaceInFiles("alpha", "beta", path, options));

      results.Count.ShouldBe(1);
      results[0].Changed.ShouldBeTrue();
      results[0].BackupPath.ShouldBeNull();
      string preview = string.Join("\n", results[0].Preview);
      preview.ShouldContain("---");
      preview.ShouldContain("-alpha");
      preview.ShouldContain("+beta");
      (await File.ReadAllBytesAsync(path)).ShouldBe(before);
      File.GetLastWriteTimeUtc(path).ShouldBe(stamp);
      File.Exists(path + ".bak").ShouldBeFalse();
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task Backup_Should_KeepTheOriginalBesideTheFile()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "alpha\n");
      Text.ReplaceInFilesOptions options = new() { Backup = true };
      List<Text.ReplaceResult> results = await Collect(Text.Direct.ReplaceInFiles("alpha", "beta", path, options));

      results[0].BackupPath.ShouldBe(path + ".bak");
      (await File.ReadAllTextAsync(path)).ShouldBe("beta\n");
      (await File.ReadAllTextAsync(path + ".bak")).ShouldBe("alpha\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task CrlfAndBom_Should_SurviveTheWrite()
  {
    string directory = NewDirectory();
    try
    {
      string path = Path.Combine(directory, "notes.txt");
      byte[] original = Bytes("alpha\r\nbeta\r\n", bom: true);
      await File.WriteAllBytesAsync(path, original);
      await Collect(Text.Direct.ReplaceInFiles("alpha", "ALPHA", path));
      byte[] updated = await File.ReadAllBytesAsync(path);

      updated.ShouldBe(Bytes("ALPHA\r\nbeta\r\n", bom: true));
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task TrailingNewline_Should_StayPresentOrAbsent()
  {
    string directory = NewDirectory();
    try
    {
      string withNewline = await WriteAsync(directory, "with.txt", "hello\n");
      string withoutNewline = await WriteAsync(directory, "without.txt", "hello");
      await Collect(Text.Direct.ReplaceInFiles("hello", "world", withNewline));
      await Collect(Text.Direct.ReplaceInFiles("hello", "world", withoutNewline));

      (await File.ReadAllTextAsync(withNewline)).ShouldBe("world\n");
      (await File.ReadAllTextAsync(withoutNewline)).ShouldBe("world");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task UnchangedFile_Should_KeepItsMtime()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "alpha\n");
      byte[] before = await File.ReadAllBytesAsync(path);
      DateTime stamp = Stamp(path);
      List<Text.ReplaceResult> results = await Collect(Text.Direct.ReplaceInFiles("missing", "beta", path));

      results[0].Changed.ShouldBeFalse();
      results[0].ReplacementCount.ShouldBe(0);
      results[0].Preview.ShouldBeEmpty();
      (await File.ReadAllBytesAsync(path)).ShouldBe(before);
      File.GetLastWriteTimeUtc(path).ShouldBe(stamp);
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task AtomicWrite_Should_NotLeaveATempFile()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "alpha\n");
      await Collect(Text.Direct.ReplaceInFiles("alpha", "beta", path, new Text.ReplaceInFilesOptions { Backup = true }));

      Directory.EnumerateFileSystemEntries(directory)
        .Select(Path.GetFileName)
        .OrderBy(name => name, StringComparer.Ordinal)
        .ShouldBe(["notes.txt", "notes.txt.bak"]);

      (await File.ReadAllTextAsync(path)).ShouldBe("beta\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task Utf8WithoutBom_Should_RoundTripNonAscii()
  {
    string directory = NewDirectory();
    try
    {
      string path = Path.Combine(directory, "notes.txt");
      Encoding utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
      await File.WriteAllBytesAsync(path, utf8.GetBytes("hello café\n"));
      await Collect(Text.Direct.ReplaceInFiles("hello", "hi", path));
      byte[] updated = await File.ReadAllBytesAsync(path);

      updated.ShouldBe(utf8.GetBytes("hi café\n"));
      updated[0].ShouldNotBe((byte)0xEF);
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task SeveralPaths_Should_VisitEveryFile()
  {
    string directory = NewDirectory();
    try
    {
      string first = await WriteAsync(directory, "a.txt", "alpha\n");
      string second = await WriteAsync(directory, "b.txt", "other\n");
      string[] paths = [first, second];
      List<Text.ReplaceResult> results = await Collect(Text.Direct.ReplaceInFiles("alpha", "beta", paths));

      results.Count.ShouldBe(2);
      results.Single(result => result.Path == first).Changed.ShouldBeTrue();
      results.Single(result => result.Path == second).Changed.ShouldBeFalse();
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task NoChange_Should_NotWriteABackup()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "alpha\n");
      Text.ReplaceInFilesOptions options = new() { Backup = true };
      List<Text.ReplaceResult> results = await Collect(Text.Direct.ReplaceInFiles("missing", "beta", path, options));

      results[0].BackupPath.ShouldBeNull();
      File.Exists(path + ".bak").ShouldBeFalse();
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task IdentityReplacement_Should_CountButNotRewrite()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "foo\n");
      DateTime stamp = Stamp(path);
      List<Text.ReplaceResult> results = await Collect(Text.Direct.ReplaceInFiles("foo", "foo", path));

      results[0].ReplacementCount.ShouldBe(1);
      results[0].Changed.ShouldBeFalse();
      File.GetLastWriteTimeUtc(path).ShouldBe(stamp);
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task CrlfFile_Should_WriteInsertedNewlinesAsCrlf()
  {
    string directory = NewDirectory();
    try
    {
      string path = Path.Combine(directory, "notes.txt");
      await File.WriteAllBytesAsync(path, Bytes("a\r\nb\r\n", bom: false));
      await Collect(Text.Direct.ReplaceInFiles("a", "x\ny", path));

      (await File.ReadAllBytesAsync(path)).ShouldBe(Bytes("x\r\ny\r\nb\r\n", bom: false));
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task LoneCarriageReturn_Should_StayLiteral()
  {
    string directory = NewDirectory();
    try
    {
      string path = Path.Combine(directory, "notes.txt");
      await File.WriteAllBytesAsync(path, Bytes("a\rb\nfoo\n", bom: false));
      await Collect(Text.Direct.ReplaceInFiles("foo", "bar", path));

      (await File.ReadAllBytesAsync(path)).ShouldBe(Bytes("a\rb\nbar\n", bom: false));
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task Latin1File_Should_ThrowAndKeepItsBytes()
  {
    string directory = NewDirectory();
    try
    {
      string path = Path.Combine(directory, "latin1.txt");
      byte[] original = [0x63, 0x61, 0x66, 0xE9, 0x0A, 0x66, 0x6F, 0x6F, 0x0A];
      await File.WriteAllBytesAsync(path, original);

      InvalidDataException exception = await Should.ThrowAsync<InvalidDataException>(
        async () => await Collect(Text.Direct.ReplaceInFiles("foo", "bar", path)));

      exception.Message.ShouldContain(path);
      (await File.ReadAllBytesAsync(path)).ShouldBe(original);
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task BinaryFile_Should_BeSkippedUntouched()
  {
    string directory = NewDirectory();
    try
    {
      string path = Path.Combine(directory, "data.bin");
      byte[] original = [.. "foo"u8, 0x00, .. "foo\n"u8];
      await File.WriteAllBytesAsync(path, original);
      DateTime stamp = Stamp(path);
      List<Text.ReplaceResult> results = await Collect(Text.Direct.ReplaceInFiles("foo", "bar", path));

      results[0].ReplacementCount.ShouldBe(0);
      results[0].Changed.ShouldBeFalse();
      (await File.ReadAllBytesAsync(path)).ShouldBe(original);
      File.GetLastWriteTimeUtc(path).ShouldBe(stamp);
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task Symlink_Should_RewriteTheTargetAndStayALink()
  {
    string directory = NewDirectory();
    try
    {
      string target = await WriteAsync(directory, "target.txt", "foo\n");
      string link = Path.Combine(directory, "link.txt");
      try
      {
        File.CreateSymbolicLink(link, target);
      }
      catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
      {
        await TimeWarpTerminal.Default.WriteLineAsync("Skipping: symbolic links are not available here.");
        return;
      }

      List<Text.ReplaceResult> results = await Collect(Text.Direct.ReplaceInFiles("foo", "bar", link));

      results[0].Changed.ShouldBeTrue();
      new FileInfo(link).LinkTarget.ShouldNotBeNull();
      (await File.ReadAllTextAsync(target)).ShouldBe("bar\n");
      Directory.EnumerateFileSystemEntries(directory)
        .Select(Path.GetFileName)
        .OrderBy(name => name, StringComparer.Ordinal)
        .ShouldBe(["link.txt", "target.txt"]);
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  private static async Task<List<Text.ReplaceResult>> Collect(IAsyncEnumerable<Text.ReplaceResult> results)
  {
    List<Text.ReplaceResult> list = [];
    await foreach (Text.ReplaceResult result in results)
    {
      list.Add(result);
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

  private static DateTime Stamp(string path)
  {
    DateTime past = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    File.SetLastWriteTimeUtc(path, past);
    return File.GetLastWriteTimeUtc(path);
  }

  private static byte[] Bytes(string text, bool bom)
  {
    byte[] body = Encoding.UTF8.GetBytes(text);
    if (!bom)
    {
      return body;
    }

    byte[] encoded = new byte[3 + body.Length];
    encoded[0] = 0xEF;
    encoded[1] = 0xBB;
    encoded[2] = 0xBF;
    body.CopyTo(encoded, 3);
    return encoded;
  }
}
}
