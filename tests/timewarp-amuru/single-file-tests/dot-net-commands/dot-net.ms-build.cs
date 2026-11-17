#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for DotNet.MsBuild() - the argument string each option produces and the build span tags. Task 151.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// Build() returns a CommandResult that has not run, so ToCommandString() checks argv without starting dotnet.
// Repeatable -t:, -p:, and -get* switches are asserted in call order.
// Telemetry tests start their own Activity, call Build(), and read the build.* tags back.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class MsBuild_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<MsBuild_Given_>();

    public static async Task NoOptions_Should_BuildBasicCommand()
    {
      string command = DotNet.MsBuild().Build().ToCommandString();

      command.ShouldBe("dotnet msbuild");

      await Task.CompletedTask;
    }

    public static async Task ProjectOverload_Should_IncludeProjectPath()
    {
      string command = DotNet.MsBuild("MyApp.slnx").Build().ToCommandString();

      command.ShouldBe("dotnet msbuild MyApp.slnx");

      await Task.CompletedTask;
    }

    public static async Task RepeatedTargets_Should_KeepCallOrder()
    {
      string command = DotNet.MsBuild("app.csproj")
        .WithTarget("Clean")
        .WithTarget("Build")
        .WithTarget("Pack")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet msbuild app.csproj -t:Clean -t:Build -t:Pack");

      await Task.CompletedTask;
    }

    public static async Task RepeatedProperties_Should_KeepInsertionOrder()
    {
      string command = DotNet.MsBuild("app.csproj")
        .WithProperty("Zeta", "1")
        .WithProperty("Alpha", "2")
        .WithProperty("Mid", "3")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet msbuild app.csproj -p:Zeta=1 -p:Alpha=2 -p:Mid=3");

      await Task.CompletedTask;
    }

    public static async Task SamePropertyTwice_Should_ReplaceValueInPlace()
    {
      string command = DotNet.MsBuild()
        .WithProperty("Configuration", "Debug")
        .WithProperty("Version", "1.0.0")
        .WithProperty("Configuration", "Release")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet msbuild -p:Configuration=Release -p:Version=1.0.0");

      await Task.CompletedTask;
    }

    public static async Task GetQueries_Should_EmitEachSwitch()
    {
      string command = DotNet.MsBuild("app.csproj")
        .WithGetProperty("Version")
        .WithGetProperty("TargetFramework")
        .WithGetItem("Compile")
        .WithGetTargetResult("Build")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet msbuild app.csproj -getProperty:Version -getProperty:TargetFramework -getItem:Compile -getTargetResult:Build");

      await Task.CompletedTask;
    }

    public static async Task AllOptions_Should_BuildCompleteCommandInFixedOrder()
    {
      string command = DotNet.MsBuild()
        .WithNoLogo()
        .WithBinaryLog("artifacts/build.binlog")
        .WithRestore()
        .WithVerbosity("minimal")
        .WithProperty("Configuration", "Release")
        .WithTarget("Publish")
        .WithProject("app.csproj")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet msbuild app.csproj -t:Publish -p:Configuration=Release -verbosity:minimal -restore -nologo -bl:artifacts/build.binlog");

      await Task.CompletedTask;
    }

    public static async Task BinaryLogWithoutPath_Should_EmitBareSwitch()
    {
      string command = DotNet.MsBuild().WithBinaryLog().Build().ToCommandString();

      command.ShouldBe("dotnet msbuild -bl");

      await Task.CompletedTask;
    }

    public static async Task WorkingDirectoryAndEnvVars_Should_NotAppearInCommandString()
    {
      string command = DotNet.MsBuild("app.csproj")
        .WithWorkingDirectory("/tmp")
        .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
        .WithNoValidation()
        .WithTarget("Build")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet msbuild app.csproj -t:Build");

      await Task.CompletedTask;
    }

    public static async Task BlankTarget_Should_Throw()
    {
      Should.Throw<ArgumentException>(() => DotNet.MsBuild().WithTarget(" "));

      await Task.CompletedTask;
    }

    public static async Task Build_Should_TagMsBuildCommandAndConfigurationProperty()
    {
      using Activity activity = new(ActivityName);
      activity.Start();

      DotNet.MsBuild("app.csproj")
        .WithTarget("Build")
        .WithProperty("Configuration", "Release")
        .WithProperty("PublishAot", "true")
        .Build();

      activity.GetTagItem("build.command").ShouldBe("msbuild");
      activity.GetTagItem("build.configuration").ShouldBe("Release");
      activity.GetTagItem("build.publish_aot").ShouldBe(false);

      await Task.CompletedTask;
    }

    public static async Task Tags_Should_NotCarryPathsOrPropertyValues()
    {
      using Activity activity = new(ActivityName);
      activity.Start();

      DotNet.MsBuild("/secret/path/app.csproj")
        .WithProperty("ApiKey", "do-not-export")
        .WithBinaryLog("/secret/path/build.binlog")
        .Build();

      string[] keys = [.. activity.TagObjects.Select(tag => tag.Key)];
      keys.ShouldBe(["build.command", "build.publish_aot"], ignoreOrder: true);

      foreach (KeyValuePair<string, object?> tag in activity.TagObjects)
      {
        string value = tag.Value?.ToString() ?? string.Empty;
        value.ShouldNotContain("secret");
        value.ShouldNotContain("do-not-export");
      }

      await Task.CompletedTask;
    }

    private const string ActivityName = "msbuild-telemetry-test";
  }
}
