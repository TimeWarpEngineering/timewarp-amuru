#!/usr/bin/dotnet --

#region Purpose
// Tests for DotNet.Test() - validates the fluent API for running .NET tests
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: DotNet (the static class providing command builders)
// Action: Test (the method being tested)
// Tests verify command string generation for various test configuration options
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class Test_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Test_Given_>();

    public static async Task NoOptions_Should_BuildBasicCommand()
    {
      string command = DotNet.Test()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet test");

      await Task.CompletedTask;
    }

    public static async Task WithProject_Should_IncludeProjectPath()
    {
      string command = DotNet.Test()
        .WithProject("MyApp.Tests.csproj")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet test MyApp.Tests.csproj");

      await Task.CompletedTask;
    }

    public static async Task FluentConfiguration_Should_BuildCompleteCommand()
    {
      string command = DotNet.Test()
        .WithProject("test.csproj")
        .WithConfiguration("Debug")
        .WithFramework("net10.0")
        .WithNoRestore()
        .WithFilter("Category=Unit")
        .WithLogger("console")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet test test.csproj --configuration Debug --framework net10.0 --filter Category=Unit --logger console --no-restore");

      await Task.CompletedTask;
    }

    public static async Task AdvancedOptions_Should_IncludeAllOptions()
    {
      string command = DotNet.Test()
        .WithProject("test.csproj")
        .WithConfiguration("Release")
        .WithArchitecture("x64")
        .WithOperatingSystem("linux")
        .WithNoRestore()
        .WithNoBuild()
        .WithVerbosity("minimal")
        .WithFilter("TestCategory=Integration")
        .WithLogger("trx")
        .WithLogger("html")
        .WithBlame()
        .WithCollect()
        .WithResultsDirectory("TestResults")
        .WithProperty("Platform", "AnyCPU")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet test test.csproj --configuration Release --arch x64 --os linux --verbosity minimal --filter TestCategory=Integration --results-directory TestResults --logger trx --logger html --no-restore --no-build --blame --collect --property:Platform=AnyCPU");

      await Task.CompletedTask;
    }

    public static async Task WorkingDirectoryAndEnvVars_Should_NotAppearInCommandString()
    {
      string command = DotNet.Test()
        .WithProject("test.csproj")
        .WithWorkingDirectory("/tmp")
        .WithEnvironmentVariable("TEST_ENV", "integration")
        .WithNoLogo()
        .WithSettings("test.runsettings")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet test test.csproj --settings test.runsettings --nologo");

      await Task.CompletedTask;
    }

    public static async Task NonExistentProject_Should_BuildValidCommand()
    {
      string command = DotNet.Test()
        .WithProject("nonexistent.csproj")
        .WithConfiguration("Debug")
        .WithNoRestore()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet test nonexistent.csproj --configuration Debug --no-restore");

      await Task.CompletedTask;
    }
  }
}
