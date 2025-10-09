#!/usr/bin/dotnet --

#:property PublishAot=false
using System.Diagnostics;
using static System.Console;

// Helper script for SSH key operations
string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
string sshDir = Path.Combine(homeDir, ".ssh");
string ed25519PrivateKey = Path.Combine(sshDir, "id_ed25519");
string ed25519PublicKey = Path.Combine(sshDir, "id_ed25519.pub");
string rsaPrivateKey = Path.Combine(sshDir, "id_rsa_for_encryption");
string rsaPublicKey = Path.Combine(sshDir, "id_rsa_for_encryption.pub");
string rsaPrivateKeyPem = Path.Combine(sshDir, "id_rsa_for_encryption.pem");

// Check if we need to create RSA key
if (!File.Exists(rsaPrivateKey))
{
  WriteLine("🔑 Generating RSA key pair for encryption...");

  // Generate new RSA key pair specifically for encryption
  var genProcess = Process.Start(new ProcessStartInfo
  {
    FileName = "ssh-keygen",
    Arguments = $"-t rsa -b 4096 -f {rsaPrivateKey} -N \"\" -C \"encryption-key\"",
    UseShellExecute = false,
    RedirectStandardOutput = true,
    RedirectStandardError = true
  });

  genProcess?.WaitForExit();

  if (genProcess?.ExitCode == 0)
  {
    WriteLine($"✅ RSA key pair created:");
    WriteLine($"   Private: {rsaPrivateKey}");
    WriteLine($"   Public: {rsaPublicKey}");

    // Convert private key to PEM format
    var pemProcess = Process.Start(new ProcessStartInfo
    {
      FileName = "bash",
      Arguments = $"-c \"ssh-keygen -p -m PEM -N '' -f {rsaPrivateKey} < /dev/null && cp {rsaPrivateKey} {rsaPrivateKeyPem}\"",
      UseShellExecute = false,
      RedirectStandardOutput = true,
      RedirectStandardError = true
    });

    pemProcess?.WaitForExit();
    if (pemProcess?.ExitCode == 0)
    {
      WriteLine($"✅ PEM key created: {rsaPrivateKeyPem}");
    }
  }
  else
  {
    WriteLine("❌ Failed to generate RSA key pair");
    return 1;
  }
}
else
{
  WriteLine($"✅ RSA encryption key already exists: {rsaPrivateKey}");

  // Check if PEM version exists
  if (!File.Exists(rsaPrivateKeyPem))
  {
    WriteLine("🔑 Converting to PEM format...");
    var pemProcess = Process.Start(new ProcessStartInfo
    {
      FileName = "bash",
      Arguments = $"-c \"ssh-keygen -p -m PEM -N '' -f {rsaPrivateKey} < /dev/null && cp {rsaPrivateKey} {rsaPrivateKeyPem}\"",
      UseShellExecute = false,
      RedirectStandardOutput = true,
      RedirectStandardError = true
    });

    pemProcess?.WaitForExit();
    if (pemProcess?.ExitCode == 0)
    {
      WriteLine($"✅ PEM key created: {rsaPrivateKeyPem}");
    }
  }
}

// Test encryption/decryption
WriteLine("\n🧪 Testing encryption/decryption...");

string testMessage = "Hello, World!";
string? encrypted = null;
string? decrypted = null;

// Test encrypt
var encProcess = Process.Start(new ProcessStartInfo
{
  FileName = "bash",
  Arguments = $"-c \"echo '{testMessage}' | openssl pkeyutl -encrypt -pubin -inkey <(ssh-keygen -f {rsaPublicKey} -e -m PKCS8) | base64 -w 0\"",
  UseShellExecute = false,
  RedirectStandardOutput = true,
  RedirectStandardError = true
});

if (encProcess != null)
{
  encrypted = encProcess.StandardOutput.ReadToEnd().Trim();
  string error = encProcess.StandardError.ReadToEnd();
  encProcess.WaitForExit();

  if (encProcess.ExitCode == 0 && !string.IsNullOrEmpty(encrypted))
  {
    WriteLine("✅ Encryption successful");
  }
  else
  {
    WriteLine($"❌ Encryption failed: {error}");
    return 1;
  }
}

// Test decrypt
if (!string.IsNullOrEmpty(encrypted))
{
  var decProcess = Process.Start(new ProcessStartInfo
  {
    FileName = "bash",
    Arguments = $"-c \"echo '{encrypted}' | base64 -d | openssl pkeyutl -decrypt -inkey {rsaPrivateKeyPem}\"",
    UseShellExecute = false,
    RedirectStandardOutput = true,
    RedirectStandardError = true
  });

  if (decProcess != null)
  {
    decrypted = decProcess.StandardOutput.ReadToEnd().Trim();
    string error = decProcess.StandardError.ReadToEnd();
    decProcess.WaitForExit();

    if (decProcess.ExitCode == 0 && decrypted == testMessage)
    {
      WriteLine("✅ Decryption successful");
      WriteLine($"   Original: {testMessage}");
      WriteLine($"   Decrypted: {decrypted}");
    }
    else
    {
      WriteLine($"❌ Decryption failed: {error}");
      return 1;
    }
  }
}

WriteLine("\n✅ SSH key setup complete!");
WriteLine($"   Use this key for encryption: {rsaPublicKey}");
WriteLine($"   Use this key for decryption: {rsaPrivateKeyPem}");

return 0;