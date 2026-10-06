#region Purpose
// Commands API for GetItemProperty: stat-style metadata on stdout.
#endregion

#region Design
// Delegates to Direct.GetItemProperty and formats a stable key: value listing so
// scripts can grep Size / Type / LinkTarget without parsing ls columns.
#endregion

namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Commands
{
  /// <summary>
  /// Returns file-system metadata as CommandOutput.
  /// </summary>
  /// <param name="path">Path to inspect.</param>
  /// <returns>CommandOutput with metadata in stdout, or error in stderr.</returns>
  public static CommandOutput GetItemProperty(string path)
  {
    return Invoke("GetItemProperty", path, () =>
    {
      ItemProperty itemProperty = Direct.GetItemProperty(path);
      StringBuilder stringBuilder = new();
      stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Path: {itemProperty.FullName}");
      stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Name: {itemProperty.Name}");
      stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Type: {(itemProperty.IsDirectory ? "Directory" : "File")}");
      stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Size: {itemProperty.Length}");
      stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Attributes: {itemProperty.Attributes}");
      stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Created: {itemProperty.CreationTime:o}");
      stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Modified: {itemProperty.LastWriteTime:o}");
      stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Accessed: {itemProperty.LastAccessTime:o}");
      if (itemProperty.LinkTarget is not null)
      {
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"LinkTarget: {itemProperty.LinkTarget}");
      }

      return stringBuilder.ToString().TrimEnd('\n', '\r');
    });
  }

  /// <summary>
  /// Bash-style alias for GetItemProperty.
  /// </summary>
  public static CommandOutput Stat(string path) => GetItemProperty(path);
}
