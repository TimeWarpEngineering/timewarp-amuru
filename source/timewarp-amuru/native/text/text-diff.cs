#region Purpose
// Builds a small unified diff for ReplaceInFiles dry-run previews.
#endregion

#region Design
// Line LCS is used while both sides stay modest. Larger texts fall back to one hunk
// that deletes the old lines and adds the new ones, which is still a valid unified diff.
// A trailing newline is not an extra empty line; its absence is marked explicitly.
#endregion

namespace TimeWarp.Amuru.Native.Text;

internal static class TextDiff
{
  private const int Context = 3;
  private const int MaxCells = 250_000;

  public static IReadOnlyList<string> Unified(string path, string before, string after)
  {
    ArgumentNullException.ThrowIfNull(path);
    ArgumentNullException.ThrowIfNull(before);
    ArgumentNullException.ThrowIfNull(after);

    string[] oldLines = Split(before);
    string[] newLines = Split(after);
    List<Edit> edits = BuildEdits(oldLines, newLines);
    List<Positioned> positioned = Position(edits);
    List<(int Start, int End)> ranges = Hunks(positioned);
    if (ranges.Count == 0)
    {
      return [];
    }

    List<string> lines =
    [
      "--- " + path,
      "+++ " + path
    ];

    bool oldNewline = EndsWithNewline(before);
    bool newNewline = EndsWithNewline(after);
    foreach ((int start, int end) in ranges)
    {
      AppendHunk(lines, positioned, start, end, oldLines.Length, newLines.Length, oldNewline, newNewline);
    }

    return lines;
  }

  private static void AppendHunk(
    List<string> lines,
    List<Positioned> positioned,
    int start,
    int end,
    int oldLength,
    int newLength,
    bool oldNewline,
    bool newNewline)
  {
    int oldStart = 0;
    int oldCount = 0;
    int newStart = 0;
    int newCount = 0;
    for (int index = start; index <= end; index++)
    {
      Positioned item = positioned[index];
      if (item.Kind != '+')
      {
        if (oldCount == 0)
        {
          oldStart = item.OldLine;
        }

        oldCount++;
      }

      if (item.Kind != '-')
      {
        if (newCount == 0)
        {
          newStart = item.NewLine;
        }

        newCount++;
      }
    }

    if (oldCount == 0)
    {
      oldStart = start == 0 ? 0 : positioned[start].OldLine;
    }

    if (newCount == 0)
    {
      newStart = start == 0 ? 0 : positioned[start].NewLine;
    }

    lines.Add(
      string.Create(
        CultureInfo.InvariantCulture,
        $"@@ -{oldStart},{oldCount} +{newStart},{newCount} @@"));

    bool sawOldEnd = false;
    bool sawNewEnd = false;
    for (int index = start; index <= end; index++)
    {
      Positioned item = positioned[index];
      lines.Add(item.Kind + item.Text);
      if (item.Kind != '+' && item.OldLine == oldLength)
      {
        sawOldEnd = true;
      }

      if (item.Kind != '-' && item.NewLine == newLength)
      {
        sawNewEnd = true;
      }
    }

    if ((sawOldEnd && !oldNewline) || (sawNewEnd && !newNewline))
    {
      lines.Add("\\ No newline at end of file");
    }
  }

  private static List<Positioned> Position(List<Edit> edits)
  {
    List<Positioned> positioned = [];
    int oldLine = 1;
    int newLine = 1;
    foreach (Edit edit in edits)
    {
      positioned.Add(new Positioned(edit.Kind, edit.Text, oldLine, newLine));
      if (edit.Kind != '+')
      {
        oldLine++;
      }

      if (edit.Kind != '-')
      {
        newLine++;
      }
    }

    return positioned;
  }

  private static List<(int Start, int End)> Hunks(List<Positioned> positioned)
  {
    List<(int Start, int End)> ranges = [];
    for (int index = 0; index < positioned.Count; index++)
    {
      if (positioned[index].Kind == ' ')
      {
        continue;
      }

      int start = Math.Max(0, index - Context);
      int end = Math.Min(positioned.Count - 1, index + Context);
      if (ranges.Count > 0 && start <= ranges[^1].End + 1)
      {
        ranges[^1] = (ranges[^1].Start, Math.Max(ranges[^1].End, end));
      }
      else
      {
        ranges.Add((start, end));
      }
    }

    return ranges;
  }

  private static List<Edit> BuildEdits(string[] oldLines, string[] newLines)
  {
    int oldLength = oldLines.Length;
    int newLength = newLines.Length;
    if ((long)oldLength * newLength > MaxCells)
    {
      return Coarse(oldLines, newLines);
    }

    int width = newLength + 1;
    int[] length = new int[(oldLength + 1) * width];
    for (int oldIndex = oldLength - 1; oldIndex >= 0; oldIndex--)
    {
      for (int newIndex = newLength - 1; newIndex >= 0; newIndex--)
      {
        int cell = (oldIndex * width) + newIndex;
        length[cell] = oldLines[oldIndex] == newLines[newIndex]
          ? length[((oldIndex + 1) * width) + newIndex + 1] + 1
          : Math.Max(length[((oldIndex + 1) * width) + newIndex], length[(oldIndex * width) + newIndex + 1]);
      }
    }

    List<Edit> edits = [];
    int left = 0;
    int right = 0;
    while (left < oldLength && right < newLength)
    {
      if (oldLines[left] == newLines[right])
      {
        edits.Add(new Edit(' ', oldLines[left]));
        left++;
        right++;
      }
      else if (length[((left + 1) * width) + right] >= length[(left * width) + right + 1])
      {
        edits.Add(new Edit('-', oldLines[left]));
        left++;
      }
      else
      {
        edits.Add(new Edit('+', newLines[right]));
        right++;
      }
    }

    while (left < oldLength)
    {
      edits.Add(new Edit('-', oldLines[left]));
      left++;
    }

    while (right < newLength)
    {
      edits.Add(new Edit('+', newLines[right]));
      right++;
    }

    return edits;
  }

  private static List<Edit> Coarse(string[] oldLines, string[] newLines)
  {
    List<Edit> edits = [];
    foreach (string line in oldLines)
    {
      edits.Add(new Edit('-', line));
    }

    foreach (string line in newLines)
    {
      edits.Add(new Edit('+', line));
    }

    return edits;
  }

  private static string[] Split(string text)
  {
    if (text.Length == 0)
    {
      return [];
    }

    string normalized = NewlineStyle.ToLineFeed(text);
    if (EndsWithNewline(text))
    {
      normalized = normalized[..^1];
    }

    if (normalized.Length == 0)
    {
      return [string.Empty];
    }

    return normalized.Split('\n');
  }

  private static bool EndsWithNewline(string text)
  {
    return text.Length > 0 && (text[^1] == '\n' || text[^1] == '\r');
  }

  private readonly record struct Edit(char Kind, string Text);

  private readonly record struct Positioned(char Kind, string Text, int OldLine, int NewLine);
}
