#!/usr/bin/dotnet --

#:package TimeWarp.Nuru
#:package Blockcore.Nostr.Client
#:property PublishAot=false

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

var builder = new DirectAppBuilder();

// Post markdown file to both platforms (default)
builder.AddRoute("{file}", async (string file) => await PostToAll(file));

// Post to specific platform
builder.AddRoute("{file} --platform {platform}", async (string file, string platform) => 
{
    string? content = ReadMarkdownFile(file);
    if (content == null) return;
    
    switch (platform.ToLowerInvariant())
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
            await PostToNostr(content);
            await PostToX(content);
            break;
        default:
            Console.WriteLine($"Unknown platform: {platform}. Use 'nostr', 'x', or 'both'");
            break;
    }
});

// Config management commands
builder.AddRoute("encrypt-config", () => EncryptConfig());
builder.AddRoute("config set nostr-key {key}", (string key) => SetConfigValue("nostr.privateKey", key));
builder.AddRoute("config set x-consumer-key {key}", (string key) => SetConfigValue("x.consumerKey", key));
builder.AddRoute("config set x-consumer-secret {secret}", (string secret) => SetConfigValue("x.consumerSecret", secret));
builder.AddRoute("config set x-access-token {token}", (string token) => SetConfigValue("x.accessToken", token));
builder.AddRoute("config set x-access-secret {secret}", (string secret) => SetConfigValue("x.accessTokenSecret", secret));
builder.AddRoute("config add relay {url}", (string url) => AddRelay(url));
builder.AddRoute("config show", () => ShowConfig());

// Help command
builder.AddRoute("help", () => 
{
    Console.WriteLine("Post Command - Publish Markdown to Social Platforms");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  ./Post.cs <file>                           - Post to both Nostr and X");
    Console.WriteLine("  ./Post.cs <file> --platform <platform>     - Post to specific platform");
    Console.WriteLine();
    Console.WriteLine("Config Management:");
    Console.WriteLine("  ./Post.cs config show                      - Show current config (masked)");
    Console.WriteLine("  ./Post.cs config set nostr-key <key>       - Set Nostr private key");
    Console.WriteLine("  ./Post.cs config set x-consumer-key <key>  - Set X consumer key");
    Console.WriteLine("  ./Post.cs config set x-consumer-secret <s> - Set X consumer secret");
    Console.WriteLine("  ./Post.cs config set x-access-token <t>    - Set X access token");
    Console.WriteLine("  ./Post.cs config set x-access-secret <s>   - Set X access token secret");
    Console.WriteLine("  ./Post.cs config add relay <url>           - Add Nostr relay");
    Console.WriteLine("  ./Post.cs encrypt-config                   - Encrypt existing config.json");
    Console.WriteLine();
    Console.WriteLine("Platforms:");
    Console.WriteLine("  nostr    - Post to Nostr");
    Console.WriteLine("  x        - Post to X (Twitter)");
    Console.WriteLine("  both     - Post to both platforms (default)");
    Console.WriteLine();
    Console.WriteLine("Configuration:");
    Console.WriteLine("  Config file: ~/.nostr/config.json");
    Console.WriteLine("  Example:");
    Console.WriteLine("  {");
    Console.WriteLine("    \"nostr\": {");
    Console.WriteLine("      \"privateKey\": \"nsec1...\",");
    Console.WriteLine("      \"relays\": [\"wss://relay.damus.io\", \"wss://relay.snort.social\"]");
    Console.WriteLine("    },");
    Console.WriteLine("    \"x\": {");
    Console.WriteLine("      \"consumerKey\": \"your-api-key\",");
    Console.WriteLine("      \"consumerSecret\": \"your-api-secret\",");
    Console.WriteLine("      \"accessToken\": \"your-access-token\",");
    Console.WriteLine("      \"accessTokenSecret\": \"your-access-token-secret\"");
    Console.WriteLine("    }");
    Console.WriteLine("  }");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  ./Post.cs announcement.md");
    Console.WriteLine("  ./Post.cs update.md --platform nostr");
    Console.WriteLine("  ./Post.cs news.md --platform x");
});

DirectApp app = builder.Build();
return await app.RunAsync(args);

// Shared JSON options
JsonSerializerOptions GetJsonOptions() => new() 
{ 
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
};

// Helper functions
Config? LoadConfig()
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
            Console.WriteLine($"❌ Error reading encrypted config: {ex.Message}");
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
            Console.WriteLine($"❌ Error reading config file: {ex.Message}");
            return null;
        }
    }
    
    return null;
}

async Task PostToAll(string file)
{
    string? content = ReadMarkdownFile(file);
    if (content != null)
    {
        await PostToNostr(content);
        await PostToX(content);
    }
}

string? ReadMarkdownFile(string file)
{
    if (!File.Exists(file))
    {
        Console.WriteLine($"❌ File not found: {file}");
        return null;
    }
    
    if (!file.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine($"⚠️  Warning: {file} doesn't appear to be a markdown file");
    }
    
    try
    {
        return File.ReadAllText(file);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error reading file: {ex.Message}");
        return null;
    }
}

async Task PostToNostr(string content)
{
    try
    {
        // Load config
        Config? config = LoadConfig();
        if (config?.Nostr?.PrivateKey == null)
        {
            Console.WriteLine("⚠️  Nostr private key not configured");
            Console.WriteLine("   Create ~/.nostr/config.json with:");
            Console.WriteLine("   {");
            Console.WriteLine("     \"nostr\": {");
            Console.WriteLine("       \"privateKey\": \"nsec1...\",");
            Console.WriteLine("       \"relays\": [\"wss://relay.damus.io\", \"wss://relay.snort.social\"]");
            Console.WriteLine("     }");
            Console.WriteLine("   }");
            Console.WriteLine();
            Console.WriteLine("📡 Would post to Nostr:");
            Console.WriteLine($"   Content length: {content.Length} characters");
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
        
        Console.WriteLine($"📡 Posting to {relayUrls.Count} relay(s)...");
        Console.WriteLine($"   Author: {pubkey}");
        
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
                
                Console.WriteLine($"   ✓ Posted to {uri.Host}");
                successCount++;
            }
            catch (Exception relayEx)
            {
                Console.WriteLine($"   ✗ Failed to post to {relayUrl}: {relayEx.Message}");
            }
        }
        
        if (successCount > 0)
        {
            Console.WriteLine($"📡 Successfully posted to {successCount}/{relayUrls.Count} relay(s)!");
            Console.WriteLine($"   Event ID: {signed.Id}");
            Console.WriteLine($"   Content length: {content.Length} characters");
        }
        else
        {
            Console.WriteLine("❌ Failed to post to any relay");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error posting to Nostr: {ex.Message}");
    }
}

async Task PostToX(string content)
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
            Console.WriteLine("⚠️  X API credentials not configured");
            Console.WriteLine("   Update ~/.nostr/config.json with:");
            Console.WriteLine("   {");
            Console.WriteLine("     \"x\": {");
            Console.WriteLine("       \"consumerKey\": \"your-api-key\",");
            Console.WriteLine("       \"consumerSecret\": \"your-api-secret\",");
            Console.WriteLine("       \"accessToken\": \"your-access-token\",");
            Console.WriteLine("       \"accessTokenSecret\": \"your-access-token-secret\"");
            Console.WriteLine("     }");
            Console.WriteLine("   }");
            Console.WriteLine();
            Console.WriteLine("🐦 Would post to X:");
            Console.WriteLine($"   Content length: {content.Length} characters");
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
            Console.WriteLine("🐦 Posted to X!");
            Console.WriteLine($"   Content length: {content.Length} characters");
            
            // Parse response to get tweet ID if available
            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                if (doc.RootElement.TryGetProperty("data", out JsonElement data) &&
                    data.TryGetProperty("id", out JsonElement id))
                {
                    Console.WriteLine($"   Tweet ID: {id.GetString()}");
                }
            }
            catch { }
        }
        else
        {
            Console.WriteLine($"❌ Error posting to X: {response.StatusCode}");
            Console.WriteLine($"   Response: {responseBody}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error posting to X: {ex.Message}");
    }
}

// Encryption/decryption functions
void EncryptConfig()
{
    string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nostr", "config.json");
    if (!File.Exists(configPath))
    {
        Console.WriteLine("❌ Config file not found");
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
        
        Console.WriteLine($"✅ Config encrypted and saved to {encryptedPath}");
        Console.WriteLine("   Original plaintext config has been deleted");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error encrypting config: {ex.Message}");
    }
}

string EncryptWithSshKey(string plaintext)
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

string DecryptWithSshKey(string encrypted)
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

void SetConfigValue(string path, string value)
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
        
        Console.WriteLine($"✅ Updated {path}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error updating config: {ex.Message}");
    }
}

void AddRelay(string url)
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
            Console.WriteLine($"✅ Added relay: {url}");
        }
        else
        {
            Console.WriteLine($"ℹ️  Relay already exists: {url}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error adding relay: {ex.Message}");
    }
}

void ShowConfig()
{
    try
    {
        Config? config = LoadConfig();
        if (config == null)
        {
            Console.WriteLine("❌ No config found");
            return;
        }
        
        Console.WriteLine("Current Configuration:");
        Console.WriteLine();
        
        if (config.Nostr != null)
        {
            Console.WriteLine("Nostr:");
            Console.WriteLine($"  Private Key: {MaskSecret(config.Nostr.PrivateKey)}");
            if (config.Nostr.Relays != null)
            {
                Console.WriteLine("  Relays:");
                foreach (string relay in config.Nostr.Relays)
                    Console.WriteLine($"    - {relay}");
            }
        }
        
        if (config.X != null)
        {
            Console.WriteLine();
            Console.WriteLine("X (Twitter):");
            Console.WriteLine($"  Consumer Key: {MaskSecret(config.X.ConsumerKey)}");
            Console.WriteLine($"  Consumer Secret: {MaskSecret(config.X.ConsumerSecret)}");
            Console.WriteLine($"  Access Token: {MaskSecret(config.X.AccessToken)}");
            Console.WriteLine($"  Access Token Secret: {MaskSecret(config.X.AccessTokenSecret)}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error showing config: {ex.Message}");
    }
}

string MaskSecret(string? secret)
{
    if (string.IsNullOrEmpty(secret)) return "<not set>";
    if (secret.Length <= 8) return "****";
    return string.Concat(secret.AsSpan(0, 4), "...", secret.AsSpan(secret.Length - 4));
}

void SaveConfig(Config config)
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

