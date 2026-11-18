#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for DotNet.Test() - validates the fluent API for running .NET tests
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: DotNet (the static class providing command builders)
// Action: Test (the method being tested)
// Tests verify command string generation for various test configuration options.
// WithArguments appends tokens verbatim and does not insert --.
// WithTimeout is an execution limit, so the command string stays unchanged and a mock proves the limit.
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
        .WithFramework("net11.0")
        .WithNoRestore()
        .WithFilter("Category=Unit")
        .WithLogger("console")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet test test.csproj --configuration Debug --framework net11.0 --filter Category=Unit --logger console --no-restore");

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
        .WithCollect("XPlat Code Coverage")
        .WithResultsDirectory("TestResults")
        .WithProperty("Platform", "AnyCPU")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet test test.csproj --configuration Release --arch x64 --os linux --verbosity minimal --filter TestCategory=Integration --results-directory TestResults --logger trx --logger html --no-restore --no-build --blame --collect \"XPlat Code Coverage\" --property:Platform=AnyCPU");

      await Task.CompletedTask;
    }

    public static async Task WithCollectMissingName_Should_Throw()
    {
      DotNetTestBuilder builder = DotNet.Test();

      Should.Throw<ArgumentException>(() => builder.WithCollect(""));
      Should.Throw<ArgumentException>(() => builder.WithCollect(" "));

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

    public static async Task WithArguments_Should_AppendExtensionOptions()
    {
      string command = DotNet.Test()
        .WithProject("tests.csproj")
        .WithConfiguration("Release")
        .WithResultsDirectory("artifacts/coverage")
        .WithArguments(
          "--coverage",
          "--coverage-output-format",
          "cobertura",
          "--coverage-output",
          "coverage.cobertura.xml")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet test tests.csproj --configuration Release --results-directory artifacts/coverage --coverage --coverage-output-format cobertura --coverage-output coverage.cobertura.xml");

      await Task.CompletedTask;
    }

    public static async Task WithArguments_Should_AppendAcrossCalls()
    {
      string command = DotNet.Test()
        .WithArguments("--coverage")
        .WithArguments("--coverage-output", "out.xml")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet test --coverage --coverage-output out.xml");

      await Task.CompletedTask;
    }

    public static async Task WithArgumentsNull_Should_LeaveTheCommandUnchanged()
    {
      string command = DotNet.Test()
        .WithArguments(null)
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet test");

      await Task.CompletedTask;
    }

    public static async Task WithArgumentsBlank_Should_Throw()
    {
      DotNetTestBuilder builder = DotNet.Test();

      Should.Throw<ArgumentException>(() => builder.WithArguments(""));
      Should.Throw<ArgumentException>(() => builder.WithArguments(" "));
      Should.Throw<ArgumentException>(() => builder.WithArguments("--coverage", ""));

      await Task.CompletedTask;
    }

    public static async Task WithArtifactsPath_Should_IncludeArtifactsPath()
    {
      string command = DotNet.Test()
        .WithProject("tests.csproj")
        .WithArtifactsPath("artifacts/owned")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet test tests.csproj --artifacts-path artifacts/owned");

      await Task.CompletedTask;
    }

    public static async Task WithArtifactsPathBlank_Should_Throw()
    {
      DotNetTestBuilder builder = DotNet.Test();

      Should.Throw<ArgumentException>(() => builder.WithArtifactsPath(""));
      Should.Throw<ArgumentException>(() => builder.WithArtifactsPath(" "));

      await Task.CompletedTask;
    }

    public static async Task WithTimeout_Should_LeaveTheCommandStringUnchanged()
    {
      string command = DotNet.Test()
        .WithTimeout(TimeSpan.FromMinutes(20))
        .WithTimeoutGracePeriod(TimeSpan.Zero)
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet test");

      await Task.CompletedTask;
    }

    public static async Task WithTimeoutInvalid_Should_Throw()
    {
      Should.Throw<ArgumentOutOfRangeException>(() => DotNet.Test().WithTimeout(TimeSpan.Zero));
      Should.Throw<ArgumentOutOfRangeException>(() => DotNet.Test().WithTimeoutGracePeriod(TimeSpan.FromMilliseconds(-1)));

      await Task.CompletedTask;
    }

    public static async Task WithTimeout_Should_StopASlowTest()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("dotnet", "test", "tests.csproj")
          .Delays(TimeSpan.FromSeconds(5))
          .Returns("tested");

        CommandOutput output = await DotNet.Test()
          .WithProject("tests.csproj")
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
