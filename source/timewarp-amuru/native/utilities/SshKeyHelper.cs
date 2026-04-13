#region Purpose
// Utility for SSH key management operations using ssh-keygen CLI
#endregion

namespace TimeWarp.Amuru.Native.Utilities;

/// <summary>
/// Utility for SSH key management operations.
/// Supports key generation, validation, encryption/decryption, and format conversion.
/// </summary>
public static class SshKeyHelper
{
  private static readonly string SshDirectory = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh");

  /// <summary>
  /// Generates a new SSH key pair.
  /// </summary>
  /// <param name="keyType">The type of key to generate (rsa or ed25519)</param>
  /// <param name="keySize">The key size (for RSA keys)</param>
  /// <param name="comment">Optional comment for the key</param>
  /// <param name="outputPath">Optional output path (defaults to ~/.ssh)</param>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>True if the key was generated successfully, false otherwise</returns>
  public static async Task<bool> GenerateKeyPairAsync(
    string keyType = "ed25519",
    int keySize = 4096,
    string? comment = null,
    string? outputPath = null,
    CancellationToken cancellationToken = default)
  {
    string outputDir = outputPath ?? SshDirectory;

    // Ensure output directory exists
    Directory.CreateDirectory(outputDir);

    string baseName = keyType == "ed25519" ? "id_ed25519" : "id_rsa";
    string privateKeyPath = Path.Combine(outputDir, baseName);
    string publicKeyPath = privateKeyPath + ".pub";

    // Check if key already exists
    if (File.Exists(privateKeyPath))
    {
      await TimeWarpTerminal.Default.WriteLineAsync($"SSH key already exists: {privateKeyPath}");
      return true;
    }

    string keyTypeArgs = keyType == "rsa" ? $"-t rsa -b {keySize}" : "-t ed25519";
    string commentArgs = string.IsNullOrEmpty(comment) ? "" : $"-C \"{comment}\"";

    CommandOutput result = await Shell.Builder("ssh-keygen")
      .WithArguments($"{keyTypeArgs} -f {privateKeyPath} -N \"\" {commentArgs}".Trim())
      .CaptureAsync(cancellationToken);

    if (result.ExitCode == 0)
    {
      await TimeWarpTerminal.Default.WriteLineAsync($"✅ SSH key pair generated successfully:");
      await TimeWarpTerminal.Default.WriteLineAsync($"   Private key: {privateKeyPath}");
      await TimeWarpTerminal.Default.WriteLineAsync($"   Public key: {publicKeyPath}");
      return true;
    }

    await TimeWarpTerminal.Default.WriteLineAsync("❌ Failed to generate SSH key pair");
    return false;
  }

  /// <summary>
  /// Gets the public key content from a private key file.
  /// </summary>
  /// <param name="privateKeyPath">Path to the private key file</param>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The public key content, or null if extraction failed</returns>
  public static async Task<string?> GetPublicKeyAsync(string privateKeyPath, CancellationToken cancellationToken = default)
  {
    if (!File.Exists(privateKeyPath))
    {
      await TimeWarpTerminal.Default.WriteLineAsync($"❌ Private key file not found: {privateKeyPath}");
      return null;
    }

    CommandOutput result = await Shell.Builder("ssh-keygen")
      .WithArguments("-y", "-f", privateKeyPath)
      .CaptureAsync(cancellationToken);

    if (result.ExitCode == 0)
    {
      return result.Stdout.Trim();
    }

    await TimeWarpTerminal.Default.WriteLineAsync($"❌ Failed to extract public key: {result.Stderr}");
    return null;
  }

  /// <summary>
  /// Validates an SSH key file.
  /// </summary>
  /// <param name="keyPath">Path to the SSH key file</param>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>True if the key is valid, false otherwise</returns>
  public static async Task<bool> ValidateKeyAsync(string keyPath, CancellationToken cancellationToken = default)
  {
    if (!File.Exists(keyPath))
    {
      await TimeWarpTerminal.Default.WriteLineAsync($"❌ Key file not found: {keyPath}");
      return false;
    }

    CommandOutput result = await Shell.Builder("ssh-keygen")
      .WithArguments("-l", "-f", keyPath)
      .CaptureAsync(cancellationToken);

    if (result.ExitCode == 0)
    {
      await TimeWarpTerminal.Default.WriteLineAsync($"✅ SSH key is valid: {keyPath}");
      return true;
    }

    await TimeWarpTerminal.Default.WriteLineAsync($"❌ SSH key is invalid: {keyPath}");
    return false;
  }

  /// <summary>
  /// Changes the passphrase of an SSH private key.
  /// </summary>
  /// <param name="privateKeyPath">Path to the private key file</param>
  /// <param name="oldPassphrase">The current passphrase (optional)</param>
  /// <param name="newPassphrase">The new passphrase (optional)</param>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>True if the passphrase was changed successfully, false otherwise</returns>
  public static async Task<bool> ChangePassphraseAsync(
    string privateKeyPath,
    string? oldPassphrase = null,
    string? newPassphrase = null,
    CancellationToken cancellationToken = default)
  {
    if (!File.Exists(privateKeyPath))
    {
      await TimeWarpTerminal.Default.WriteLineAsync($"❌ Private key file not found: {privateKeyPath}");
      return false;
    }

    // Build stdin input for passphrase change
    string stdinInput = "";
    if (!string.IsNullOrEmpty(oldPassphrase))
    {
      stdinInput += oldPassphrase + "\n";
    }

    if (!string.IsNullOrEmpty(newPassphrase))
    {
      stdinInput += newPassphrase + "\n" + newPassphrase + "\n"; // Confirm new passphrase
    }

    ShellBuilder builder = Shell.Builder("ssh-keygen")
      .WithArguments("-p", "-f", privateKeyPath);

    if (!string.IsNullOrEmpty(stdinInput))
    {
      builder = builder.WithStandardInput(stdinInput);
    }

    CommandOutput result = await builder.CaptureAsync(cancellationToken);

    if (result.ExitCode == 0)
    {
      await TimeWarpTerminal.Default.WriteLineAsync($"✅ Passphrase changed successfully for: {privateKeyPath}");
      return true;
    }

    await TimeWarpTerminal.Default.WriteLineAsync($"❌ Failed to change passphrase for: {privateKeyPath}");
    return false;
  }

  /// <summary>
  /// Converts an SSH key to a different format.
  /// </summary>
  /// <param name="inputPath">Path to the input key file</param>
  /// <param name="outputPath">Path to the output key file</param>
  /// <param name="targetFormat">Target format (PEM, PKCS8, etc.)</param>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>True if the conversion was successful, false otherwise</returns>
  public static async Task<bool> ConvertKeyFormatAsync(
    string inputPath,
    string outputPath,
    string targetFormat = "PEM",
    CancellationToken cancellationToken = default)
  {
    if (!File.Exists(inputPath))
    {
      await TimeWarpTerminal.Default.WriteLineAsync($"❌ Input key file not found: {inputPath}");
      return false;
    }

    CommandOutput result = await Shell.Builder("ssh-keygen")
      .WithArguments("-p", "-m", targetFormat, "-f", inputPath, "-N", "")
      .CaptureAsync(cancellationToken);

    if (result.ExitCode == 0)
    {
      // Copy the converted key to output path
      File.Copy(inputPath, outputPath, true);
      await TimeWarpTerminal.Default.WriteLineAsync($"✅ Key converted to {targetFormat} format: {outputPath}");
      return true;
    }

    await TimeWarpTerminal.Default.WriteLineAsync("❌ Failed to convert key format");
    return false;
  }

  /// <summary>
  /// Lists all SSH keys in the default SSH directory.
  /// </summary>
  /// <returns>A list of SSH key file paths</returns>
  public static IReadOnlyList<string> ListKeys()
  {
    if (!Directory.Exists(SshDirectory))
    {
      return Array.Empty<string>();
    }

#pragma warning disable IDE0007 // Use explicit type per C# coding standards
    List<string> keyFiles = Directory.GetFiles(SshDirectory, "id_*")
      .Where(file => !file.EndsWith(".pub", StringComparison.OrdinalIgnoreCase))
      .ToList();
#pragma warning restore IDE0007

    return keyFiles.AsReadOnly();
  }

  /// <summary>
  /// Gets information about an SSH key.
  /// </summary>
  /// <param name="keyPath">Path to the SSH key file</param>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>Information about the key, or null if the key is invalid</returns>
  internal static async Task<SshKeyInfo?> GetKeyInfoAsync(string keyPath, CancellationToken cancellationToken = default)
  {
    if (!File.Exists(keyPath))
    {
      return null;
    }

    CommandOutput result = await Shell.Builder("ssh-keygen")
      .WithArguments("-l", "-f", keyPath)
      .CaptureAsync(cancellationToken);

    if (result.ExitCode == 0 && !string.IsNullOrWhiteSpace(result.Stdout))
    {
      // Parse the output
      string[] parts = result.Stdout.Trim().Split(' ', 3);
      if (parts.Length >= 2)
      {
        return new SshKeyInfo
        {
          Path = keyPath,
          BitLength = int.TryParse(parts[0], out int bits) ? bits : 0,
          Fingerprint = parts.Length >= 2 ? parts[1] : string.Empty,
          Comment = parts.Length >= 3 ? parts[2] : string.Empty,
          IsPublicKey = keyPath.EndsWith(".pub", StringComparison.OrdinalIgnoreCase)
        };
      }
    }

    return null;
  }

  /// <summary>
  /// Information about an SSH key.
  /// </summary>
  internal sealed class SshKeyInfo
  {
    /// <summary>
    /// Path to the key file
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Bit length of the key
    /// </summary>
    public int BitLength { get; set; }

    /// <summary>
    /// Fingerprint of the key
    /// </summary>
    public string Fingerprint { get; set; } = string.Empty;

    /// <summary>
    /// Comment associated with the key
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a public key file
    /// </summary>
    public bool IsPublicKey { get; set; }
  }
}
