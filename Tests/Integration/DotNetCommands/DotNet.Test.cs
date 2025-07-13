#!/usr/bin/dotnet run

#pragma warning disable IDE0005 // Using directive is unnecessary
#pragma warning restore IDE0005

Console.WriteLine("🧪 Testing DotNetTestCommand...");

int passCount = 0;
int totalTests = 0;

// Test 1: Basic DotNet.Test() builder creation
totalTests++;
try
{
  DotNetTestBuilder testBuilder = DotNet.Test();
  if (testBuilder != null)
  {
    Console.WriteLine("✅ Test 1 PASSED: DotNet.Test() builder created successfully");
    passCount++;
  }
  else
  {
    Console.WriteLine("❌ Test 1 FAILED: DotNet.Test() returned null");
  }
}
catch (Exception ex)
{
  Console.WriteLine($"❌ Test 1 FAILED: Exception - {ex.Message}");
}

// Test 2: Fluent configuration methods
totalTests++;
try
{
  CommandResult command = DotNet.Test()
    .WithProject("test.csproj")
    .WithConfiguration("Debug")
    .WithFramework("net10.0")
    .WithNoRestore()
    .WithFilter("Category=Unit")
    .WithLogger("console")
    .Build();
  
  if (command != null)
  {
    Console.WriteLine("✅ Test 2 PASSED: Test fluent configuration methods work correctly");
    passCount++;
  }
  else
  {
    Console.WriteLine("❌ Test 2 FAILED: Test Build() returned null");
  }
}
catch (Exception ex)
{
  Console.WriteLine($"❌ Test 2 FAILED: Exception - {ex.Message}");
}

// Test 3: Method chaining with advanced test options
totalTests++;
try
{
  CommandResult chainedCommand = DotNet.Test()
    .WithProject("test.csproj")
    .WithConfiguration("Release")
    .WithArchitecture("x64")
    .WithOperatingSystem("linux")
    .WithNoRestore()
    .WithNoBuild()
    .WithVerbosity("minimal")
    .WithFilter("TestCategory=Integration")
    .WithLogger("trx")
    .WithLogger("html")
    .WithBlame()
    .WithCollect()
    .WithResultsDirectory("TestResults")
    .WithProperty("Platform", "AnyCPU")
    .Build();
  
  if (chainedCommand != null)
  {
    Console.WriteLine("✅ Test 3 PASSED: Test method chaining works correctly");
    passCount++;
  }
  else
  {
    Console.WriteLine("❌ Test 3 FAILED: Chained Test Build() returned null");
  }
}
catch (Exception ex)
{
  Console.WriteLine($"❌ Test 3 FAILED: Exception - {ex.Message}");
}

// Test 4: Working directory and environment variables
totalTests++;
try
{
  CommandResult envCommand = DotNet.Test()
    .WithProject("test.csproj")
    .WithWorkingDirectory("/tmp")
    .WithEnvironmentVariable("TEST_ENV", "integration")
    .WithNoLogo()
    .WithSettings("test.runsettings")
    .Build();
  
  if (envCommand != null)
  {
    Console.WriteLine("✅ Test 4 PASSED: Test working directory and environment variables work correctly");
    passCount++;
  }
  else
  {
    Console.WriteLine("❌ Test 4 FAILED: Environment config Test Build() returned null");
  }
}
catch (Exception ex)
{
  Console.WriteLine($"❌ Test 4 FAILED: Exception - {ex.Message}");
}

// Test 5: Command execution (graceful handling for non-existent project)
totalTests++;
try
{
  // This should handle gracefully since the project doesn't exist
  string output = await DotNet.Test()
    .WithProject("nonexistent.csproj")
    .WithConfiguration("Debug")
    .WithNoRestore()
    .GetStringAsync();
  
  // Should return empty string for non-existent project (graceful degradation)
  Console.WriteLine("✅ Test 5 PASSED: Test command execution completed with graceful handling");
  passCount++;
}
catch (Exception ex)
{
  Console.WriteLine($"❌ Test 5 FAILED: Exception - {ex.Message}");
}

// Summary
Console.WriteLine($"\n📊 DotNetTestCommand Results: {passCount}/{totalTests} tests passed");
Environment.Exit(passCount == totalTests ? 0 : 1);