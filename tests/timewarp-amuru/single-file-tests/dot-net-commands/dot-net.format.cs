#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for DotNet.Format() - the argument string each option produces. Task 154.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// Build() returns a CommandResult that has not run, so ToCommandString() checks argv without starting dotnet.
// Repeatable --exclude switches are asserted in call order.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class Format_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Format_Given_>();

    public static async Task NoOptions_Should_BuildBasicCommand()
    {
      string command = DotNet.Format().Build().ToCommandString();

      command.ShouldBe("dotnet format");

      await Task.CompletedTask;
    }

    public static async Task ProjectOverload_Should_IncludeProjectPath()
    {
      string command = DotNet.Format("timewarp-nuru.slnx").Build().ToCommandString();

      command.ShouldBe("dotnet format timewarp-nuru.slnx");

      await Task.CompletedTask;
    }

    public static async Task NuruCheckShape_Should_EmitSeverityExcludeAndVerify()
    {
      string command = DotNet.Format("timewarp-nuru.slnx")
        .WithSeverity("warn")
        .WithExclude("**/benchmarks/**")
        .WithVerifyNoChanges()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet format timewarp-nuru.slnx --severity warn --exclude **/benchmarks/** --verify-no-changes");

      await Task.CompletedTask;
    }

    public static async Task RepeatedExcludes_Should_KeepCallOrder()
    {
      string command = DotNet.Format("app.slnx")
        .WithExclude("**/benchmarks/**")
        .WithExclude("artifacts")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet format app.slnx --exclude **/benchmarks/** --exclude artifacts");

      await Task.CompletedTask;
    }

    public static async Task SeverityTwice_Should_KeepTheLastLevel()
    {
      string command = DotNet.Format()
        .WithSeverity("info")
        .WithSeverity("error")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet format --severity error");

      await Task.CompletedTask;
    }

    public static async Task WorkingDirectory_Should_NotAppearInCommandString()
    {
      string command = DotNet.Format("app.slnx")
        .WithWorkingDirectory("/tmp")
        .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
        .WithNoValidation()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet format app.slnx");

      await Task.CompletedTask;
    }

    public static async Task BlankArguments_Should_Throw()
    {
      Should.Throw<ArgumentException>(() => DotNet.Format().WithProject(" "));
      Should.Throw<ArgumentException>(() => DotNet.Format().WithSeverity(""));
      Should.Throw<ArgumentException>(() => DotNet.Format().WithExclude(" "));

      await Task.CompletedTask;
    }
  }
}
