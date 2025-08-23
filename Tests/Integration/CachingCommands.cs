#!/usr/bin/dotnet run

await RunTests<CachingCommandsTests>();

internal sealed class CachingCommandsTests
{
  // Static arrays to avoid CA1861
  static readonly string[] BashExitArgs = { "-c", "exit 1" };
  public static async Task TestBasicCachingExecutedOnlyOnce()
  {
    int executionCount = 0;
    string testFile = Path.GetTempFileName();
    
    try
    {
      // Use a command that appends to a file to track executions
      CommandResult cachedCmd = Shell.Builder("bash").WithArguments("-c", $"echo 'execution' >> {testFile}").Build().Cached();
      
      // Execute multiple times
      await cachedCmd.ExecuteAsync();
      await cachedCmd.ExecuteAsync();
      await cachedCmd.ExecuteAsync();
      
      // Count lines in file (should be 1 if cached properly)
      string[] lines = await File.ReadAllLinesAsync(testFile);
      executionCount = lines.Length;
      
      AssertTrue(
        executionCount == 1,
        $"Cached command should execute only once, got {executionCount} executions"
      );
    }
    finally
    {
      if (File.Exists(testFile)) File.Delete(testFile);
    }
  }

  public static async Task TestGetStringAsyncCaching()
  {
    // Use a command that returns current time
    CommandResult cachedCmd = Shell.Builder("date").WithArguments("+%N").Build().Cached(); // Nanoseconds for precision
    
    string result1 = await cachedCmd.GetStringAsync();
    await Task.Delay(10); // Small delay to ensure time would change
    string result2 = await cachedCmd.GetStringAsync();
    string result3 = await cachedCmd.GetStringAsync();
    
    AssertTrue(
      result1 == result2 && result2 == result3 && !string.IsNullOrEmpty(result1),
      $"GetStringAsync should return cached result - R1: {result1.Trim()}, R2: {result2.Trim()}, R3: {result3.Trim()}"
    );
  }

  public static async Task TestGetLinesAsyncCaching()
  {
    CommandResult cachedCmd = Shell.Builder("echo").WithArguments("line1\nline2\nline3").Build().Cached();
    
    string[] lines1 = await cachedCmd.GetLinesAsync();
    string[] lines2 = await cachedCmd.GetLinesAsync();
    string[] lines3 = await cachedCmd.GetLinesAsync();
    
    AssertTrue(
      lines1.Length == 3 && 
      lines1.SequenceEqual(lines2) && 
      lines2.SequenceEqual(lines3),
      "GetLinesAsync should return cached result"
    );
  }

  public static async Task TestNonCachedCommandsExecuteFreshEachTime()
  {
    CommandResult nonCachedCmd = Shell.Builder("date").WithArguments("+%N").Build(); // No .Cached() call
    
    string result1 = await nonCachedCmd.GetStringAsync();
    await Task.Delay(10); // Small delay
    string result2 = await nonCachedCmd.GetStringAsync();
    
    AssertTrue(
      result1 != result2,
      $"Non-cached commands should execute fresh each time, got identical: {result1.Trim()}"
    );
  }

  public static async Task TestPipelineCaching()
  {
    string testData = "apple\nbanana\napricot\navocado";
    CommandResult cachedPipeline = Shell.Builder("echo").WithArguments(testData).Build()
      .Pipe("grep", "a")
      .Pipe("sort")
      .Cached();
    
    string[] result1 = await cachedPipeline.GetLinesAsync();
    string[] result2 = await cachedPipeline.GetLinesAsync();
    
    AssertTrue(
      result1.Length == 4 && result1.SequenceEqual(result2),
      "Pipeline caching should work correctly"
    );
  }

  public static async Task TestIntermediatePipelineCaching()
  {
    // Cache an expensive first operation
    CommandResult cachedFind = Shell.Builder("echo").WithArguments("file1.cs\nfile2.txt\nfile3.cs\nfile4.md").Build().Cached();
    
    // Different filters on the same cached base
    string[] csFiles = await cachedFind.Pipe("grep", ".cs").GetLinesAsync();
    string[] txtFiles = await cachedFind.Pipe("grep", ".txt").GetLinesAsync();
    string[] allFiles = await cachedFind.GetLinesAsync();
    
    AssertTrue(
      csFiles.Length == 2 && txtFiles.Length == 1 && allFiles.Length == 4,
      $"Intermediate pipeline caching should work - Expected 2 cs, 1 txt, 4 total. Got {csFiles.Length}, {txtFiles.Length}, {allFiles.Length}"
    );
  }

  public static async Task TestCacheIsolation()
  {
    CommandResult cmd1 = Shell.Builder("echo").WithArguments(Guid.NewGuid().ToString()).Build().Cached();
    CommandResult cmd2 = Shell.Builder("echo").WithArguments(Guid.NewGuid().ToString()).Build().Cached();
    
    string result1 = await cmd1.GetStringAsync();
    string result2 = await cmd2.GetStringAsync();
    
    AssertTrue(
      result1 != result2,
      "Each CommandResult instance should have isolated cache"
    );
  }

  public static async Task TestMixedCachingOnSameCommand()
  {
    CommandResult baseCmd = Shell.Builder("echo").WithArguments("test_data").Build();
    
    // First call cached
    string cachedResult1 = await baseCmd.Cached().GetStringAsync();
    string cachedResult2 = await baseCmd.Cached().GetStringAsync();
    
    // These should be equal (both cached)
    AssertTrue(
      cachedResult1 == cachedResult2,
      "Cached results should be equal"
    );
    
    // Now call without caching - should still work
    string freshResult = await baseCmd.GetStringAsync();
    
    AssertTrue(
      freshResult == cachedResult1,
      "Fresh result should match cached result for same command"
    );
  }

  public static async Task TestCachingWithCancellationToken()
  {
    using var cts = new CancellationTokenSource();
    CommandResult cachedCmd = Shell.Builder("echo").WithArguments("cancellation_test").Build().Cached();
    
    string result1 = await cachedCmd.GetStringAsync(cts.Token);
    string result2 = await cachedCmd.GetStringAsync(cts.Token);
    
    AssertTrue(
      result1 == result2 && result1.Trim() == "cancellation_test",
      "Caching should work with cancellation tokens"
    );
  }

  public static async Task TestCachingFailedCommands()
  {
    // Test caching of commands that fail due to non-zero exit code
    // Use a command that exists but returns non-zero exit
    CommandOptions noValidation = new CommandOptions().WithNoValidation();
    CommandResult cachedCmd = Shell.Builder("bash").WithArguments(BashExitArgs).WithNoValidation().Build().Cached();
    
    string result1 = await cachedCmd.GetStringAsync();
    string result2 = await cachedCmd.GetStringAsync();
    string[] lines1 = await cachedCmd.GetLinesAsync();
    
    AssertTrue(
      string.IsNullOrEmpty(result1) && string.IsNullOrEmpty(result2) && lines1.Length == 0,
      "Failed commands should cache empty results"
    );
  }
}