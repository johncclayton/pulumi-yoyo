using System.Text.Json;

namespace pulumi_yoyo.api;

public class PulumiStackExportCache
{
    public PulumiStackExportCache(string cacheDirectory)
    {
        CacheDirectory = cacheDirectory;
        Directory.CreateDirectory(cacheDirectory);
    }

    public string CacheDirectory { get; }

    public static string StackNameToFilename(string stackName)
    {
        return stackName.Replace("/", "-");
    }

    private string FilenameForStack(string stackFullyQualifiedStackName)
    {
        var file = Path.Combine(CacheDirectory, StackNameToFilename(stackFullyQualifiedStackName)) + ".json";
        return Path.GetFullPath(file);
    }
    
    public void ClearCacheDirectory()
    {
        var directoryInfo = new DirectoryInfo(CacheDirectory);

        foreach (var file in directoryInfo.GetFiles())
        {
            file.Delete();
        }
    }
    
    public bool FetchExportedStackState(string stackFullyQualifiedStackName, ref ExportStackStateResponseData? answer)
    {
        // turn the fully qualified stack name into a filename
        var filename = FilenameForStack(stackFullyQualifiedStackName);
        if (!File.Exists(filename)) return false;
        
        answer = JsonSerializer.Deserialize<ExportStackStateResponseData>(File.ReadAllText(filename));
        return answer != null;
    }

    public void StoreExportedStackState(string stackFullyQualifiedStackName, ExportStackStateResponseData response)
    {
        var filename = FilenameForStack(stackFullyQualifiedStackName);
        File.WriteAllText(filename, JsonSerializer.Serialize(response, new JsonSerializerOptions()
        {
            WriteIndented = true,
        }));
    }
}