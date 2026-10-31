#region Purpose
// Internal glob matcher for GetChildItem / FindItem patterns (* ? **).
#endregion

#region Design
// Segment-based matching so ** crosses directories and * / ? stay within one segment.
// Windows matching is ordinal-ignore-case; Unix is ordinal. No extra globbing package
// (AOT, zero extra deps). Character classes are not supported.
#endregion

namespace TimeWarp.Amuru.Native.FileSystem;

internal static class GlobMatcher
{
  public static bool IsGlob(string path)
  {
    ArgumentNullException.ThrowIfNull(path);
    return path.Contains('*', StringComparison.Ordinal)
      || path.Contains('?', StringComparison.Ordinal);
  }

  public static (string Root, string Pattern) SplitGlobPath(string path)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(path);

    string normalized = path.Replace('\\', '/');
    int firstGlob = IndexOfGlobChar(normalized);
    if (firstGlob < 0)
    {
      return (path, "*");
    }

    int slashBefore = normalized.LastIndexOf('/', firstGlob, StringComparison.Ordinal);
    if (slashBefore < 0)
    {
      return (".", normalized);
    }

    string root = normalized[..slashBefore];
    string pattern = normalized[(slashBefore + 1)..];
    if (string.IsNullOrEmpty(root))
    {
      root = "/";
    }

    return (root, pattern);
  }

  public static bool PatternImpliesRecursive(string? pattern)
  {
    return !string.IsNullOrEmpty(pattern)
      && pattern.Contains("**", StringComparison.Ordinal);
  }

  public static bool IsMatch(string relativePath, string pattern)
  {
    ArgumentNullException.ThrowIfNull(relativePath);
    ArgumentNullException.ThrowIfNull(pattern);

    bool ignoreCase = OperatingSystem.IsWindows();
    string[] pathSegments = SplitSegments(relativePath);
    string[] patternSegments = SplitSegments(pattern);
    return MatchSegments(pathSegments, 0, patternSegments, 0, ignoreCase);
  }

  private static int IndexOfGlobChar(string path)
  {
    int star = path.IndexOf('*', StringComparison.Ordinal);
    int question = path.IndexOf('?', StringComparison.Ordinal);
    if (star < 0)
    {
      return question;
    }

    if (question < 0)
    {
      return star;
    }

    return Math.Min(star, question);
  }

  private static string[] SplitSegments(string path)
  {
    return path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
  }

  private static bool MatchSegments(
    string[] path,
    int pathIndex,
    string[] pattern,
    int patternIndex,
    bool ignoreCase)
  {
    while (patternIndex < pattern.Length)
    {
      if (pattern[patternIndex] == "**")
      {
        if (patternIndex == pattern.Length - 1)
        {
          return true;
        }

        for (int i = pathIndex; i <= path.Length; i++)
        {
          if (MatchSegments(path, i, pattern, patternIndex + 1, ignoreCase))
          {
            return true;
          }
        }

        return false;
      }

      if (pathIndex >= path.Length)
      {
        return false;
      }

      if (!MatchSegment(path[pathIndex], pattern[patternIndex], ignoreCase))
      {
        return false;
      }

      pathIndex++;
      patternIndex++;
    }

    return pathIndex == path.Length;
  }

  private static bool MatchSegment(string text, string pattern, bool ignoreCase)
  {
    int textIndex = 0;
    int patternIndex = 0;
    int starText = -1;
    int starPattern = -1;

    while (textIndex < text.Length)
    {
      if (patternIndex < pattern.Length
        && (pattern[patternIndex] == '?'
          || Equal(text[textIndex], pattern[patternIndex], ignoreCase)))
      {
        textIndex++;
        patternIndex++;
      }
      else if (patternIndex < pattern.Length && pattern[patternIndex] == '*')
      {
        starPattern = patternIndex;
        patternIndex++;
        starText = textIndex;
      }
      else if (starPattern >= 0)
      {
        patternIndex = starPattern + 1;
        starText++;
        textIndex = starText;
      }
      else
      {
        return false;
      }
    }

    while (patternIndex < pattern.Length && pattern[patternIndex] == '*')
    {
      patternIndex++;
    }

    return patternIndex == pattern.Length;
  }

  private static bool Equal(char left, char right, bool ignoreCase)
  {
    if (left == right)
    {
      return true;
    }

    if (!ignoreCase)
    {
      return false;
    }

    return char.ToUpperInvariant(left) == char.ToUpperInvariant(right);
  }
}
