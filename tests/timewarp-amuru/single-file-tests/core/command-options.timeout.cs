#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for CommandOptions timeout validation and snapshot copying.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// With* methods return a copy. A later WithWorkingDirectory must keep the timeout.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace CommandOptions_
{
  [TestTag("Core")]
  public class WithTimeout_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<WithTimeout_Given_>();

    public static Task RejectsNonPositiveTimeout_Should_Throw()
    {
      Should.Throw<ArgumentOutOfRangeException>(() => new CommandOptions().WithTimeout(TimeSpan.Zero));
      Should.Throw<ArgumentOutOfRangeException>(() => new CommandOptions().WithTimeout(TimeSpan.FromMilliseconds(-1)));
      return Task.CompletedTask;
    }

    public static Task Grace_Should_AllowZeroAndRejectNegative()
    {
      Should.Throw<ArgumentOutOfRangeException>(() =>
        new CommandOptions().WithTimeoutGracePeriod(TimeSpan.FromMilliseconds(-1)));

      CommandOptions options = new CommandOptions().WithTimeoutGracePeriod(TimeSpan.Zero);

      options.TimeoutGracePeriod.ShouldBe(TimeSpan.Zero);
      return Task.CompletedTask;
    }

    public static Task DefaultGrace_Should_BeFiveSeconds()
    {
      CommandOptions options = new();

      options.Timeout.ShouldBeNull();
      options.TimeoutGracePeriod.ShouldBe(CommandOptions.DefaultTimeoutGracePeriod);
      options.TimeoutGracePeriod.ShouldBe(TimeSpan.FromSeconds(5));
      return Task.CompletedTask;
    }

    public static Task Copy_Should_KeepTimeoutWhenOtherOptionsChange()
    {
      string directory = Directory.CreateTempSubdirectory("amuru-timeout-options-").FullName;
      try
      {
        CommandOptions options = new CommandOptions()
          .WithTimeout(TimeSpan.FromSeconds(2))
          .WithTimeoutGracePeriod(TimeSpan.FromSeconds(1))
          .WithWorkingDirectory(directory);

        options.Timeout.ShouldBe(TimeSpan.FromSeconds(2));
        options.TimeoutGracePeriod.ShouldBe(TimeSpan.FromSeconds(1));
        options.WorkingDirectory.ShouldBe(directory);
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }

      return Task.CompletedTask;
    }
  }
}
