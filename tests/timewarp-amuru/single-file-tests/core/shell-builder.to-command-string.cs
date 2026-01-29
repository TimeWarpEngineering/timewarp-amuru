#!/usr/bin/dotnet --

#region Purpose
// Tests for ShellBuilder.Build().ToCommandString() - validates command string formatting
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// ToCommandString returns the formatted command string without executing
// Note: Environment variables and working directory don't appear in the string
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ShellBuilder_
{
  [TestTag("Core")]
  public class ToCommandString_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<ToCommandString_Given_>();

    public static async Task BasicArgs_Should_ReturnFormattedString()
    {
      string command = Shell.Builder("echo")
        .WithArguments("Hello", "World")
        .Build()
        .ToCommandString();

      command.ShouldBe("echo Hello World");
      await Task.CompletedTask;
    }

    public static async Task MultipleWithArguments_Should_ConcatenateAll()
    {
      string command = Shell.Builder("echo")
        .WithArguments("arg1")
        .WithArguments("arg2", "arg3")
        .Build()
        .ToCommandString();

      command.ShouldBe("echo arg1 arg2 arg3");
      await Task.CompletedTask;
    }

    public static async Task EnvironmentVariable_Should_NotAppearInString()
    {
      string command = Shell.Builder("printenv")
        .WithEnvironmentVariable("TEST_VAR", "test_value")
        .WithArguments("TEST_VAR")
        .Build()
        .ToCommandString();

      command.ShouldBe("printenv TEST_VAR");
      await Task.CompletedTask;
    }

    public static async Task WorkingDirectory_Should_NotAppearInString()
    {
      string command = Shell.Builder("pwd")
        .WithWorkingDirectory("/tmp")
        .Build()
        .ToCommandString();

      command.ShouldBe("pwd ");
      await Task.CompletedTask;
    }

    public static async Task ComplexArguments_Should_EscapeCorrectly()
    {
      string command = Shell.Builder("git")
        .WithArguments("log", "--oneline", "--author=\"John Doe\"", "--grep=fix")
        .Build()
        .ToCommandString();

      command.ShouldBe("git log --oneline \"--author=\\\"John Doe\\\"\" --grep=fix");
      await Task.CompletedTask;
    }

    public static async Task NoValidation_Should_NotAffectString()
    {
      string command = Shell.Builder("false")
        .WithNoValidation()
        .Build()
        .ToCommandString();

      command.ShouldBe("false ");
      await Task.CompletedTask;
    }
  }
}
