using dotenv.net.Utilities;

namespace scan_pulumi;

public static class EnvReaderExtensions
{
    public static string? GetOptionalStringValue(string key, string? defaultValue = null)
    {
        try
        {
            return EnvReader.GetStringValue(key);
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }
    
}