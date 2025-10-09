#!/usr/bin/dotnet --

#:property PublishAot=false
#:project ../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:package TimeWarp.Nuru

using TimeWarp.Nuru;
using TimeWarp.Amuru;
using static System.Console;

static async Task<int> SetupSshKeysAsync()
{
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
    CommandOutput result = await Shell.Builder("ssh-keygen")
      .WithArguments("-t", "rsa", "-b", "4096", "-f", rsaPrivateKey, "-N", "", "-C", "encryption-key")
      .CaptureAsync();

    if (result.Success)
    {
      WriteLine("✅ RSA key pair created:");
      WriteLine($"   Private: {rsaPrivateKey}");
      WriteLine($"   Public: {rsaPublicKey}");

      // Convert private key to PEM format
      CommandOutput pemResult = await Shell.Builder("bash")
        .WithArguments("-c", $"ssh-keygen -p -m PEM -N '' -f {rsaPrivateKey} < /dev/null && cp {rsaPrivateKey} {rsaPrivateKeyPem}")
        .CaptureAsync();

      if (pemResult.Success)
      {
        WriteLine($"✅ PEM key created: {rsaPrivateKeyPem}");
      }
      else
      {
        WriteLine($"❌ Failed to convert to PEM format: {pemResult.Stderr}");
        return 1;
      }
    }
    else
    {
      WriteLine($"❌ Failed to generate RSA key pair: {result.Stderr}");
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
      CommandOutput pemResult = await Shell.Builder("bash")
        .WithArguments("-c", $"ssh-keygen -p -m PEM -N '' -f {rsaPrivateKey} < /dev/null && cp {rsaPrivateKey} {rsaPrivateKeyPem}")
        .CaptureAsync();

      if (pemResult.Success)
      {
        WriteLine($"✅ PEM key created: {rsaPrivateKeyPem}");
      }
      else
      {
        WriteLine($"❌ Failed to convert to PEM format: {pemResult.Stderr}");
        return 1;
      }
    }
  }

  WriteLine("\n✅ SSH key setup complete!");
  WriteLine($"   Use this key for encryption: {rsaPublicKey}");
  WriteLine($"   Use this key for decryption: {rsaPrivateKeyPem}");
  WriteLine($"\nRun '{System.Diagnostics.Process.GetCurrentProcess().ProcessName} test' to verify encryption/decryption works.");

  return 0;
}

static async Task<int> TestEncryptionAsync()
{
  string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
  string sshDir = Path.Combine(homeDir, ".ssh");
  string rsaPublicKey = Path.Combine(sshDir, "id_rsa_for_encryption.pub");
  string rsaPrivateKeyPem = Path.Combine(sshDir, "id_rsa_for_encryption.pem");

  if (!File.Exists(rsaPublicKey) || !File.Exists(rsaPrivateKeyPem))
  {
    WriteLine("❌ Keys not found. Run setup first.");
    return 1;
  }

  WriteLine("🧪 Testing encryption/decryption...");

  const string testMessage = "Hello, World!";

  // Test encrypt
  CommandOutput encResult = await Shell.Builder("bash")
    .WithArguments("-c", $"echo '{testMessage}' | openssl pkeyutl -encrypt -pubin -inkey <(ssh-keygen -f {rsaPublicKey} -e -m PKCS8) | base64 -w 0")
    .CaptureAsync();

  if (!encResult.Success || string.IsNullOrEmpty(encResult.Stdout))
  {
    WriteLine($"❌ Encryption failed: {encResult.Stderr}");
    return 1;
  }

  string encrypted = encResult.Stdout.Trim();
  WriteLine("✅ Encryption successful");

  // Test decrypt
  CommandOutput decResult = await Shell.Builder("bash")
    .WithArguments("-c", $"echo '{encrypted}' | base64 -d | openssl pkeyutl -decrypt -inkey {rsaPrivateKeyPem}")
    .CaptureAsync();

  if (!decResult.Success)
  {
    WriteLine($"❌ Decryption failed: {decResult.Stderr}");
    return 1;
  }

  string decrypted = decResult.Stdout.Trim();
  if (decrypted == testMessage)
  {
    WriteLine("✅ Decryption successful");
    WriteLine($"   Original: {testMessage}");
    WriteLine($"   Decrypted: {decrypted}");
  }
  else
  {
    WriteLine($"❌ Decryption mismatch: expected '{testMessage}', got '{decrypted}'");
    return 1;
  }

  return 0;
}

var builder = new NuruAppBuilder();

builder.AddDefaultRoute(SetupSshKeysAsync, "Setup SSH keys for encryption/decryption");
builder.AddRoute("test", TestEncryptionAsync, "Test encryption/decryption with existing keys");

NuruApp app = builder.Build();
return await app.RunAsync(args);
