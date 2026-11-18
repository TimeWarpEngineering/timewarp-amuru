#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for DotNet.Build() - validates the fluent API for building .NET projects
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: DotNet (the static class providing command builders)
// Action: Build (the method being tested)
// Tests verify command string generation for various configuration options.
// WithTimeout is an execution limit, so the command string stays unchanged and a mock proves the limit.
// WithNoLogo emits --nologo. The runfile-coverage shape combines it with --artifacts-path and --verbosity.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class Build_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Build_Given_>();

    public static async Task NoOptions_Should_BuildBasicCommand()
    {
      string command = DotNet.Build()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet build");

      await Task.CompletedTask;
    }

    public static async Task WithProject_Should_IncludeProjectPath()
    {
      string command = DotNet.Build()
        .WithProject("MyApp.csproj")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet build MyApp.csproj");

      await Task.CompletedTask;
    }

    public static async Task FluentConfiguration_Should_BuildCompleteCommand()
    {
      string command = DotNet.Build()
        .WithProject("test.csproj")
        .WithConfiguration("Debug")
        .WithFramework("net11.0")
        .WithNoRestore()
        .WithOutput("bin/Debug")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet build test.csproj --configuration Debug --framework net11.0 --output bin/Debug --no-restore");

      await Task.CompletedTask;
    }

    public static async Task AdvancedOptions_Should_IncludeAllOptions()
    {
      string command = DotNet.Build()
        .WithProject("test.csproj")
        .WithConfiguration("Release")
        .WithArchitecture("x64")
        .WithOperatingSystem("linux")
        .WithNoRestore()
        .WithNoDependencies()
        .WithNoIncremental()
        .WithVerbosity("minimal")
        .WithProperty("Platform", "AnyCPU")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet build test.csproj --configuration Release --arch x64 --os linux --verbosity minimal --no-restore --no-dependencies --no-incremental --property:Platform=AnyCPU");

      await Task.CompletedTask;
    }

    public static async Task WorkingDirectoryAndEnvVars_Should_NotAppearInCommandString()
    {
      string command = DotNet.Build()
        .WithProject("test.csproj")
        .WithWorkingDirectory("/tmp")
        .WithEnvironmentVariable("BUILD_ENV", "test")
        .WithNoLogo()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet build test.csproj --nologo");

      await Task.CompletedTask;
    }

    public static async Task MSBuildProperties_Should_IncludeAllProperties()
    {
      string command = DotNet.Build()
        .WithProject("test.csproj")
        .WithConfiguration("Debug")
        .WithProperty("RestoreNoCache", "true")
        .WithProperty("DisableImplicitNuGetFallbackFolder", "true")
        .WithProperty("RestoreIgnoreFailedSources", "true")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet build test.csproj --configuration Debug --property:RestoreNoCache=true --property:DisableImplicitNuGetFallbackFolder=true --property:RestoreIgnoreFailedSources=true");

      await Task.CompletedTask;
    }

    public static async Task ProjectOverload_Should_SetProject()
    {
      string command = DotNet.Build("test.csproj")
        .WithConfiguration("Debug")
        .WithNoRestore()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet build test.csproj --configuration Debug --no-restore");

      await Task.CompletedTask;
    }

    public static async Task NonExistentProject_Should_BuildValidCommand()
    {
      string commandString = DotNet.Build()
        .WithProject("nonexistent.csproj")
        .WithConfiguration("Debug")
        .WithNoRestore()
        .Build()
        .ToCommandString();

      commandString.ShouldBe("dotnet build nonexistent.csproj --configuration Debug --no-restore");

      await Task.CompletedTask;
    }

    public static async Task WithArtifactsPath_Should_IncludeArtifactsPath()
    {
      string command = DotNet.Build()
        .WithProject("test.csproj")
        .WithArtifactsPath("artifacts/owned")
        .WithNoLogo()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet build test.csproj --artifacts-path artifacts/owned --nologo");

      await Task.CompletedTask;
    }

    public static async Task RunfileCoverageShape_Should_IncludeArtifactsPathNoLogoAndVerbosity()
    {
      string command = DotNet.Build("entry.cs")
        .WithArtifactsPath("artifacts/owned")
        .WithNoLogo()
        .WithVerbosity("minimal")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet build entry.cs --artifacts-path artifacts/owned --verbosity minimal --nologo");

      await Task.CompletedTask;
    }

    public static async Task WithArtifactsPathBlank_Should_Throw()
    {
      DotNetBuildBuilder builder = DotNet.Build();

      Should.Throw<ArgumentException>(() => builder.WithArtifactsPath(""));
      Should.Throw<ArgumentException>(() => builder.WithArtifactsPath(" "));

      await Task.CompletedTask;
    }

    public static async Task WithTimeout_Should_LeaveTheCommandStringUnchanged()
    {
      string command = DotNet.Build()
        .WithTimeout(TimeSpan.FromSeconds(30))
        .WithTimeoutGracePeriod(TimeSpan.FromSeconds(1))
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet build");

      await Task.CompletedTask;
    }

    public static async Task WithTimeoutInvalid_Should_Throw()
    {
      Should.Throw<ArgumentOutOfRangeException>(() => DotNet.Build().WithTimeout(TimeSpan.Zero));
      Should.Throw<ArgumentOutOfRangeException>(() => DotNet.Build().WithTimeoutGracePeriod(TimeSpan.FromMilliseconds(-1)));

      await Task.CompletedTask;
    }

    public static async Task WithTimeout_Should_StopASlowBuild()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("dotnet", "build", "app.csproj")
          .Delays(TimeSpan.FromSeconds(5))
          .Returns("built");

        CommandOutput output = await DotNet.Build()
          .WithProject("app.csproj")
          .WithTimeout(TimeSpan.FromMilliseconds(200))
          .WithTimeoutGracePeriod(TimeSpan.Zero)
          .Build()
          .CaptureAsync();

        output.TimedOut.ShouldBeTrue();
        output.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
      }
    }
  }
}
