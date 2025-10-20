#!/usr/bin/dotnet --

#:package TimeWarp.Multiavatar
#:package TimeWarp.Nuru
#:property NoWarn=IL2104;IL3053

using TimeWarp.Multiavatar;
using TimeWarp.Nuru;
using static System.Console;

NuruApp app =
  new NuruAppBuilder()
  .AddAutoHelp()
  .AddRoute
  (
    "{input|Text to generate avatar from (email, username, etc)} " +
    "--output,-o? {file?|Save SVG to file instead of stdout} " +
    "--no-env|Generate without environment circle " +
    "--output-hash|Display hash calculation details instead of SVG",
    GenerateAvatar,
    "Generate unique, deterministic SVG avatars from any text input"
  )
  .Build();

return await app.RunAsync(args);

static void GenerateAvatar(string input, string? file, bool noEnv, bool outputHash)
{
  // If output-hash flag is set, show hash information instead
  if (outputHash)
  {
    string hash = HashService.GenerateHash(input);
    string sha256Hash = HashService.GetSha256Hash(input);
    string sha256Numbers = HashService.GetSha256Numbers(sha256Hash);

    WriteLine($"{input}:");
    WriteLine($"  SHA256: {sha256Hash}");
    WriteLine($"  Numbers: {sha256Numbers}");
    WriteLine($"  Hash-12: {hash}");
    WriteLine("  Parts:");
    WriteLine($"    env: {hash[..2]} -> {HashService.GetPartSelection(hash[..2])}");
    WriteLine($"    clo: {hash[2..4]} -> {HashService.GetPartSelection(hash[2..4])}");
    WriteLine($"    head: {hash[4..6]} -> {HashService.GetPartSelection(hash[4..6])}");
    WriteLine($"    mouth: {hash[6..8]} -> {HashService.GetPartSelection(hash[6..8])}");
    WriteLine($"    eyes: {hash[8..10]} -> {HashService.GetPartSelection(hash[8..10])}");
    WriteLine($"    top: {hash[10..12]} -> {HashService.GetPartSelection(hash[10..12])}");
    return;
  }

  // Generate the avatar
  string svg = MultiavatarGenerator.Generate(input, sansEnv: noEnv);

  // Output to file or stdout
  if (!string.IsNullOrEmpty(file))
  {
    File.WriteAllText(file, svg);
    WriteLine($"Avatar saved to: {file}");
  }
  else
  {
    Write(svg);
  }
}