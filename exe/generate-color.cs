#!/usr/bin/dotnet --

using System.Security.Cryptography;
using System.Text;

static string RepoNameToColor(string repoName)
{
    // Using MD5 for color generation is safe - not for security
    #pragma warning disable CA5351
    byte[] inputBytes = Encoding.UTF8.GetBytes(repoName);
    byte[] hashBytes = MD5.HashData(inputBytes);
    #pragma warning restore CA5351
    
    // Take first 3 bytes for RGB color
    string hex = Convert.ToHexStringLower(hashBytes.Take(3).ToArray());
    return $"#{hex}";
}

if (args.Length == 0)
{
    Console.WriteLine("Please provide a repository name as an argument.");
    return;
}

string repoName = args[0];
string color = RepoNameToColor(repoName);
Console.WriteLine($"Repo: {repoName}, Color: {color}");