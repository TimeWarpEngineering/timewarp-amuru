#!/usr/bin/dotnet --

#region Purpose
// Tests that CommandOptions.With* copies EnvironmentVariables instead of aliasing the dictionary
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// Mutating the source options dictionary must not change a derived instance
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace CommandOptions_
{
  [TestTag("Core")]
  public class EnvironmentVariables_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<EnvironmentVariables_Given_>();

    public static Task WithWorkingDirectory_Should_CopyEnvironmentVariables()
    {
      CommandOptions original = new CommandOptions().WithEnvironmentVariable("K", "V");
      CommandOptions derived = original.WithWorkingDirectory(Path.GetTempPath());

      original.EnvironmentVariables!["K"] = "mutated";

      derived.EnvironmentVariables!["K"].ShouldBe("V");
      derived.WorkingDirectory.ShouldBe(Path.GetTempPath());
      return Task.CompletedTask;
    }

    public static Task WithNoValidation_Should_CopyEnvironmentVariables()
    {
      CommandOptions original = new CommandOptions().WithEnvironmentVariable("K", "V");
      CommandOptions derived = original.WithNoValidation();

      original.EnvironmentVariables!["K"] = "mutated";

      derived.EnvironmentVariables!["K"].ShouldBe("V");
      return Task.CompletedTask;
    }

    public static Task WithZeroExitCodeValidation_Should_CopyEnvironmentVariables()
    {
      CommandOptions original = new CommandOptions().WithEnvironmentVariable("K", "V");
      CommandOptions derived = original.WithZeroExitCodeValidation();

      original.EnvironmentVariables!["K"] = "mutated";

      derived.EnvironmentVariables!["K"].ShouldBe("V");
      return Task.CompletedTask;
    }
  }
}
