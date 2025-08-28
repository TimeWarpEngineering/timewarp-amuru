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
  /// <returns>True if the key was generated successfully, false otherwise</returns>
  public static bool GenerateKeyPair(
    string keyType = "ed25519",
    int keySize = 4096,
    string? comment = null,
    string? outputPath = null)
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
      Console.WriteLine($"SSH key already exists: {privateKeyPath}");
      return true;
    }

    try
    {
#pragma warning disable CA1416 // Validate platform compatibility
      using var process = new System.Diagnostics.Process();
      process.StartInfo = new System.Diagnostics.ProcessStartInfo
      {
        FileName = "ssh-keygen",
        Arguments = $"{(keyType == "rsa" ? $"-t rsa -b {keySize}" : "-t ed25519")} -f {privateKeyPath} -N \"\" {(string.IsNullOrEmpty(comment) ? "" : $"-C \"{comment}\"")}",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
      };

      process.Start();
      process.WaitForExit();

      if (process.ExitCode == 0)
      {
        Console.WriteLine($"✅ SSH key pair generated successfully:");
        Console.WriteLine($"   Private key: {privateKeyPath}");
        Console.WriteLine($"   Public key: {publicKeyPath}");
        return true;
      }
      else
      {
        Console.WriteLine("❌ Failed to generate SSH key pair");
        return false;
      }
#pragma warning restore CA1416
    }
    catch (Exception ex)
    {
      Console.WriteLine($"❌ Error generating SSH key pair: {ex.Message}");
      return false;
    }
  }

  /// <summary>
  /// Gets the public key content from a private key file.
  /// </summary>
  /// <param name="privateKeyPath">Path to the private key file</param>
  /// <returns>The public key content, or null if extraction failed</returns>
  public static string? GetPublicKey(string privateKeyPath)
  {
    if (!File.Exists(privateKeyPath))
    {
      Console.WriteLine($"❌ Private key file not found: {privateKeyPath}");
      return null;
    }

    try
    {
#pragma warning disable CA1416 // Validate platform compatibility
      using var process = new System.Diagnostics.Process();
      process.StartInfo = new System.Diagnostics.ProcessStartInfo
      {
        FileName = "ssh-keygen",
        Arguments = $"-y -f {privateKeyPath}",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
      };

      process.Start();
      string output = process.StandardOutput.ReadToEnd();
      string error = process.StandardError.ReadToEnd();
      process.WaitForExit();

      if (process.ExitCode == 0)
      {
        return output.Trim();
      }
      else
      {
        Console.WriteLine($"❌ Failed to extract public key: {error}");
        return null;
      }
#pragma warning restore CA1416
    }
    catch (Exception ex)
    {
      Console.WriteLine($"❌ Error extracting public key: {ex.Message}");
      return null;
    }
  }

  /// <summary>
  /// Validates an SSH key file.
  /// </summary>
  /// <param name="keyPath">Path to the SSH key file</param>
  /// <returns>True if the key is valid, false otherwise</returns>
  public static bool ValidateKey(string keyPath)
  {
    if (!File.Exists(keyPath))
    {
      Console.WriteLine($"❌ Key file not found: {keyPath}");
      return false;
    }

    try
    {
#pragma warning disable CA1416 // Validate platform compatibility
      using var process = new System.Diagnostics.Process();
      process.StartInfo = new System.Diagnostics.ProcessStartInfo
      {
        FileName = "ssh-keygen",
        Arguments = $"-l -f {keyPath}",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
      };

      process.Start();
      process.WaitForExit();

      if (process.ExitCode == 0)
      {
        Console.WriteLine($"✅ SSH key is valid: {keyPath}");
        return true;
      }
      else
      {
        Console.WriteLine($"❌ SSH key is invalid: {keyPath}");
        return false;
      }
#pragma warning restore CA1416
    }
    catch (Exception ex)
    {
      Console.WriteLine($"❌ Error validating SSH key: {ex.Message}");
      return false;
    }
  }

  /// <summary>
  /// Changes the passphrase of an SSH private key.
  /// </summary>
  /// <param name="privateKeyPath">Path to the private key file</param>
  /// <param name="oldPassphrase">The current passphrase (optional)</param>
  /// <param name="newPassphrase">The new passphrase (optional)</param>
  /// <returns>True if the passphrase was changed successfully, false otherwise</returns>
  public static bool ChangePassphrase(string privateKeyPath, string? oldPassphrase = null, string? newPassphrase = null)
  {
    if (!File.Exists(privateKeyPath))
    {
      Console.WriteLine($"❌ Private key file not found: {privateKeyPath}");
      return false;
    }

    try
    {
#pragma warning disable CA1416 // Validate platform compatibility
      using var process = new System.Diagnostics.Process();
      process.StartInfo = new System.Diagnostics.ProcessStartInfo
      {
        FileName = "ssh-keygen",
        Arguments = $"-p -f {privateKeyPath}",
        UseShellExecute = false,
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true
      };

      process.Start();

      // Handle passphrase input if provided
      using (StreamWriter writer = process.StandardInput)
      {
        if (!string.IsNullOrEmpty(oldPassphrase))
        {
          writer.WriteLine(oldPassphrase);
        }

        if (!string.IsNullOrEmpty(newPassphrase))
        {
          writer.WriteLine(newPassphrase);
          writer.WriteLine(newPassphrase); // Confirm
        }
      }

      process.WaitForExit();

      if (process.ExitCode == 0)
      {
        Console.WriteLine($"✅ Passphrase changed successfully for: {privateKeyPath}");
        return true;
      }
      else
      {
        Console.WriteLine($"❌ Failed to change passphrase for: {privateKeyPath}");
        return false;
      }
#pragma warning restore CA1416
    }
    catch (Exception ex)
    {
      Console.WriteLine($"❌ Error changing passphrase: {ex.Message}");
      return false;
    }
  }

  /// <summary>
  /// Converts an SSH key to a different format.
  /// </summary>
  /// <param name="inputPath">Path to the input key file</param>
  /// <param name="outputPath">Path to the output key file</param>
  /// <param name="targetFormat">Target format (PEM, PKCS8, etc.)</param>
  /// <returns>True if the conversion was successful, false otherwise</returns>
  public static bool ConvertKeyFormat(string inputPath, string outputPath, string targetFormat = "PEM")
  {
    if (!File.Exists(inputPath))
    {
      Console.WriteLine($"❌ Input key file not found: {inputPath}");
      return false;
    }

    try
    {
#pragma warning disable CA1416 // Validate platform compatibility
      using var process = new System.Diagnostics.Process();
      process.StartInfo = new System.Diagnostics.ProcessStartInfo
      {
        FileName = "ssh-keygen",
        Arguments = $"-p -m {targetFormat} -f {inputPath} -N \"\"",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
      };

      process.Start();
      process.WaitForExit();

      if (process.ExitCode == 0)
      {
        // Copy the converted key to output path
        File.Copy(inputPath, outputPath, true);
        Console.WriteLine($"✅ Key converted to {targetFormat} format: {outputPath}");
        return true;
      }
      else
      {
        Console.WriteLine($"❌ Failed to convert key format");
        return false;
      }
#pragma warning restore CA1416
    }
    catch (Exception ex)
    {
      Console.WriteLine($"❌ Error converting key format: {ex.Message}");
      return false;
    }
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

    var keyFiles = Directory.GetFiles(SshDirectory, "id_*")
      .Where(file => !file.EndsWith(".pub", StringComparison.OrdinalIgnoreCase))
      .ToList();

    return keyFiles.AsReadOnly();
  }

  /// <summary>
  /// Gets information about an SSH key.
  /// </summary>
  /// <param name="keyPath">Path to the SSH key file</param>
  /// <returns>Information about the key, or null if the key is invalid</returns>
  internal static SshKeyInfo? GetKeyInfo(string keyPath)
  {
    if (!File.Exists(keyPath))
    {
      return null;
    }

    try
    {
#pragma warning disable CA1416 // Validate platform compatibility
      using var process = new System.Diagnostics.Process();
      process.StartInfo = new System.Diagnostics.ProcessStartInfo
      {
        FileName = "ssh-keygen",
        Arguments = $"-l -f {keyPath}",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
      };

      process.Start();
      string output = process.StandardOutput.ReadToEnd();
      process.WaitForExit();

      if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
      {
        // Parse the output
        string[] parts = output.Trim().Split(' ', 3);
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
#pragma warning restore CA1416
    }
    catch
    {
      // Ignore errors
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