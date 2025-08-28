namespace TimeWarp.Multiavatar;

public static class ThemeService
{
    private static readonly Dictionary<string, Theme> Themes = new();
    
    static ThemeService()
    {
        LoadThemes();
    }
    
    private static void LoadThemes()
    {
        // Load themes from embedded resources
        Assembly assembly = typeof(ThemeService).Assembly;
        string[] resourceNames = assembly.GetManifestResourceNames();
        
        // Look for theme resources: TimeWarp.Multiavatar.Data.themes.*.json
        string[] themeResources = resourceNames.Where(name => 
            name.StartsWith("TimeWarp.Multiavatar.Data.themes.", StringComparison.Ordinal) && 
            name.EndsWith(".json", StringComparison.Ordinal)).ToArray();
        
        if (themeResources.Length == 0)
        {
            throw new InvalidOperationException(
                "No theme resources found. Ensure Data/themes/*.json files are embedded as resources.");
        }
        
        foreach (string resourceName in themeResources)
        {
            // Extract theme number from resource name
            // e.g., "TimeWarp.Multiavatar.Data.themes.00-robo.json" -> "00"
            string fileName = resourceName.Split('.').Last(item => item.Contains('-', StringComparison.Ordinal));
            string themeNumber = fileName.Split('-')[0];
            
            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) continue;
            
            using StreamReader reader = new(stream);
            string json = reader.ReadToEnd();
            Theme? theme = JsonSerializer.Deserialize(json, MultiavatarJsonContext.Default.Theme);
            
            if (theme != null)
            {
                Themes[themeNumber] = theme;
            }
        }
    }
    
    public static Theme? GetTheme(string themeNumber)
    {
        return Themes.TryGetValue(themeNumber, out Theme? theme) ? theme : null;
    }
    
    public static string[] GetThemeColors(string themeNumber, string variant, string partName)
    {
        Theme? theme = GetTheme(themeNumber);
        if (theme == null) return Array.Empty<string>();
        
        Dictionary<string, string[]> variantData = variant switch
        {
            "A" => theme.A,
            "B" => theme.B,
            "C" => theme.C,
            _ => []
        };
        
        if (!variantData.TryGetValue(partName, out string[]? colors))
        {
            throw new KeyNotFoundException($"Part '{partName}' not found in theme {themeNumber} variant {variant}. Available parts: {string.Join(", ", variantData.Keys)}");
        }
        
        Error.WriteLine($"  Theme colors for {themeNumber}/{variant}/{partName}: [{string.Join(", ", colors.Select(c => $"\"{c}\""))}]");
        return colors;
    }
}