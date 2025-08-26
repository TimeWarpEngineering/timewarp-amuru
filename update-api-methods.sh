#!/bin/bash

# Script to update all DotNet command builder files with new API methods

OLD_METHODS='  /// <summary>
  /// Executes the dotnet .* command and returns the output as a string\.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The command output as a string</returns>
  public async Task<string> GetStringAsync\(CancellationToken cancellationToken = default\)
  {
    return await Build\(\)\.GetStringAsync\(cancellationToken\);
  }

  /// <summary>
  /// Executes the dotnet .* command and returns the output as an array of lines\.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The command output as an array of lines</returns>
  public async Task<string\[\]> GetLinesAsync\(CancellationToken cancellationToken = default\)
  {
    return await Build\(\)\.GetLinesAsync\(cancellationToken\);
  }

  /// <summary>
  /// Executes the dotnet .* command .*\.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>.*</returns>
  public async Task<ExecutionResult> ExecuteAsync\(CancellationToken cancellationToken = default\)
  {
    return await Build\(\)\.ExecuteAsync\(cancellationToken\);
  }'

NEW_METHODS='  /// <summary>
  /// Executes the command and streams output to the console in real-time.
  /// This is the default behavior matching shell execution.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The exit code of the command</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken);
  }

  /// <summary>
  /// Executes the command silently and captures all output.
  /// No output is written to the console.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>CommandOutput with stdout, stderr, combined output and exit code</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken);
  }
  
  /// <summary>
  /// Passes the command through to the terminal with full interactive control.
  /// This allows commands like vim, fzf, or REPLs to work with user input and terminal UI.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The execution result (output strings will be empty since output goes to console)</returns>
  public async Task<ExecutionResult> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken);
  }
  
  /// <summary>
  /// Executes an interactive selection command and returns the selected value.
  /// The UI is rendered to the console (via stderr) while stdout is captured and returned.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The selected value from the interactive command</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken);
  }'

# Find all C# files that need updating (exclude already updated files)
FILES=$(find /home/steventcramer/worktrees/github.com/TimeWarpEngineering/timewarp-amuru/Cramer-2025-08-03-ci-cd/Source/TimeWarp.Amuru/DotNetCommands \
  -name "*.cs" \
  -not -name "DotNet.Base.cs" \
  -not -name "DotNet.Build.cs" \
  -not -name "DotNet.Clean.cs" \
  -not -name "DotNet.Pack.cs" \
  -not -name "DotNet.Publish.cs" \
  -not -name "DotNet.RemovePackage.cs" \
  -not -name "DotNet.cs")

echo "Files to update:"
echo "$FILES"

echo
echo "This script would update the API methods but given the complexity of the pattern,"
echo "it's better to do this manually file by file to ensure accuracy."
echo
echo "Manual approach recommended for:"
for file in $FILES; do
  echo "  $file"
done