#region Purpose
// Per-file outcome of ReplaceInFiles: count, whether bytes changed, dry-run preview, backup path.
#endregion

#region Design
// Changed is false when the replacement leaves the file text identical, so callers can
// skip work. Preview is empty unless DryRun is set and the text would change.
// BackupPath is null when no .bak file was written.
#endregion

namespace TimeWarp.Amuru.Native.Text;

/// <summary>
/// The outcome of applying a replacement to one file.
/// </summary>
public sealed class ReplaceResult
{
  /// <summary>
  /// Gets the fully qualified file path.
  /// </summary>
  public required string Path { get; init; }

  /// <summary>
  /// Gets how many substitutions were applied in this file.
  /// </summary>
  public required int ReplacementCount { get; init; }

  /// <summary>
  /// Gets a value indicating whether the file text differs after substitution.
  /// A dry run sets this when the file would change and does not write it.
  /// </summary>
  public required bool Changed { get; init; }

  /// <summary>
  /// Gets unified-diff lines when this result is a dry run and <see cref="Changed"/> is
  /// <see langword="true"/>. Empty otherwise.
  /// </summary>
  public required IReadOnlyList<string> Preview { get; init; }

  /// <summary>
  /// Gets the <c>.bak</c> path written before a successful replacement, or <see langword="null"/>
  /// when no backup was written.
  /// </summary>
  public required string? BackupPath { get; init; }
}
