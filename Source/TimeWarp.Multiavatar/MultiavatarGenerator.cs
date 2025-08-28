namespace TimeWarp.Multiavatar;

public static class MultiavatarGenerator
{
    public static string Generate(string input, bool sansEnv = false, Dictionary<string, string>? ver = null)
    {
        if (string.IsNullOrEmpty(input))
            return "";
        
        // Generate hash and extract parts
        string hash = HashService.GenerateHash(input);
        
        // Get parts selection (range 0-47)
        var parts = new Dictionary<string, string>
        {
            ["env"] = HashService.GetPartSelection(hash.Substring(0, 2)),
            ["clo"] = HashService.GetPartSelection(hash.Substring(2, 2)),
            ["head"] = HashService.GetPartSelection(hash.Substring(4, 2)),
            ["mouth"] = HashService.GetPartSelection(hash.Substring(6, 2)),
            ["eyes"] = HashService.GetPartSelection(hash.Substring(8, 2)),
            ["top"] = HashService.GetPartSelection(hash.Substring(10, 2))
        };
        
        // Determine avatar type from the env part
        string envPartSelection = parts["env"];  // e.g. "07C"
        string avatarType = envPartSelection.Substring(0, 2);  // e.g. "07"
        
        Error.WriteLine($"DEBUG: Avatar type={avatarType} from env={envPartSelection}");
        
        // Get the final SVG for each part
        var final = new Dictionary<string, string>();
        
        foreach (KeyValuePair<string, string> part in parts)
        {
            if (part.Key == "part") continue; // Skip the debug part
            
            // Extract the part number and theme variant from this part's selection
            string partNumber = part.Value[..2]; // First 2 chars, e.g. "12" from "12A"
            string partThemeVariant = part.Value[^1..]; // Last character, e.g. "A" from "12A"
            
            // Override with version if provided
            string finalPartNumber = partNumber;
            string finalThemeVariant = partThemeVariant;
            
            if (ver != null)
            {
                if (ver.TryGetValue("part", out string? verPart))
                {
                    finalPartNumber = verPart;
                }
                
                if (ver.TryGetValue("theme", out string? verTheme))
                {
                    finalThemeVariant = verTheme;
                }
            }
            
            Error.WriteLine($"  Part {part.Key}: partNumber={finalPartNumber}, theme={finalThemeVariant}");
            final[part.Key] = GetFinalSvg(part.Key, finalPartNumber, finalThemeVariant);
        }
        
        // Without environment
        if (sansEnv)
        {
            final["env"] = "";
        }
        
        // Combine all parts in the correct order
        string svgStart = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 231 231\">";
        string svgEnd = "</svg>";
        
        return svgStart + 
               final["env"] + 
               final["head"] + 
               final["clo"] + 
               final["top"] + 
               final["eyes"] + 
               final["mouth"] + 
               svgEnd;
    }
    
    private static string GetFinalSvg(string partName, string avatarType, string themeVariant)
    {
        // Get the avatar type number (e.g. "07" from "07C")
        string avatarTypeNumber = avatarType.Length >= 2 ? avatarType.Substring(0, 2) : avatarType;
        
        // Get SVG template for this part
        string svgTemplate = PartTemplates.GetPart(avatarTypeNumber, partName);
        if (string.IsNullOrEmpty(svgTemplate))
        {
            return ""; // Part not found
        }
        
        // Get theme colors for this part
        string[] colors = ThemeService.GetThemeColors(avatarTypeNumber, themeVariant, partName);
        if (colors.Length == 0)
        {
            return svgTemplate; // Return without color replacement
        }
        
        // Replace colors
        return ColorReplacementService.ReplaceColors(svgTemplate, colors);
    }
}