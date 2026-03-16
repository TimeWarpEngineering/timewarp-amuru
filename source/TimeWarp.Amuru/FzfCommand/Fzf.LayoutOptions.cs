namespace TimeWarp.Amuru;

public partial class FzfBuilder
{
  // Layout Options
  /// <summary>
  /// Specifies the display height.
  /// </summary>
  /// <param name="height">Height in lines or percentage</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithHeight(int height)
  {
    Arguments.Add($"--height={height}");
    return this;
  }

  /// <summary>
  /// Specifies the display height as percentage.
  /// </summary>
  /// <param name="heightPercent">Height as percentage</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithHeightPercent(int heightPercent)
  {
    Arguments.Add($"--height={heightPercent}%");
    return this;
  }

  /// <summary>
  /// Specifies the minimum height when using percentage.
  /// </summary>
  /// <param name="minHeight">Minimum height in lines</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithMinHeight(int minHeight)
  {
    Arguments.Add($"--min-height={minHeight}");
    return this;
  }

  /// <summary>
  /// Specifies the layout style.
  /// </summary>
  /// <param name="layout">Layout style (default, reverse, reverse-list)</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithLayout(string layout)
  {
    Arguments.Add($"--layout={layout}");
    return this;
  }

  /// <summary>
  /// Enables border with default style.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithBorder()
  {
    Arguments.Add("--border");
    return this;
  }

  /// <summary>
  /// Enables border with specified style.
  /// </summary>
  /// <param name="style">Border style (rounded, sharp, bold, etc.)</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithBorder(string style)
  {
    Arguments.Add($"--border={style}");
    return this;
  }

  /// <summary>
  /// Specifies the border label.
  /// </summary>
  /// <param name="label">Label text for the border</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithBorderLabel(string label)
  {
    Arguments.Add($"--border-label={label}");
    return this;
  }

  /// <summary>
  /// Specifies the border label position.
  /// </summary>
  /// <param name="position">Position of the border label</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithBorderLabelPos(string position)
  {
    Arguments.Add($"--border-label-pos={position}");
    return this;
  }

  /// <summary>
  /// Specifies screen margins.
  /// </summary>
  /// <param name="margin">Margin specification (TRBL format)</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithMargin(string margin)
  {
    Arguments.Add($"--margin={margin}");
    return this;
  }

  /// <summary>
  /// Specifies padding inside border.
  /// </summary>
  /// <param name="padding">Padding specification (TRBL format)</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithPadding(string padding)
  {
    Arguments.Add($"--padding={padding}");
    return this;
  }

  /// <summary>
  /// Specifies the finder info style.
  /// </summary>
  /// <param name="style">Info style (default, right, hidden, inline, etc.)</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithInfo(string style)
  {
    Arguments.Add($"--info={style}");
    return this;
  }

  /// <summary>
  /// Specifies the horizontal separator string.
  /// </summary>
  /// <param name="separator">Separator string</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithSeparator(string separator)
  {
    Arguments.Add($"--separator={separator}");
    return this;
  }

  /// <summary>
  /// Hides the info line separator.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithNoSeparator()
  {
    Arguments.Add("--no-separator");
    return this;
  }

  /// <summary>
  /// Enables scrollbar with default characters.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithScrollbar()
  {
    Arguments.Add("--scrollbar");
    return this;
  }

  /// <summary>
  /// Enables scrollbar with custom characters.
  /// </summary>
  /// <param name="chars">Characters for scrollbar</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithScrollbar(string chars)
  {
    Arguments.Add($"--scrollbar={chars}");
    return this;
  }

  /// <summary>
  /// Hides the scrollbar.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithNoScrollbar()
  {
    Arguments.Add("--no-scrollbar");
    return this;
  }

  /// <summary>
  /// Specifies the input prompt.
  /// </summary>
  /// <param name="prompt">Prompt string</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithPrompt(string prompt)
  {
    Arguments.Add($"--prompt={prompt}");
    return this;
  }

  /// <summary>
  /// Specifies the pointer to the current line.
  /// </summary>
  /// <param name="pointerString">Pointer string</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithPointer(string pointerString)
  {
    Arguments.Add($"--pointer={pointerString}");
    return this;
  }

  /// <summary>
  /// Specifies the multi-select marker.
  /// </summary>
  /// <param name="marker">Marker string</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithMarker(string marker)
  {
    Arguments.Add($"--marker={marker}");
    return this;
  }

  /// <summary>
  /// Specifies the header string.
  /// </summary>
  /// <param name="header">Header string</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithHeader(string header)
  {
    Arguments.Add($"--header={header}");
    return this;
  }

  /// <summary>
  /// Specifies the number of header lines from input.
  /// </summary>
  /// <param name="lines">Number of header lines</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithHeaderLines(int lines)
  {
    Arguments.Add($"--header-lines={lines}");
    return this;
  }

  /// <summary>
  /// Prints header before the prompt line.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithHeaderFirst()
  {
    Arguments.Add("--header-first");
    return this;
  }

  /// <summary>
  /// Specifies the ellipsis string for truncated lines.
  /// </summary>
  /// <param name="ellipsis">Ellipsis string</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithEllipsis(string ellipsis)
  {
    Arguments.Add($"--ellipsis={ellipsis}");
    return this;
  }
}