namespace TimeWarp.Amuru.Native.Utilities;

/// <summary>
/// Utility for posting content to social media platforms.
/// Supports posting markdown content to Nostr and X (Twitter).
/// </summary>
public static class Post
{
  /// <summary>
  /// Posts content to all platforms (Nostr and X/Twitter).
  /// </summary>
  /// <param name="content">The content to post</param>
  /// <returns>True if all posts were successful, false otherwise</returns>
  public static bool ToAll(string content)
  {
    if (string.IsNullOrWhiteSpace(content))
    {
      throw new ArgumentException("Content cannot be null or whitespace", nameof(content));
    }

    bool nostrSuccess = ToNostr(content);
    bool xSuccess = ToX(content);

    return nostrSuccess && xSuccess;
  }

  /// <summary>
  /// Posts content to Nostr.
  /// </summary>
  /// <param name="content">The content to post</param>
  /// <returns>True if the post was successful, false otherwise</returns>
  public static bool ToNostr(string content)
  {
    if (string.IsNullOrWhiteSpace(content))
    {
      throw new ArgumentException("Content cannot be null or whitespace", nameof(content));
    }

    // This is a placeholder implementation
    // In a real implementation, this would use Nostr libraries or HTTP calls
    Console.WriteLine($"📡 Would post to Nostr: {content.Substring(0, Math.Min(50, content.Length))}...");
    return true;
  }

  /// <summary>
  /// Posts content to X (Twitter).
  /// </summary>
  /// <param name="content">The content to post</param>
  /// <returns>True if the post was successful, false otherwise</returns>
  public static bool ToX(string content)
  {
    if (string.IsNullOrWhiteSpace(content))
    {
      throw new ArgumentException("Content cannot be null or whitespace", nameof(content));
    }

    // This is a placeholder implementation
    // In a real implementation, this would use Twitter API v2
    Console.WriteLine($"🐦 Would post to X: {content.Substring(0, Math.Min(50, content.Length))}...");
    return true;
  }

  /// <summary>
  /// Reads content from a markdown file.
  /// </summary>
  /// <param name="filePath">The path to the markdown file</param>
  /// <returns>The content of the file</returns>
  public static string ReadMarkdownFile(string filePath)
  {
    if (string.IsNullOrWhiteSpace(filePath))
    {
      throw new ArgumentException("File path cannot be null or whitespace", nameof(filePath));
    }

    if (!File.Exists(filePath))
    {
      throw new FileNotFoundException($"Markdown file not found: {filePath}", filePath);
    }

    try
    {
      return File.ReadAllText(filePath);
    }
    catch (Exception ex)
    {
      throw new IOException($"Error reading markdown file: {ex.Message}", ex);
    }
  }

  /// <summary>
  /// Posts content from a markdown file to all platforms.
  /// </summary>
  /// <param name="filePath">The path to the markdown file</param>
  /// <returns>True if all posts were successful, false otherwise</returns>
  public static bool FromFile(string filePath)
  {
    string content = ReadMarkdownFile(filePath);
    return ToAll(content);
  }

  /// <summary>
  /// Posts content from a markdown file to Nostr only.
  /// </summary>
  /// <param name="filePath">The path to the markdown file</param>
  /// <returns>True if the post was successful, false otherwise</returns>
  public static bool FromFileToNostr(string filePath)
  {
    string content = ReadMarkdownFile(filePath);
    return ToNostr(content);
  }

  /// <summary>
  /// Posts content from a markdown file to X (Twitter) only.
  /// </summary>
  /// <param name="filePath">The path to the markdown file</param>
  /// <returns>True if the post was successful, false otherwise</returns>
  public static bool FromFileToX(string filePath)
  {
    string content = ReadMarkdownFile(filePath);
    return ToX(content);
  }
}