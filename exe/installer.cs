#!/usr/bin/dotnet --
#:package TimeWarp.Amuru@1.0.0-beta.10

using TimeWarp.Amuru;

// Simple wrapper that calls the Amuru library's installer
return await Installer.InstallUtilitiesAsync(args);