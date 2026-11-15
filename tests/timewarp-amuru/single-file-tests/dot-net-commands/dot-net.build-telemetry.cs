#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for the build.command, build.configuration, and build.publish_aot tags that the
// DotNet build, publish, pack, and test builders set on Activity.Current. Task 144.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// Each test starts its own Activity so Activity.Current is that activity in the test's async flow,
// calls Build() (which tags but does not run dotnet), and reads the tags back.
// Runfile and project-file AOT detection use files in a fresh temp directory.
// The no-secrets test asserts only the three build.* keys are added and none carries a path or argv.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class BuildTelemetry_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<BuildTelemetry_Given_>();

    public static async Task BuildWithReleaseConfiguration_Should_TagBuildNotAot()
    {
      using Activity activity = new(ActivityName);
      activity.Start();

      DotNet.Build("app.csproj").WithConfiguration("Release").Build();

      activity.GetTagItem("build.command").ShouldBe("build");
      activity.GetTagItem("build.configuration").ShouldBe("Release");
      activity.GetTagItem("build.publish_aot").ShouldBe(false);

      await Task.CompletedTask;
    }

    public static async Task Test_Should_TagTestCommand()
    {
      using Activity activity = new(ActivityName);
      activity.Start();

      DotNet.Test().WithConfiguration("Debug").Build();

      activity.GetTagItem("build.command").ShouldBe("test");
      activity.GetTagItem("build.configuration").ShouldBe("Debug");
      activity.GetTagItem("build.publish_aot").ShouldBe(false);

      await Task.CompletedTask;
    }

    public static async Task Pack_Should_TagPackCommand()
    {
      using Activity activity = new(ActivityName);
      activity.Start();

      DotNet.Pack().WithConfiguration("Release").Build();

      activity.GetTagItem("build.command").ShouldBe("pack");
      activity.GetTagItem("build.configuration").ShouldBe("Release");
      activity.GetTagItem("build.publish_aot").ShouldBe(false);

      await Task.CompletedTask;
    }

    public static async Task NoConfiguration_Should_OmitConfigurationTag()
    {
      using Activity activity = new(ActivityName);
      activity.Start();

      DotNet.Build().Build();

      activity.GetTagItem("build.command").ShouldBe("build");
      activity.GetTagItem("build.configuration").ShouldBeNull();

      await Task.CompletedTask;
    }

    public static async Task ConfigurationProperty_Should_TagConfiguration()
    {
      using Activity activity = new(ActivityName);
      activity.Start();

      DotNet.Build().WithProperty("Configuration", "Release").Build();

      activity.GetTagItem("build.configuration").ShouldBe("Release");

      await Task.CompletedTask;
    }

    public static async Task PublishWithPublishAotProperty_Should_TagAot()
    {
      using Activity activity = new(ActivityName);
      activity.Start();

      DotNet.Publish("app.csproj")
        .WithConfiguration("Release")
        .WithProperty("PublishAot", "true")
        .Build();

      activity.GetTagItem("build.command").ShouldBe("publish");
      activity.GetTagItem("build.configuration").ShouldBe("Release");
      activity.GetTagItem("build.publish_aot").ShouldBe(true);

      await Task.CompletedTask;
    }

    public static async Task BuildWithPublishAotProperty_Should_NotTagAot()
    {
      using Activity activity = new(ActivityName);
      activity.Start();

      DotNet.Build().WithProperty("PublishAot", "true").Build();

      activity.GetTagItem("build.publish_aot").ShouldBe(false);

      await Task.CompletedTask;
    }

    public static async Task PublishOfMissingProjectFile_Should_NotTagAot()
    {
      using Activity activity = new(ActivityName);
      activity.Start();

      DotNet.Publish("missing-project.csproj").WithConfiguration("Release").Build();

      activity.GetTagItem("build.command").ShouldBe("publish");
      activity.GetTagItem("build.publish_aot").ShouldBe(false);

      await Task.CompletedTask;
    }

    public static async Task PublishOfRunfile_Should_TagAot()
    {
      string directory = CreateTempDirectory();
      try
      {
        await File.WriteAllTextAsync(Path.Combine(directory, "dev.cs"), "#:package TimeWarp.Nuru\nreturn 0;\n");
        using Activity activity = new(ActivityName);
        activity.Start();

        DotNet.Publish("dev.cs").WithWorkingDirectory(directory).WithOutput("./bin").Build();

        activity.GetTagItem("build.command").ShouldBe("publish");
        activity.GetTagItem("build.publish_aot").ShouldBe(true);
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }

      await Task.CompletedTask;
    }

    public static async Task PublishOfRunfileWithAotOff_Should_NotTagAot()
    {
      string directory = CreateTempDirectory();
      try
      {
        string runfile = Path.Combine(directory, "tool.cs");
        await File.WriteAllTextAsync(runfile, "#:property PublishAot=false\nreturn 0;\n");
        using Activity activity = new(ActivityName);
        activity.Start();

        DotNet.Publish(runfile).Build();

        activity.GetTagItem("build.publish_aot").ShouldBe(false);
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }

      await Task.CompletedTask;
    }

    public static async Task PublishAotPropertyFalseOnRunfile_Should_NotTagAot()
    {
      using Activity activity = new(ActivityName);
      activity.Start();

      DotNet.Publish("dev.cs").WithProperty("PublishAot", "false").Build();

      activity.GetTagItem("build.publish_aot").ShouldBe(false);

      await Task.CompletedTask;
    }

    public static async Task PublishOfAotProjectFile_Should_TagAot()
    {
      string directory = CreateTempDirectory();
      try
      {
        string project = Path.Combine(directory, "app.csproj");
        await File.WriteAllTextAsync
        (
          project,
          "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <PublishAot>true</PublishAot>\n  </PropertyGroup>\n</Project>\n"
        );
        using Activity activity = new(ActivityName);
        activity.Start();

        DotNet.Publish(project).WithConfiguration("Release").Build();

        activity.GetTagItem("build.publish_aot").ShouldBe(true);
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }

      await Task.CompletedTask;
    }

    public static async Task Tags_Should_ExposeOnlyBuildKeysWithoutPathsOrArguments()
    {
      using Activity activity = new(ActivityName);
      activity.Start();

      DotNet.Publish("/secret/path/app.csproj")
        .WithConfiguration("Release")
        .WithProperty("PublishAot", "true")
        .WithProperty("ApiKey", "do-not-export")
        .WithSource("https://user:token@example.invalid/index.json")
        .Build();

      string[] keys = [.. activity.TagObjects.Select(tag => tag.Key)];
      keys.ShouldBe(["build.command", "build.configuration", "build.publish_aot"], ignoreOrder: true);

      foreach (KeyValuePair<string, object?> tag in activity.TagObjects)
      {
        string value = tag.Value?.ToString() ?? string.Empty;
        value.ShouldNotContain("secret");
        value.ShouldNotContain("do-not-export");
        value.ShouldNotContain("token");
      }

      await Task.CompletedTask;
    }

    public static async Task NoCurrentActivity_Should_NotThrow()
    {
      Activity? previous = Activity.Current;
      Activity.Current = null;
      try
      {
        string command = DotNet.Publish("dev.cs").Build().ToCommandString();
        command.ShouldBe("dotnet publish dev.cs");
      }
      finally
      {
        Activity.Current = previous;
      }

      await Task.CompletedTask;
    }

    private const string ActivityName = "build-telemetry-test";

    private static string CreateTempDirectory()
    {
      string directory = Path.Combine(Path.GetTempPath(), "amuru-build-telemetry-" + Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(directory);
      return directory;
    }
  }
}
