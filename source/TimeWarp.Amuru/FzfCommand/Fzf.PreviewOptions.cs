namespace TimeWarp.Amuru;

public partial class FzfBuilder
{
  // Preview Options
  /// <summary>
  /// Specifies the preview command.
  /// </summary>
  /// <param name="command">Command to preview highlighted line</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithPreview(string command)
  {
    Arguments.Add($"--preview={command}");
    return this;
  }

  /// <summary>
  /// Specifies the preview window options.
  /// </summary>
  /// <param name="options">Preview window layout options</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithPreviewWindow(string options)
  {
    Arguments.Add($"--preview-window={options}");
    return this;
  }

  /// <summary>
  /// Specifies the preview window label.
  /// </summary>
  /// <param name="label">Preview window label</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithPreviewLabel(string label)
  {
    Arguments.Add($"--preview-label={label}");
    return this;
  }

  /// <summary>
  /// Specifies the preview window label position.
  /// </summary>
  /// <param name="position">Label position</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithPreviewLabelPos(int position)
  {
    Arguments.Add($"--preview-label-pos={position}");
    return this;
  }
}