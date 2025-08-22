#!/usr/bin/dotnet --
#:project ../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:property RestoreNoCache=true
#:property DisableImplicitNuGetFallbackFolder=true
#:property LangVersion=preview
#:property EnablePreviewFeatures=true

using TimeWarp.Amuru;

Console.WriteLine("Testing AppContext extensions:");
Console.WriteLine($"EntryPointFilePath: {AppContext.EntryPointFilePath()}");
Console.WriteLine($"EntryPointFileDirectoryPath: {AppContext.EntryPointFileDirectoryPath()}");

Console.WriteLine("\nTesting ScriptContext:");
using (var context = ScriptContext.FromEntryPoint())
{
  Console.WriteLine($"ScriptFilePath: {context.ScriptFilePath}");
  Console.WriteLine($"ScriptDirectory: {context.ScriptDirectory}");
  Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");
}

Console.WriteLine($"Directory restored: {Directory.GetCurrentDirectory()}");

return 0;