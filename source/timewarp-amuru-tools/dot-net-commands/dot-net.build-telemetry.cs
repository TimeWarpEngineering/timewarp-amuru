#region Purpose
// Tags the ambient Activity with build.command, build.configuration, and build.publish_aot
// when a DotNet build, publish, pack, or test builder runs. Task 144.
#endregion

#region Design
// No ActivitySource of our own: the tags land on Activity.Current, which for a dev CLI is the
// Nuru TelemetryBehavior command span. With no current activity this does nothing.
// Only the command word, the configuration name, and a bool are exported. No argv, paths,
// property values other than PublishAot, or exception text.
// build.publish_aot is true for a publish when, in order:
//   1. a PublishAot MSBuild property was passed (its bool value wins);
//   2. the project is a file-based app (.cs); the SDK defaults PublishAot to true there, and a
//      `#:property PublishAot=false` directive turns it off;
//   3. the project file text sets <PublishAot>true</PublishAot>.
// Directory.Build.props and directory project paths are not inspected. A missing or unreadable
// file reads as "not AOT" for project files and as the SDK default for runfiles.
#endregion

namespace TimeWarp.Amuru;

using System.Diagnostics;

/// <summary>
/// Sets build span attributes on <see cref="Activity.Current"/> for dotnet build-family commands.
/// </summary>
internal static class DotNetBuildTelemetry
{
  internal const string CommandTag = "build.command";
  internal const string ConfigurationTag = "build.configuration";
  internal const string PublishAotTag = "build.publish_aot";

  private const string PublishAotProperty = "PublishAot";
  private const string ConfigurationProperty = "Configuration";
  private const string PropertyDirective = "#:property";

  /// <summary>
  /// Tags the current activity for a build-family command.
  /// </summary>
  /// <param name="command">One of build, publish, pack, or test.</param>
  /// <param name="configuration">The configuration passed with WithConfiguration, if any.</param>
  /// <param name="properties">The MSBuild properties passed to the builder.</param>
  /// <param name="project">The project argument, if any.</param>
  /// <param name="workingDirectory">The working directory the command runs in, if set.</param>
  internal static void Tag
  (
    string command,
    string? configuration,
    IReadOnlyDictionary<string, string> properties,
    string? project,
    string? workingDirectory
  )
  {
    Activity? activity = Activity.Current;
    if (activity is null)
    {
      return;
    }

    activity.SetTag(CommandTag, command);

    string? resolvedConfiguration = ResolveConfiguration(configuration, properties);
    if (resolvedConfiguration is not null)
    {
      activity.SetTag(ConfigurationTag, resolvedConfiguration);
    }

    bool publishAot =
      string.Equals(command, "publish", StringComparison.Ordinal)
      && IsAotPublish(properties, project, workingDirectory);

    activity.SetTag(PublishAotTag, publishAot);
  }

  private static string? ResolveConfiguration(string? configuration, IReadOnlyDictionary<string, string> properties)
  {
    if (!string.IsNullOrWhiteSpace(configuration))
    {
      return configuration;
    }

    string? propertyValue = FindProperty(properties, ConfigurationProperty);
    return string.IsNullOrWhiteSpace(propertyValue) ? null : propertyValue;
  }

  private static bool IsAotPublish(IReadOnlyDictionary<string, string> properties, string? project, string? workingDirectory)
  {
    string? propertyValue = FindProperty(properties, PublishAotProperty);
    if (propertyValue is not null)
    {
      return bool.TryParse(propertyValue.Trim(), out bool explicitValue) && explicitValue;
    }

    if (string.IsNullOrWhiteSpace(project))
    {
      return false;
    }

    string path = ResolvePath(project, workingDirectory);

    if (path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
    {
      return RunfilePublishAot(path);
    }

    if (path.EndsWith("proj", StringComparison.OrdinalIgnoreCase))
    {
      return ProjectFilePublishAot(path);
    }

    return false;
  }

  private static string? FindProperty(IReadOnlyDictionary<string, string> properties, string name)
  {
    foreach (KeyValuePair<string, string> property in properties)
    {
      if (string.Equals(property.Key, name, StringComparison.OrdinalIgnoreCase))
      {
        return property.Value;
      }
    }

    return null;
  }

  private static string ResolvePath(string project, string? workingDirectory)
  {
    if (Path.IsPathRooted(project) || string.IsNullOrWhiteSpace(workingDirectory))
    {
      return project;
    }

    return Path.Combine(workingDirectory, project);
  }

  private static bool RunfilePublishAot(string path)
  {
    // File-based apps publish Native AOT unless a directive turns it off.
    string[] lines = ReadLines(path);
    foreach (string rawLine in lines)
    {
      string line = rawLine.Trim();
      if (!line.StartsWith(PropertyDirective, StringComparison.Ordinal))
      {
        continue;
      }

      string assignment = line[PropertyDirective.Length..].Trim();
      int separator = assignment.IndexOf('=', StringComparison.Ordinal);
      if (separator <= 0)
      {
        continue;
      }

      string name = assignment[..separator].Trim();
      if (!string.Equals(name, PublishAotProperty, StringComparison.OrdinalIgnoreCase))
      {
        continue;
      }

      string value = assignment[(separator + 1)..].Trim();
      return !bool.TryParse(value, out bool directiveValue) || directiveValue;
    }

    return true;
  }

  private static bool ProjectFilePublishAot(string path)
  {
    string[] lines = ReadLines(path);
    foreach (string line in lines)
    {
      string compact = line.Replace(" ", string.Empty, StringComparison.Ordinal);
      if (compact.Contains("<PublishAot>true</PublishAot>", StringComparison.OrdinalIgnoreCase))
      {
        return true;
      }
    }

    return false;
  }

  private static string[] ReadLines(string path)
  {
    try
    {
      return File.Exists(path) ? File.ReadAllLines(path) : [];
    }
    catch (IOException)
    {
      return [];
    }
    catch (UnauthorizedAccessException)
    {
      return [];
    }
  }
}
