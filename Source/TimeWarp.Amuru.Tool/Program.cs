namespace TimeWarp.Amuru.Tool;

/// <summary>
/// CLI tool for TimeWarp Amuru utilities
/// </summary>
internal static class Program
{
    /// <summary>
    /// Main entry point with basic Nuru routing
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Exit code (0 for success, 1 for error)</returns>
    public static async Task<int> Main(string[] args)
    {
        NuruAppBuilder builder = new();

        // Enable auto-help
        builder.AddAutoHelp();

        // Default route - simple test message
        builder.AddDefaultRoute(() =>
        {
            WriteLine("🌟 TimeWarp Amuru CLI Tool working!");
            WriteLine();
            WriteLine("Nuru routing is active");
        });

        NuruApp app = builder.Build();
        return await app.RunAsync(args);
    }
}
