#region Purpose
// Marker partial for the DotNet fluent API. Subcommand builders live in the other partial files.
#endregion

#region Design
// Each builder exposes WithNoValidation and WithZeroExitCodeValidation on its own type.
// This partial holds no command options.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for .NET CLI commands, providing strongly-typed access to common dotnet operations.
/// </summary>
public static partial class DotNet
{
  // This partial class will contain the main DotNet class definition
  // Individual subcommands will be in separate partial class files
}