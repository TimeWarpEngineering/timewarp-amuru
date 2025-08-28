namespace TimeWarp.Multiavatar;

public static partial class ColorReplacementService
{
    public static string ReplaceColors(string svgString, string[] colors)
    {
        ArgumentException.ThrowIfNullOrEmpty(svgString);
        ArgumentNullException.ThrowIfNull(colors);
        
        // Find all color placeholders in the SVG
        Regex regex = ColorPlaceholderRegex();
        MatchCollection matches = regex.Matches(svgString);
        
        // Replicate JavaScript's behavior: replace first occurrence of each match
        string resultFinal = svgString;
        
        for (int i = 0; i < matches.Count && i < colors.Length; i++)
        {
            string fullMatch = matches[i].Value;  // e.g., "#01;"
            string replacement = colors[i] + ";";  // e.g., "#ff0000;"
            
            Error.WriteLine($"Replacing first occurrence of {fullMatch} -> {replacement}");
            
            // Find and replace the FIRST occurrence in the current string
            int index = resultFinal.IndexOf(fullMatch, StringComparison.Ordinal);
            if (index >= 0)
            {
                resultFinal = resultFinal.Remove(index, fullMatch.Length).Insert(index, replacement);
            }
        }
        
        return resultFinal;
    }

    [GeneratedRegex(@"#(.*?);")]
    private static partial Regex ColorPlaceholderRegex();
}