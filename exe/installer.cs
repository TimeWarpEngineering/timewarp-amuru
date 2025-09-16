#!/usr/bin/dotnet --
#:project ../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj

using TimeWarp.Amuru;

// Simple wrapper that calls the Amuru library's installer
return await Installer.InstallUtilitiesAsync(args);