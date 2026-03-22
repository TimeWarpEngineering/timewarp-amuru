#!/usr/bin/dotnet --

// Multi-mode Test Runner
// Test classes are auto-registered via [ModuleInitializer] when compiled with JARIBU_MULTI.

await TimeWarpTerminal.Default.WriteLineAsync("TimeWarp.Amuru Multi-Mode Test Runner");
await TimeWarpTerminal.Default.WriteLineAsync();

return await RunAllTests();
