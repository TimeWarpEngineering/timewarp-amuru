#!/usr/bin/dotnet --

#:project ../Source/TimeWarp.Multiavatar/TimeWarp.Multiavatar.csproj
#:property TrimMode=partial
#:property NoWarn=IL2104

using TimeWarp.Multiavatar;
using TimeWarp.Nuru;
using static System.Console;

var builder = new NuruAppBuilder();

// Default route - generate avatar from input
builder.AddRoute("{input}", (string input) =>
{
    string svg = MultiavatarGenerator.Generate(input);
    Write(svg);
});

// Generate with output file
builder.AddRoute("{input} --output {file}", (string input, string file) =>
{
    string svg = MultiavatarGenerator.Generate(input);
    File.WriteAllText(file, svg);
    WriteLine($"Avatar saved to: {file}");
});

// Generate without environment circle
builder.AddRoute("{input} --no-env", (string input) =>
{
    string svg = MultiavatarGenerator.Generate(input, sansEnv: true);
    Write(svg);
});

// Generate without environment and save to file
builder.AddRoute("{input} --no-env --output {file}", (string input, string file) =>
{
    string svg = MultiavatarGenerator.Generate(input, sansEnv: true);
    File.WriteAllText(file, svg);
    WriteLine($"Avatar saved to: {file}");
});

// Output hash information
builder.AddRoute("{input} --output-hash", (string input) =>
{
    string hash = HashService.GenerateHash(input);
    string sha256Hash = HashService.GetSha256Hash(input);
    string sha256Numbers = HashService.GetSha256Numbers(sha256Hash);
    
    WriteLine($"{input}:");
    WriteLine($"  SHA256: {sha256Hash}");
    WriteLine($"  Numbers: {sha256Numbers}");
    WriteLine($"  Hash-12: {hash}");
    WriteLine("  Parts:");
    WriteLine($"    env: {hash.Substring(0, 2)} -> {HashService.GetPartSelection(hash.Substring(0, 2))}");
    WriteLine($"    clo: {hash.Substring(2, 2)} -> {HashService.GetPartSelection(hash.Substring(2, 2))}");
    WriteLine($"    head: {hash.Substring(4, 2)} -> {HashService.GetPartSelection(hash.Substring(4, 2))}");
    WriteLine($"    mouth: {hash.Substring(6, 2)} -> {HashService.GetPartSelection(hash.Substring(6, 2))}");
    WriteLine($"    eyes: {hash.Substring(8, 2)} -> {HashService.GetPartSelection(hash.Substring(8, 2))}");
    WriteLine($"    top: {hash.Substring(10, 2)} -> {HashService.GetPartSelection(hash.Substring(10, 2))}");
});

// Help command
builder.AddRoute("help", () =>
{
    WriteLine("TimeWarp.Multiavatar - Generate unique avatars from text");
    WriteLine();
    WriteLine("Usage:");
    WriteLine("  multiavatar <input>                        Generate avatar and output to stdout");
    WriteLine("  multiavatar <input> --output <file>        Generate avatar and save to file");
    WriteLine("  multiavatar <input> --no-env               Generate without environment circle");
    WriteLine("  multiavatar <input> --no-env --output <f>  Generate without env and save");
    WriteLine("  multiavatar <input> --output-hash          Output hash information");
    WriteLine("  multiavatar help                           Show this help");
    WriteLine();
    WriteLine("Examples:");
    WriteLine("  multiavatar \"user@example.com\"");
    WriteLine("  multiavatar \"John Doe\" --output john.svg");
    WriteLine("  multiavatar \"timewarp-flow\" --no-env");
});

NuruApp app = builder.Build();
return await app.RunAsync(args);