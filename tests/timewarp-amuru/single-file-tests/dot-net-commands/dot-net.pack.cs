#!/usr/bin/dotnet --

#region Purpose
// Tests for DotNet.Pack() - validates the fluent API for packing .NET projects into NuGet packages
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: DotNet (the static class providing command builders)
// Action: Pack (the method being tested)
// Tests verify command string generation for various packaging options
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class Pack_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Pack_Given_>();

    public static async Task NoOptions_Should_BuildBasicCommand()
    {
      string command = DotNet.Pack()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet pack");

      await Task.CompletedTask;
    }

    public static async Task WithProject_Should_IncludeProjectPath()
    {
      string command = DotNet.Pack()
        .WithProject("MyApp.csproj")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet pack MyApp.csproj");

      await Task.CompletedTask;
    }

    public static async Task FluentConfiguration_Should_BuildCompleteCommand()
    {
      string command = DotNet.Pack()
        .WithProject("test.csproj")
        .WithConfiguration("Release")
        .WithFramework("net10.0")
        .WithRuntime("win-x64")
        .WithOutput("./packages")
        .WithNoRestore()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet pack test.csproj --configuration Release --framework net10.0 --runtime win-x64 --output ./packages --no-restore");

      await Task.CompletedTask;
    }

    public static async Task PackageOptions_Should_IncludeAllOptions()
    {
      string command = DotNet.Pack()
        .WithProject("test.csproj")
        .WithConfiguration("Release")
        .WithVersionSuffix("beta")
        .IncludeSymbols()
        .IncludeSource()
        .WithServiceable()
        .WithNoLogo()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet pack test.csproj --configuration Release --version-suffix beta --nologo --include-symbols --include-source --serviceable");

      await Task.CompletedTask;
    }

    public static async Task WorkingDirectoryAndEnvVars_Should_NotAppearInCommandString()
    {
      string command = DotNet.Pack()
        .WithProject("test.csproj")
        .WithWorkingDirectory("/tmp")
        .WithEnvironmentVariable("PACK_ENV", "production")
        .WithVerbosity("detailed")
        .WithTerminalLogger("on")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet pack test.csproj --verbosity detailed --tl on");

      await Task.CompletedTask;
    }

    public static async Task MSBuildPropertiesAndSources_Should_IncludeAllOptions()
    {
      string command = DotNet.Pack()
        .WithProject("test.csproj")
        .WithConfiguration("Release")
        .WithProperty("PackageVersion", "1.0.0")
        .WithProperty("PackageDescription", "Test package")
        .WithSource("https://api.nuget.org/v3/index.json")
        .WithNoBuild()
        .WithForce()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet pack test.csproj --configuration Release --source https://api.nuget.org/v3/index.json --no-build --force --property:PackageVersion=1.0.0 \"--property:PackageDescription=Test package\"");

      await Task.CompletedTask;
    }

    public static async Task ProjectOverload_Should_SetProject()
    {
      string command = DotNet.Pack("test.csproj")
        .WithConfiguration("Release")
        .WithOutput("./dist")
        .WithVersionSuffix("rc1")
        .WithNoDependencies()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet pack test.csproj --configuration Release --output ./dist --version-suffix rc1 --no-dependencies");

      await Task.CompletedTask;
    }

    public static async Task NonExistentProject_Should_BuildValidCommand()
    {
      string command = DotNet.Pack()
        .WithProject("nonexistent.csproj")
        .WithConfiguration("Release")
        .WithOutput("./packages")
        .WithNoRestore()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet pack nonexistent.csproj --configuration Release --output ./packages --no-restore");

      await Task.CompletedTask;
    }
  }
}
