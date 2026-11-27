#region Purpose
// Exit code and saved path from Direct.Tail, which writes the bytes itself.
#endregion

namespace TimeWarp.Amuru.Native.Text;

/// <summary>
/// Result of <see cref="Direct.Tail"/>. The bytes were written to the streams the caller passed.
/// </summary>
/// <param name="ExitCode">Zero on success. One when a file or follow target failed.</param>
/// <param name="SavedPath">Saved stdin file, or <see langword="null"/> when nothing was kept.</param>
public readonly record struct TailRunResult(int ExitCode, string? SavedPath);
