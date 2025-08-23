#!/usr/bin/dotnet run

await RunTests<GhqCommandTests>();

internal sealed class GhqCommandTests
{
  public static async Task TestBasicGhqBuilderCreation()
  {
    GhqBuilder ghqBuilder = Ghq.Builder();
    
    AssertTrue(
      ghqBuilder != null,
      "Ghq.Builder() should create builder successfully"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestGhqGetCommand()
  {
    string command = Ghq.Builder()
      .Get("github.com/TimeWarpEngineering/timewarp-cli")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "ghq get github.com/TimeWarpEngineering/timewarp-cli",
      $"Expected 'ghq get github.com/TimeWarpEngineering/timewarp-cli', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestGhqCloneCommand()
  {
    string command = Ghq.Builder()
      .Clone("github.com/user/repo")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "ghq get github.com/user/repo",
      $"Expected 'ghq get github.com/user/repo', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestGhqGetWithOptions()
  {
    string command = Ghq.Builder()
      .Get("github.com/user/repo")
      .WithShallow()
      .WithBranch("develop")
      .WithUpdate()
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "ghq get --shallow --branch develop --update github.com/user/repo",
      $"Expected 'ghq get --shallow --branch develop --update github.com/user/repo', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestGhqListCommand()
  {
    string command = Ghq.Builder()
      .List()
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "ghq list",
      $"Expected 'ghq list', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestGhqListWithOptions()
  {
    string command = Ghq.Builder()
      .List()
      .WithFullPath()
      .WithExact()
      .FilterByVcs("git")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "ghq list --full-path --exact --vcs git",
      $"Expected 'ghq list --full-path --exact --vcs git', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestGhqRemoveCommand()
  {
    string command = Ghq.Builder()
      .Remove("github.com/old/repo")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "ghq rm github.com/old/repo",
      $"Expected 'ghq rm github.com/old/repo', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestGhqRmCommandWithDryRun()
  {
    string command = Ghq.Builder()
      .Rm("github.com/old/repo")
      .WithDryRun()
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "ghq rm --dry-run github.com/old/repo",
      $"Expected 'ghq rm --dry-run github.com/old/repo', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestGhqRootCommand()
  {
    string command = Ghq.Builder()
      .Root()
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "ghq root",
      $"Expected 'ghq root', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestGhqRootWithAllOption()
  {
    string command = Ghq.Builder()
      .Root()
      .WithAll()
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "ghq root --all",
      $"Expected 'ghq root --all', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestGhqCreateCommand()
  {
    string command = Ghq.Builder()
      .Create("github.com/user/new-repo")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "ghq create github.com/user/new-repo",
      $"Expected 'ghq create github.com/user/new-repo', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestGhqWithWorkingDirectoryAndEnvironment()
  {
    // Note: Working directory and environment variables don't appear in ToCommandString()
    string command = Ghq.Builder()
      .WithWorkingDirectory("/tmp")
      .WithEnvironmentVariable("GHQ_ROOT", "/tmp/ghq")
      .List()
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "ghq list",
      $"Expected 'ghq list', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestGhqGetWithAdvancedOptions()
  {
    string command = Ghq.Builder()
      .Get("github.com/example/repo")
      .WithLook()
      .WithParallel()
      .WithSilent()
      .WithNoRecursive()
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "ghq get --look --parallel --silent --no-recursive github.com/example/repo",
      $"Expected 'ghq get --look --parallel --silent --no-recursive github.com/example/repo', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestGhqGetWithVcsBackend()
  {
    string command = Ghq.Builder()
      .Get("gitlab.com/user/project")
      .WithVcs("gitlab")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "ghq get --vcs gitlab gitlab.com/user/project",
      $"Expected 'ghq get --vcs gitlab gitlab.com/user/project', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestGhqListWithUniqueOption()
  {
    string command = Ghq.Builder()
      .List()
      .WithUnique()
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "ghq list --unique",
      $"Expected 'ghq list --unique', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestGhqGetWithBareRepository()
  {
    string command = Ghq.Builder()
      .Get("github.com/user/bare-repo")
      .WithBare()
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "ghq get --bare github.com/user/bare-repo",
      $"Expected 'ghq get --bare github.com/user/bare-repo', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestGhqPipeToExtension()
  {
    string command = Ghq.Builder()
      .List()
      .PipeTo("grep", "timewarp")
      .ToCommandString();
    
    AssertTrue(
      command == "grep timewarp",
      $"Expected 'grep timewarp', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestGhqCommandBuilds()
  {
    // This tests that the command builds correctly even if ghq might not be installed
    string command = Ghq.Builder()
      .List()
      .WithFullPath()
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "ghq list --full-path",
      $"Expected 'ghq list --full-path', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestGhqRootExecution()
  {
    // Test command string generation for root command
    string command = Ghq.Builder()
      .Root()
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "ghq root",
      $"Expected 'ghq root', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestGhqListExecution()
  {
    // Test command string generation for list command
    string command = Ghq.Builder()
      .List()
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "ghq list",
      $"Expected 'ghq list', got '{command}'"
    );
    
    await Task.CompletedTask;
  }
}