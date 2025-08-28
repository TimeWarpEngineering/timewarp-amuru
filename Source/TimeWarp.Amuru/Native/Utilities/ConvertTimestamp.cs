namespace TimeWarp.Amuru.Native.Utilities;

/// <summary>
/// Utility for converting Unix timestamps to human-readable dates and vice versa.
/// Supports various output formats (ISO8601, RFC3339, custom) and timezone handling.
/// </summary>
public static class ConvertTimestamp
{
  /// <summary>
  /// Converts a Unix timestamp (seconds since epoch) to a human-readable date string.
  /// </summary>
  /// <param name="timestamp">The Unix timestamp in seconds</param>
  /// <returns>A formatted date string in ISO8601 format</returns>
  public static string FromUnix(long timestamp)
  {
    return FromUnix(timestamp, "yyyy-MM-ddTHH:mm:ssK", TimeZoneInfo.Utc);
  }

  /// <summary>
  /// Converts a Unix timestamp (seconds since epoch) to a human-readable date string with custom format.
  /// </summary>
  /// <param name="timestamp">The Unix timestamp in seconds</param>
  /// <param name="format">The date format string (default: ISO8601)</param>
  /// <returns>A formatted date string</returns>
  public static string FromUnix(long timestamp, string format)
  {
    return FromUnix(timestamp, format, TimeZoneInfo.Utc);
  }

  /// <summary>
  /// Converts a Unix timestamp (seconds since epoch) to a human-readable date string with custom format and timezone.
  /// </summary>
  /// <param name="timestamp">The Unix timestamp in seconds</param>
  /// <param name="format">The date format string (default: ISO8601)</param>
  /// <param name="timeZone">The target timezone (default: UTC)</param>
  /// <returns>A formatted date string</returns>
  public static string FromUnix(long timestamp, string format, TimeZoneInfo timeZone)
  {
    var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
    DateTimeOffset convertedTime = TimeZoneInfo.ConvertTime(dateTimeOffset, timeZone);
    return convertedTime.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
  }

  /// <summary>
  /// Converts a DateTime to a Unix timestamp (seconds since epoch).
  /// </summary>
  /// <param name="date">The DateTime to convert</param>
  /// <returns>The Unix timestamp in seconds</returns>
  public static long ToUnix(DateTime date)
  {
    return ToUnix(date, TimeZoneInfo.Utc);
  }

  /// <summary>
  /// Converts a DateTime to a Unix timestamp (seconds since epoch) with timezone consideration.
  /// </summary>
  /// <param name="date">The DateTime to convert</param>
  /// <param name="timeZone">The timezone of the input date (default: UTC)</param>
  /// <returns>The Unix timestamp in seconds</returns>
  public static long ToUnix(DateTime date, TimeZoneInfo timeZone)
  {
    var dateTimeOffset = new DateTimeOffset(date, timeZone.GetUtcOffset(date));
    return dateTimeOffset.ToUnixTimeSeconds();
  }

  /// <summary>
  /// Converts a Git commit timestamp (Unix timestamp) to a human-readable date string.
  /// This is a convenience method specifically for Git commit timestamps.
  /// </summary>
  /// <param name="timestamp">The Git commit timestamp (Unix timestamp in seconds)</param>
  /// <returns>A formatted date string in ISO8601 format</returns>
  public static string FromGitCommitTimestamp(long timestamp)
  {
    return FromUnix(timestamp);
  }

  /// <summary>
  /// Converts a Git commit timestamp (Unix timestamp) to a human-readable date string with custom format.
  /// </summary>
  /// <param name="timestamp">The Git commit timestamp (Unix timestamp in seconds)</param>
  /// <param name="format">The date format string</param>
  /// <returns>A formatted date string</returns>
  public static string FromGitCommitTimestamp(long timestamp, string format)
  {
    return FromUnix(timestamp, format);
  }

  /// <summary>
  /// Creates a builder for configuring timestamp conversion options.
  /// </summary>
  /// <param name="timestamp">The Unix timestamp to convert</param>
  /// <returns>A builder for configuring conversion options</returns>
  internal static TimestampConverter FromUnixTime(long timestamp)
  {
    return new TimestampConverter(timestamp);
  }

  /// <summary>
  /// Builder class for configuring timestamp conversion options.
  /// </summary>
  internal sealed class TimestampConverter
  {
    private readonly long Timestamp;
    private string Format = "yyyy-MM-ddTHH:mm:ssK";
    private TimeZoneInfo TimeZone = TimeZoneInfo.Utc;

    internal TimestampConverter(long timestamp)
    {
      Timestamp = timestamp;
    }

    /// <summary>
    /// Sets the output format for the timestamp conversion.
    /// </summary>
    /// <param name="format">The date format string</param>
    /// <returns>The builder instance for method chaining</returns>
    public TimestampConverter WithFormat(string format)
    {
      Format = format ?? throw new ArgumentNullException(nameof(format));
      return this;
    }

    /// <summary>
    /// Sets the target timezone for the timestamp conversion.
    /// </summary>
    /// <param name="timeZone">The target timezone</param>
    /// <returns>The builder instance for method chaining</returns>
    public TimestampConverter WithTimeZone(TimeZoneInfo timeZone)
    {
      TimeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
      return this;
    }

    /// <summary>
    /// Sets the target timezone by ID for the timestamp conversion.
    /// </summary>
    /// <param name="timeZoneId">The target timezone ID</param>
    /// <returns>The builder instance for method chaining</returns>
    public TimestampConverter WithTimeZone(string timeZoneId)
    {
      TimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
      return this;
    }

    /// <summary>
    /// Converts the timestamp using the configured options.
    /// </summary>
    /// <returns>The formatted date string</returns>
    public override string ToString()
    {
      return FromUnix(Timestamp, Format, TimeZone);
    }

    /// <summary>
    /// Converts to ISO8601 format (default).
    /// </summary>
    /// <returns>The ISO8601 formatted date string</returns>
    public string ToIso8601()
    {
      return WithFormat("yyyy-MM-ddTHH:mm:ssK").ToString();
    }

    /// <summary>
    /// Converts to RFC3339 format.
    /// </summary>
    /// <returns>The RFC3339 formatted date string</returns>
    public string ToRfc3339()
    {
      return WithFormat("yyyy-MM-ddTHH:mm:ssK").ToString();
    }

    /// <summary>
    /// Converts to a human-readable format.
    /// </summary>
    /// <returns>The human-readable date string</returns>
    public string ToHumanReadable()
    {
      return WithFormat("yyyy-MM-dd HH:mm:ss").ToString();
    }
  }
}