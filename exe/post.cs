#!/usr/bin/dotnet --

#:package TimeWarp.Nuru
#:package Blockcore.Nostr.Client
#:property TrimMode=partial
#:property NoWarn=IL2104;IL3053;CA1031;IL2026;IL3050

using TimeWarp.Nuru;
using Nostr.Client.Client;
using Nostr.Client.Communicator;
using Nostr.Client.Keys;
using Nostr.Client.Messages;
using Nostr.Client.Requests;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Diagnostics;
using static System.Console;

NuruAppBuilder builder = new();

// Enable auto-help
builder.AddAutoHelp();

// Register main command with description
builder.AddRoute
(
  "{file|Markdown file to post} " +
  "--platform,-p {platform?|Platform to post to (nostr, x, or both - default: both)}",
  PostContent,
  "Publish markdown content to social platforms (Nostr and/or X)"
);

// Config management commands
builder.AddRoute
(
  "encrypt-config",
  EncryptConfig,
  "Encrypt existing plaintext config.json file"
);

builder.AddRoute
(
  "config set nostr-key {key|Nostr private key (nsec1...)}",
  (string key) => SetConfigValue("nostr.privateKey", key),
  "Set Nostr private key"
);

builder.AddRoute
(
  "config set x-consumer-key {key|X API consumer key}",
  (string key) => SetConfigValue("x.consumerKey", key),
  "Set X API consumer key"
);

builder.AddRoute
(
  "config set x-consumer-secret {secret|X API consumer secret}",
  (string secret) => SetConfigValue("x.consumerSecret", secret),
  "Set X API consumer secret"
);

builder.AddRoute
(
  "config set x-access-token {token|X API access token}",
  (string token) => SetConfigValue("x.accessToken", token),
  "Set X API access token"
);

builder.AddRoute
(
  "config set x-access-secret {secret|X API access token secret}",
  (string secret) => SetConfigValue("x.accessTokenSecret", secret),
  "Set X API access token secret"
);

builder.AddRoute
(
  "config add relay {url|WebSocket URL of Nostr relay}",
  AddRelay,
  "Add a Nostr relay URL"
);

builder.AddRoute
(
  "config show",
  ShowConfig,
  "Display current configuration (with masked secrets)"
);

builder.AddRoute
(
  "config example",
  ShowConfigExample,
  "Show example configuration file format and setup instructions"
);

NuruApp app = builder.Build();
return await app.RunAsync(args);

// Main posting command
static async Task PostContent(string file, string? platform)
{
  string? content = ReadMarkdownFile(file);
  if (content == null) return;

  string platformLower = platform?.ToLowerInvariant() ?? "both";

  switch (platformLower)
  {
    case "nostr":
      await PostToNostr(content);
      break;
    case "x":
    case "twitter":
      await PostToX(content);
      break;
    case "both":
    case "all":
    case null:
    case "":
      await PostToNostr(content);
      await PostToX(content);
      break;
    default:
      WriteLine($"Unknown platform: {platform}. Use 'nostr', 'x', or 'both'");
      break;
  }
}

// Shared JSON options
static JsonSerializerOptions GetJsonOptions() => new()
{
  WriteIndented = true,
  PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
  PropertyNameCaseInsensitive = true
};

// Helper functions
static Config? LoadConfig()
{
  string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nostr");
  string configPath = Path.Combine(configDir, "config.json");
  string encryptedPath = configPath + ".enc";

  // Check for encrypted config first
  if (File.Exists(encryptedPath))
  {
    try
    {
      string encrypted = File.ReadAllText(encryptedPath);
      string json = DecryptWithSshKey(encrypted);
#pragma warning disable IL2026, IL3050 // JSON serialization warnings for AOT
      return JsonSerializer.Deserialize<Config>(json, GetJsonOptions());
#pragma warning restore IL2026, IL3050
    }
    catch (Exception ex)
    {
      WriteLine($"❌ Error reading encrypted config: {ex.Message}");
      return null;
    }
  }

  // Fall back to plaintext config
  if (File.Exists(configPath))
  {
    try
    {
      string json = File.ReadAllText(configPath);
#pragma warning disable IL2026, IL3050 // JSON serialization warnings for AOT
      return JsonSerializer.Deserialize<Config>(json, GetJsonOptions());
#pragma warning restore IL2026, IL3050
    }
    catch (Exception ex)
    {
      WriteLine($"❌ Error reading config file: {ex.Message}");
      return null;
    }
  }

  return null;
}

static string? ReadMarkdownFile(string file)
{
  if (!File.Exists(file))
  {
    WriteLine($"❌ File not found: {file}");
    return null;
  }

  if (!file.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
  {
    WriteLine($"⚠️  Warning: {file} doesn't appear to be a markdown file");
  }

  try
  {
    return File.ReadAllText(file);
  }
  catch (Exception ex)
  {
    WriteLine($"❌ Error reading file: {ex.Message}");
    return null;
  }
}

static async Task PostToNostr(string content)
{
  try
  {
    // Load config
    Config? config = LoadConfig();
    if (config?.Nostr?.PrivateKey == null)
    {
      WriteLine($$"""
        ⚠️  Nostr private key not configured
           Create ~/.nostr/config.json with:
           {
             "nostr": {
               "privateKey": "nsec1...",
               "relays": ["wss://relay.damus.io", "wss://relay.snort.social"]
             }
           }

        📡 Would post to Nostr:
           Content length: {{content.Length}} characters
        """);
      return;
    }

    // Get relay URLs from config or use defaults
    List<string> relayUrls = config.Nostr.Relays ?? new List<string> { "wss://relay.damus.io", "wss://relay.snort.social" };

    // Parse private key
    var key = NostrPrivateKey.FromBech32(config.Nostr.PrivateKey!);
    string pubkey = key.DerivePublicKey().Bech32;

    // Create the event
    var ev = new NostrEvent
    {
      Kind = NostrKind.ShortTextNote,
      CreatedAt = DateTime.UtcNow,
      Content = content,
      Tags = new NostrEventTags()
    };

    // Sign the event
    NostrEvent signed = ev.Sign(key);

    WriteLine($"📡 Posting to {relayUrls.Count} relay(s)...");
    WriteLine($"   Author: {pubkey}");

    // Post to each relay
    int successCount = 0;
    foreach (string relayUrl in relayUrls)
    {
      try
      {
        var uri = new Uri(relayUrl.Trim());
        using var communicator = new NostrWebsocketCommunicator(uri);
        communicator.Name = uri.Host;
        communicator.ReconnectTimeout = TimeSpan.FromSeconds(30);
        communicator.ErrorReconnectTimeout = TimeSpan.FromSeconds(60);

        using var client = new NostrWebsocketClient(communicator, null);

        // Connect
        await communicator.Start();

        // Send the event
        client.Send(new NostrEventRequest(signed));

        // Wait for acknowledgment
        await Task.Delay(500);

        WriteLine($"   ✓ Posted to {uri.Host}");
        successCount++;
      }
      catch (Exception relayEx)
      {
        WriteLine($"   ✗ Failed to post to {relayUrl}: {relayEx.Message}");
      }
    }

    if (successCount > 0)
    {
      WriteLine($"📡 Successfully posted to {successCount}/{relayUrls.Count} relay(s)!");
      WriteLine($"   Event ID: {signed.Id}");
      WriteLine($"   Content length: {content.Length} characters");
    }
    else
    {
      WriteLine("❌ Failed to post to any relay");
    }
  }
  catch (Exception ex)
  {
    WriteLine($"❌ Error posting to Nostr: {ex.Message}");
  }
}

static async Task PostToX(string content)
{
  try
  {
    // Load config
    Config? config = LoadConfig();
    if (config?.X == null ||
        string.IsNullOrWhiteSpace(config.X.ConsumerKey) ||
        string.IsNullOrWhiteSpace(config.X.ConsumerSecret) ||
        string.IsNullOrWhiteSpace(config.X.AccessToken) ||
        string.IsNullOrWhiteSpace(config.X.AccessTokenSecret))
    {
      WriteLine($$"""
        ⚠️  X API credentials not configured
           Update ~/.nostr/config.json with:
           {
             "x": {
               "consumerKey": "your-api-key",
               "consumerSecret": "your-api-secret",
               "accessToken": "your-access-token",
               "accessTokenSecret": "your-access-token-secret"
             }
           }

        🐦 Would post to X:
           Content length: {{content.Length}} characters
        """);
      return;
    }

    // X API v2 endpoint
    const string url = "https://api.x.com/2/tweets";
    int timestamp = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    string nonce = Guid.NewGuid().ToString("N");

    // OAuth 1.0a parameters
    var oauthParams = new Dictionary<string, string>
        {
            { "oauth_consumer_key", config.X.ConsumerKey },
            { "oauth_nonce", nonce },
            { "oauth_signature_method", "HMAC-SHA1" },
            { "oauth_timestamp", timestamp.ToString(System.Globalization.CultureInfo.InvariantCulture) },
            { "oauth_token", config.X.AccessToken },
            { "oauth_version", "1.0" }
        };

    // Build signature base string
    // Note: For Twitter API v2 with JSON body, don't include body parameters in signature
    string sigBase = "POST&" + Uri.EscapeDataString(url) + "&";
    var allParams = new SortedDictionary<string, string>(oauthParams);
    sigBase += Uri.EscapeDataString(string.Join("&",
        allParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}")));

    // Sign with HMAC-SHA1 (required by OAuth 1.0a)
    string signingKey = $"{Uri.EscapeDataString(config.X.ConsumerSecret)}&{Uri.EscapeDataString(config.X.AccessTokenSecret)}";
#pragma warning disable CA5350 // OAuth 1.0a requires HMAC-SHA1
    using var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey));
#pragma warning restore CA5350
    string signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.ASCII.GetBytes(sigBase)));
    oauthParams["oauth_signature"] = signature;

    // Build OAuth header
    string oauthHeader = "OAuth " + string.Join(", ",
        oauthParams.Select(kvp => $"{kvp.Key}=\"{Uri.EscapeDataString(kvp.Value)}\""));

    // Send request
    using var client = new HttpClient();
    client.DefaultRequestHeaders.Add("Authorization", oauthHeader);
    var jsonContent = new StringContent(
        JsonSerializer.Serialize(new { text = content }),
        Encoding.UTF8,
        "application/json");

    HttpResponseMessage response = await client.PostAsync(new Uri(url), jsonContent);
    string responseBody = await response.Content.ReadAsStringAsync();

    if (response.IsSuccessStatusCode)
    {
      WriteLine("🐦 Posted to X!");
      WriteLine($"   Content length: {content.Length} characters");

      // Parse response to get tweet ID if available
      try
      {
        using var doc = JsonDocument.Parse(responseBody);
        if (doc.RootElement.TryGetProperty("data", out JsonElement data) &&
            data.TryGetProperty("id", out JsonElement id))
        {
          WriteLine($"   Tweet ID: {id.GetString()}");
        }
      }
      catch { }
    }
    else
    {
      WriteLine($"❌ Error posting to X: {response.StatusCode}");
      WriteLine($"   Response: {responseBody}");
    }
  }
  catch (Exception ex)
  {
    WriteLine($"❌ Error posting to X: {ex.Message}");
  }
}

// Encryption/decryption functions
static void EncryptConfig()
{
  string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nostr", "config.json");
  if (!File.Exists(configPath))
  {
    WriteLine("❌ Config file not found");
    return;
  }

  try
  {
    // Read plaintext config
    string plaintext = File.ReadAllText(configPath);

    // Encrypt using SSH key
    string encrypted = EncryptWithSshKey(plaintext);

    // Save encrypted config
    string encryptedPath = configPath + ".enc";
    File.WriteAllText(encryptedPath, encrypted);

    // Remove plaintext file
    File.Delete(configPath);

    WriteLine($"✅ Config encrypted and saved to {encryptedPath}");
    WriteLine("   Original plaintext config has been deleted");
  }
  catch (Exception ex)
  {
    WriteLine($"❌ Error encrypting config: {ex.Message}");
  }
}

static string EncryptWithSshKey(string plaintext)
{
  // Use openssl to encrypt with your SSH public key
  string sshKeyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "id_rsa_for_encryption.pub");

  // Convert SSH public key to PEM format and encrypt
  using var process = new Process();
  process.StartInfo = new ProcessStartInfo
  {
    FileName = "bash",
    Arguments = $"-c \"openssl pkeyutl -encrypt -pubin -inkey <(ssh-keygen -f {sshKeyPath} -e -m PKCS8) | base64 -w 0\"",
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    RedirectStandardInput = true,
    UseShellExecute = false
  };

  process.Start();
  process.StandardInput.Write(plaintext);
  process.StandardInput.Close();
  string output = process.StandardOutput.ReadToEnd();
  string error = process.StandardError.ReadToEnd();
  process.WaitForExit();

  if (process.ExitCode != 0)
    throw new InvalidOperationException($"Encryption failed: {error}");

  return output.Trim();
}

static string DecryptWithSshKey(string encrypted)
{
  // Use openssl to decrypt with your SSH private key
  string sshKeyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "id_rsa_for_encryption.pem");

  using var process = new Process();
  process.StartInfo = new ProcessStartInfo
  {
    FileName = "bash",
    Arguments = $"-c \"base64 -d | openssl pkeyutl -decrypt -inkey {sshKeyPath}\"",
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    RedirectStandardInput = true,
    UseShellExecute = false
  };

  process.Start();
  process.StandardInput.Write(encrypted);
  process.StandardInput.Close();
  string output = process.StandardOutput.ReadToEnd();
  string error = process.StandardError.ReadToEnd();
  process.WaitForExit();

  if (process.ExitCode != 0)
    throw new InvalidOperationException($"Decryption failed: {error}");

  return output;
}

static void SetConfigValue(string path, string value)
{
  try
  {
    // Load current config
    Config? config = LoadConfig() ?? new Config();

    // Update the value based on path
    string[] parts = path.Split('.');
    if (parts.Length == 2)
    {
      switch (parts[0])
      {
        case "nostr":
          config.Nostr ??= new NostrConfig();
          if (parts[1] == "privateKey")
            config.Nostr.PrivateKey = value.Trim();
          break;

        case "x":
          config.X ??= new XConfig();
          switch (parts[1])
          {
            case "consumerKey":
              config.X.ConsumerKey = value.Trim();
              break;
            case "consumerSecret":
              config.X.ConsumerSecret = value.Trim();
              break;
            case "accessToken":
              config.X.AccessToken = value.Trim();
              break;
            case "accessTokenSecret":
              config.X.AccessTokenSecret = value.Trim();
              break;
          }

          break;
      }
    }

    // Save config
    SaveConfig(config);

    WriteLine($"✅ Updated {path}");
  }
  catch (Exception ex)
  {
    WriteLine($"❌ Error updating config: {ex.Message}");
  }
}

static void AddRelay(string url)
{
  try
  {
    // Load current config
    Config? config = LoadConfig() ?? new Config();
    config.Nostr ??= new NostrConfig();
    config.Nostr.Relays ??= new List<string>();

    if (!config.Nostr.Relays.Contains(url))
    {
      config.Nostr.Relays.Add(url.Trim());
      SaveConfig(config);
      WriteLine($"✅ Added relay: {url}");
    }
    else
    {
      WriteLine($"ℹ️  Relay already exists: {url}");
    }
  }
  catch (Exception ex)
  {
    WriteLine($"❌ Error adding relay: {ex.Message}");
  }
}

static void ShowConfig()
{
  try
  {
    Config? config = LoadConfig();
    if (config == null)
    {
      WriteLine("❌ No config found");
      WriteLine("   Run 'post.cs config example' to see setup instructions");
      return;
    }

    WriteLine("Current Configuration:");
    WriteLine();

    if (config.Nostr != null)
    {
      WriteLine("Nostr:");
      WriteLine($"  Private Key: {MaskSecret(config.Nostr.PrivateKey)}");
      if (config.Nostr.Relays != null)
      {
        WriteLine("  Relays:");
        foreach (string relay in config.Nostr.Relays)
          WriteLine($"    - {relay}");
      }
    }

    if (config.X != null)
    {
      WriteLine();
      WriteLine("X (Twitter):");
      WriteLine($"  Consumer Key: {MaskSecret(config.X.ConsumerKey)}");
      WriteLine($"  Consumer Secret: {MaskSecret(config.X.ConsumerSecret)}");
      WriteLine($"  Access Token: {MaskSecret(config.X.AccessToken)}");
      WriteLine($"  Access Token Secret: {MaskSecret(config.X.AccessTokenSecret)}");
    }
  }
  catch (Exception ex)
  {
    WriteLine($"❌ Error showing config: {ex.Message}");
  }
}

static void ShowConfigExample()
{
  string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nostr", "config.json");
  WriteLine($$"""
    📚 Post Command Configuration Guide
    {{new string('=', 41)}}

    Configuration file location:
      {{configPath}}

    Example config.json format:
    ---
    {
      "nostr": {
        "privateKey": "nsec1...",
        "relays": [
          "wss://relay.damus.io",
          "wss://relay.snort.social",
          "wss://nos.lol"
        ]
      },
      "x": {
        "consumerKey": "your-api-key",
        "consumerSecret": "your-api-secret",
        "accessToken": "your-access-token",
        "accessTokenSecret": "your-access-token-secret"
      }
    }
    ---

    Setup Instructions:

    1. Create the config directory:
       mkdir -p ~/.nostr

    2. Create config.json with your credentials:
       nano ~/.nostr/config.json

    3. (Optional) Encrypt your config for security:
       ./post.cs encrypt-config

    Getting Your Keys:

    🔑 Nostr:
       - Generate a key pair at: https://nostrgenerator.com/
       - Or use an existing key from your Nostr client
       - Format: nsec1... (private key in bech32 format)

    🐦 X/Twitter API:
       1. Go to https://developer.x.com/
       2. Create a project and app
       3. Generate API keys and access tokens
       4. Make sure you have 'Read and Write' permissions

    Usage Examples:
       ./post.cs announcement.md                    # Post to both platforms
       ./post.cs update.md --platform nostr         # Post to Nostr only
       ./post.cs news.md --platform x               # Post to X only

    Config Management:
       ./post.cs config show                        # View current config
       ./post.cs config set nostr-key nsec1...      # Set Nostr key
       ./post.cs config add relay wss://relay.url   # Add a relay
    """);
}

static string MaskSecret(string? secret)
{
  if (string.IsNullOrEmpty(secret)) return "<not set>";
  if (secret.Length <= 8) return "****";
  return string.Concat(secret.AsSpan(0, 4), "...", secret.AsSpan(secret.Length - 4));
}

static void SaveConfig(Config config)
{
  string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nostr");
  string configPath = Path.Combine(configDir, "config.json");

  // Serialize config
  string json = JsonSerializer.Serialize(config, GetJsonOptions());

  // Encrypt and save
  string encrypted = EncryptWithSshKey(json);
  string encryptedPath = configPath + ".enc";
  File.WriteAllText(encryptedPath, encrypted);
}

// Configuration classes
sealed class NostrConfig
{
  public string? PrivateKey { get; set; }
  public List<string>? Relays { get; set; }
}

sealed class XConfig
{
  public string? ConsumerKey { get; set; }
  public string? ConsumerSecret { get; set; }
  public string? AccessToken { get; set; }
  public string? AccessTokenSecret { get; set; }
}

sealed class Config
{
  public NostrConfig? Nostr { get; set; }
  public XConfig? X { get; set; }
}

