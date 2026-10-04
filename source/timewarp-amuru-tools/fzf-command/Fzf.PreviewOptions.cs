#region Purpose
// Preview-window options for the fzf builder.
#endregion

#region Design
// --preview-label-pos accepts the same N[:top|bottom] text as --border-label-pos.
// The int overload is a column shorthand; the string overload carries the anchor.
#endregion

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
  /// <param name="position">Column, or fzf's <c>N[:top|bottom]</c> form</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithPreviewLabelPos(string position)
  {
    Arguments.Add($"--preview-label-pos={position}");
    return this;
  }

  /// <summary>
  /// Specifies the preview window label position as a column number.
  /// </summary>
  /// <param name="position">Column number</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithPreviewLabelPos(int position)
  {
    return WithPreviewLabelPos(position.ToString(CultureInfo.InvariantCulture));
  }
}