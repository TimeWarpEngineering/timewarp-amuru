#!/usr/bin/dotnet run

await RunTests<FzfCommandTests>();

internal sealed class FzfCommandTests
{
  public static async Task TestBasicFzfBuilderCreation()
  {
    FzfBuilder fzfBuilder = Fzf.Builder();
    
    AssertTrue(
      fzfBuilder != null,
      "Fzf.Builder() should create builder successfully"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfWithInputItems()
  {
    string command = Fzf.Builder()
      .FromInput("apple", "banana", "cherry")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf ",
      $"Expected 'fzf ', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfWithMultiSelect()
  {
    string command = Fzf.Builder()
      .WithMulti()
      .FromInput("item1", "item2", "item3")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf --multi",
      $"Expected 'fzf --multi', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfWithPreview()
  {
    string command = Fzf.Builder()
      .WithPreview("echo {}")
      .FromInput("file1.txt", "file2.txt", "file3.txt")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf \"--preview=echo {}\"",
      $"Expected 'fzf \"--preview=echo {{}}\"', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfWithHeightAndLayoutOptions()
  {
    string command = Fzf.Builder()
      .WithHeightPercent(50)
      .WithLayout("reverse")
      .WithBorder("rounded")
      .FromInput("option1", "option2", "option3")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf --height=50% --layout=reverse --border=rounded",
      $"Expected 'fzf --height=50% --layout=reverse --border=rounded', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfWithSearchOptions()
  {
    string command = Fzf.Builder()
      .WithExact()
      .WithCaseInsensitive()
      .WithScheme("path")
      .FromInput("path/to/file1", "path/to/file2", "another/path/file3")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf --exact -i --scheme=path",
      $"Expected 'fzf --exact -i --scheme=path', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfWithInterfaceOptions()
  {
    string command = Fzf.Builder()
      .WithCycle()
      .WithNoMouse()
      .WithScrollOff(3)
      .WithPrompt("Select: ")
      .FromInput("choice1", "choice2", "choice3")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf --cycle --no-mouse --scroll-off=3 \"--prompt=Select: \"",
      $"Expected 'fzf --cycle --no-mouse --scroll-off=3 \"--prompt=Select: \"', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfWithDisplayOptions()
  {
    string command = Fzf.Builder()
      .WithAnsi()
      .WithColor("dark")
      .WithTabstop(4)
      .WithNoBold()
      .FromInput("colored text", "bold text", "normal text")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf --ansi --color=dark --tabstop=4 --no-bold",
      $"Expected 'fzf --ansi --color=dark --tabstop=4 --no-bold', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfWithHistoryOptions()
  {
    string command = Fzf.Builder()
      .WithHistory("/tmp/fzf_history")
      .WithHistorySize(100)
      .FromInput("history1", "history2", "history3")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf --history=/tmp/fzf_history --history-size=100",
      $"Expected 'fzf --history=/tmp/fzf_history --history-size=100', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfWithScriptingOptions()
  {
    string command = Fzf.Builder()
      .WithQuery("initial")
      .WithSelect1()
      .WithExit0()
      .WithFilter("test")
      .FromInput("initial test", "other item", "another option")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf --query=initial --select-1 --exit-0 --filter=test",
      $"Expected 'fzf --query=initial --select-1 --exit-0 --filter=test', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfWithComprehensiveOptions()
  {
    string command = Fzf.Builder()
      .WithMulti(5)
      .WithPreview("echo 'Preview: {}'")
      .WithPreviewWindow("right:50%")
      .WithHeightPercent(60)
      .WithBorder("sharp")
      .WithPrompt("Choose: ")
      .WithMarker("*")
      .WithPointer("=>")
      .WithHeader("Select multiple items")
      .FromInput("item1", "item2", "item3", "item4", "item5", "item6")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf --multi=5 \"--preview=echo 'Preview: {}'\" --preview-window=right:50% --height=60% --border=sharp \"--prompt=Choose: \" --marker=* --pointer==> \"--header=Select multiple items\"",
      $"Expected comprehensive fzf command, got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfWithFileInput()
  {
    string command = Fzf.Builder()
      .FromFiles("*.cs")
      .WithPreview("head -20 {}")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf \"--preview=head -20 {}\"",
      $"Expected 'fzf \"--preview=head -20 {{}}\"', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfWithCommandInput()
  {
    string command = Fzf.Builder()
      .FromCommand("echo hello world")
      .WithPrompt("Select output: ")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf \"--prompt=Select output: \"",
      $"Expected 'fzf \"--prompt=Select output: \"', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfWithWorkingDirectoryAndEnvironment()
  {
    // Note: Working directory and environment variables don't appear in ToCommandString()
    string command = Fzf.Builder()
      .WithWorkingDirectory("/tmp")
      .WithEnvironmentVariable("FZF_DEFAULT_OPTS", "--height 40%")
      .FromInput("env1", "env2", "env3")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf ",
      $"Expected 'fzf ', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfWithAdvancedLayoutOptions()
  {
    string command = Fzf.Builder()
      .WithLayout("reverse-list")
      .WithBorderLabel("Selection")
      .WithBorderLabelPos("10")
      .WithMargin("1,2,3,4")
      .WithPadding("1")
      .WithInfo("inline")
      .WithSeparator("---")
      .FromInput("advanced1", "advanced2", "advanced3")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf --layout=reverse-list --border-label=Selection --border-label-pos=10 --margin=1,2,3,4 --padding=1 --info=inline --separator=---",
      $"Expected 'fzf --layout=reverse-list --border-label=Selection --border-label-pos=10 --margin=1,2,3,4 --padding=1 --info=inline --separator=---', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfWithFieldProcessing()
  {
    string command = Fzf.Builder()
      .WithNth("1,2")
      .WithWithNth("2..")
      .WithDelimiter(":")
      .FromInput("field1:field2:field3", "data1:data2:data3", "info1:info2:info3")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf --nth=1,2 --with-nth=2.. --delimiter=:",
      $"Expected 'fzf --nth=1,2 --with-nth=2.. --delimiter=:', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfWithSortingAndTrackingOptions()
  {
    string command = Fzf.Builder()
      .WithNoSort()
      .WithTac()
      .WithTrack()
      .WithTiebreak("length,begin")
      .FromInput("short", "medium length", "very long text item")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf --no-sort --tac --track --tiebreak=length,begin",
      $"Expected 'fzf --no-sort --tac --track --tiebreak=length,begin', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfWithScrollbarAndEllipsis()
  {
    string command = Fzf.Builder()
      .WithScrollbar("█░")
      .WithEllipsis("...")
      .WithHeaderLines(2)
      .WithHeaderFirst()
      .FromInput("Header Line 1", "Header Line 2", "Content 1", "Content 2", "Content 3")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf --scrollbar=█░ --ellipsis=... --header-lines=2 --header-first",
      $"Expected 'fzf --scrollbar=█░ --ellipsis=... --header-lines=2 --header-first', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfWithPreviewLabelOptions()
  {
    string command = Fzf.Builder()
      .WithPreview("cat {}")
      .WithPreviewLabel("File Contents")
      .WithPreviewLabelPos(5)
      .WithPreviewWindow("right:60%:border")
      .FromInput("file1.txt", "file2.txt", "file3.txt")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf \"--preview=cat {}\" \"--preview-label=File Contents\" --preview-label-pos=5 --preview-window=right:60%:border",
      $"Expected 'fzf \"--preview=cat {{}}\" \"--preview-label=File Contents\" --preview-label-pos=5 --preview-window=right:60%:border', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfWithInputCollection()
  {
    var items = new List<string> { "collection1", "collection2", "collection3" };
    string command = Fzf.Builder()
      .FromInput(items)
      .WithPrompt("From collection: ")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf \"--prompt=From collection: \"",
      $"Expected 'fzf \"--prompt=From collection: \"', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfFilterMode()
  {
    // This tests that the command builds correctly even if fzf might not be installed
    string command = Fzf.Builder()
      .WithFilter("test")  // Use filter mode to avoid interactive requirement
      .FromInput("test1", "test2", "test3")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf --filter=test",
      $"Expected 'fzf --filter=test', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfWithListenAndSyncOptions()
  {
    string command = Fzf.Builder()
      .WithListen(8080)
      .WithSync()
      .FromInput("server1", "server2", "server3")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf --listen=8080 --sync",
      $"Expected 'fzf --listen=8080 --sync', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfWithNullHandlingOptions()
  {
    string command = Fzf.Builder()
      .WithRead0()
      .WithPrint0()
      .FromInput("null1", "null2", "null3")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf --read0 --print0",
      $"Expected 'fzf --read0 --print0', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfWithExpectAndPrintQuery()
  {
    string command = Fzf.Builder()
      .WithExpect("ctrl-a,ctrl-b")
      .WithPrintQuery()
      .FromInput("expect1", "expect2", "expect3")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf --expect=ctrl-a,ctrl-b --print-query",
      $"Expected 'fzf --expect=ctrl-a,ctrl-b --print-query', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFzfWithBindAndJumpLabels()
  {
    string command = Fzf.Builder()
      .WithBind("ctrl-a:select-all")
      .WithJumpLabels("abcdefghij")
      .WithFilepathWord()
      .FromInput("path/to/file1", "path/to/file2", "different/path/file3")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "fzf --bind=ctrl-a:select-all --jump-labels=abcdefghij --filepath-word",
      $"Expected 'fzf --bind=ctrl-a:select-all --jump-labels=abcdefghij --filepath-word', got '{command}'"
    );
    
    await Task.CompletedTask;
  }
}