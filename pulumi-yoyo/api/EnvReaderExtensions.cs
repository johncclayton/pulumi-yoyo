using dotenv.net.Utilities;

namespace pulumi_yoyo.api;

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