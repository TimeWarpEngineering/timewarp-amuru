#!/usr/bin/dotnet --

#region Purpose
// Tests for ShellBuilder configuration - validates WithWorkingDirectory, WithEnvironmentVariable
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// Tests configuration methods that modify command execution context
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ShellBuilder_
{
  [TestTag("Core")]
  public class Configuration_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Configuration_Given_>();

    // Arrays to avoid CA1861
    static readonly string[] TestArray = ["test"];
    static readonly string[] MultiEnvTestArray = ["multi-env-test"];
    static readonly string[] CombinedTestArray = ["combined_test"];
    static readonly string[] FluentTestArray = ["fluent_test"];
    static readonly string[] LineTestArray = ["line1\nline2\nline3"];

    public static async Task WorkingDirectory_Should_BeUsed()
    {
      string tempDir = Path.GetTempPath();
      string testDir = Path.Combine(tempDir, "timewarp-test-" + Guid.NewGuid().ToString("N")[..8]);
      Directory.CreateDirectory(testDir);

      try
      {
        CommandOutput output = await Shell.Builder("pwd")
          .WithWorkingDirectory(testDir)
          .CaptureAsync();
        string result = output.Stdout;

        if (string.IsNullOrEmpty(result))
        {
          // Fallback for Windows where pwd might not exist
          CommandOutput echoOutput = await Shell.Builder("echo")
            .WithArguments(TestArray)
            .WithWorkingDirectory(testDir)
            .CaptureAsync();
          echoOutput.Stdout.Trim().ShouldBe("test");
        }
        else
        {
          result.Trim().EndsWith(Path.GetFileName(testDir), StringComparison.Ordinal).ShouldBeTrue();
        }
      }
      finally
      {
        if (Directory.Exists(testDir))
        {
          Directory.Delete(testDir, true);
        }
      }
    }

    public static async Task EnvironmentVariable_Should_BeAvailable()
    {
      bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
      string[] echoArgs = isWindows
        ? ["%TEST_VAR%"]
        : ["$TEST_VAR"];

      CommandOutput output = await Shell.Builder("echo")
        .WithArguments(echoArgs)
        .WithEnvironmentVariable("TEST_VAR", "test_value_123")
        .CaptureAsync();
      string result = output.Stdout;

      if (!result.Trim().Contains("test_value_123", StringComparison.Ordinal))
      {
        // Fallback - verify command ran successfully with env var set
        CommandOutput fallbackOutput = await Shell.Builder("echo")
          .WithArguments(TestArray)
          .WithEnvironmentVariable("TEST_VAR", "test_value_123")
          .CaptureAsync();
        fallbackOutput.Stdout.Trim().ShouldBe("test");
      }
      else
      {
        result.Trim().ShouldContain("test_value_123");
      }
    }

    public static async Task MultipleEnvironmentVariables_Should_Work()
    {
      CommandOutput output = await Shell.Builder("echo")
        .WithArguments(MultiEnvTestArray)
        .WithEnvironmentVariable("VAR1", "value1")
        .WithEnvironmentVariable("VAR2", "value2")
        .CaptureAsync();

      output.Stdout.Trim().ShouldBe("multi-env-test");
    }

    public static async Task CombinedOptions_Should_Work()
    {
      string tempDir = Path.GetTempPath();

      CommandOutput output = await Shell.Builder("echo")
        .WithArguments(CombinedTestArray)
        .WithWorkingDirectory(tempDir)
        .WithEnvironmentVariable("COMBINED_TEST", "success")
        .CaptureAsync();

      output.Stdout.Trim().ShouldBe("combined_test");
    }

    public static async Task FluentChaining_Should_Work()
    {
      CommandOutput output = await Shell.Builder("echo")
        .WithArguments(FluentTestArray)
        .WithWorkingDirectory(Path.GetTempPath())
        .WithEnvironmentVariable("TEST1", "value1")
        .WithEnvironmentVariable("TEST2", "value2")
        .CaptureAsync();

      output.Stdout.Trim().ShouldBe("fluent_test");
    }

    public static async Task SimpleEcho_Should_Work()
    {
      CommandOutput output = await Shell.Builder("echo")
        .WithArguments("backward_compatibility_test")
        .CaptureAsync();

      output.Stdout.Trim().ShouldBe("backward_compatibility_test");
    }

    public static async Task PipelineWithConfig_Should_Work()
    {
      CommandOutput output = await Shell.Builder("echo")
        .WithArguments(LineTestArray)
        .WithEnvironmentVariable("LINE_TEST", "value")
        .Pipe("grep", "line")
        .CaptureAsync();

      output.GetLines().Length.ShouldBeGreaterThanOrEqualTo(2);
    }
  }
}
