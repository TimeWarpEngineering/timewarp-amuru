namespace TimeWarp.Multiavatar;

public static class PartTemplates
{
    private static readonly Dictionary<string, Dictionary<string, string>> Templates = new();
    
    static PartTemplates()
    {
        LoadTemplates();
    }
    
    private static void LoadTemplates()
    {
        // Load SVG templates from embedded resources
        Assembly assembly = typeof(PartTemplates).Assembly;
        string[] resourceNames = assembly.GetManifestResourceNames();
        
        // Look for template resources: TimeWarp.Multiavatar.Data.svg-templates.*.json
        string[] templateResources = resourceNames.Where(name => 
            name.Contains(".Data.svg-templates.", StringComparison.Ordinal) && 
            name.EndsWith(".json", StringComparison.Ordinal)).ToArray();
        
        if (templateResources.Length == 0)
        {
            // Try alternate naming pattern (hyphen might be converted to underscore)
            templateResources = resourceNames.Where(name => 
                name.Contains(".Data.svg_templates.", StringComparison.Ordinal) && 
                name.EndsWith(".json", StringComparison.Ordinal)).ToArray();
        }
        
        if (templateResources.Length == 0)
        {
            throw new InvalidOperationException(
                $"No SVG template resources found. Ensure Data/svg-templates/*.json files are embedded as resources. Available resources: {string.Join(", ", resourceNames.Take(5))}...");
        }
        
        foreach (string resourceName in templateResources)
        {
            // Extract avatar type from resource name
            // e.g., "TimeWarp.Multiavatar.Data.svg-templates.00-robo.json" -> "00"
            string fileName = resourceName.Split('.').Last(item => item.Contains('-', StringComparison.Ordinal));
            string avatarType = fileName.Split('-')[0];
            
            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) continue;
            
            using StreamReader reader = new(stream);
            string json = reader.ReadToEnd();
            Dictionary<string, string>? parts = JsonSerializer.Deserialize(json, MultiavatarJsonContext.Default.DictionaryStringString);
            
            if (parts != null)
            {
                Templates[avatarType] = parts;
            }
        }
    }
    
    public static string GetPart(string avatarType, string partName)
    {
        if (Templates.TryGetValue(avatarType, out Dictionary<string, string>? parts))
        {
            if (parts.TryGetValue(partName, out string? template))
            {
                return template;
            }
        }
        
        return string.Empty;
    }
    
    public static Dictionary<string, string> GetAllParts(string avatarType)
    {
        return Templates.TryGetValue(avatarType, out Dictionary<string, string>? parts) ? parts : new Dictionary<string, string>();
    }
}