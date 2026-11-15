#!/usr/bin/env -S dotnet --

#region Purpose
// Multi-mode test runner: runs every Jaribu test class compiled into this runfile.
// Test classes are auto-registered via [ModuleInitializer] when compiled with JARIBU_MULTI.
#endregion

await TimeWarpTerminal.Default.WriteLineAsync("TimeWarp.Amuru Multi-Mode Test Runner");
await TimeWarpTerminal.Default.WriteLineAsync();

return await RunAllTests();
