using config;
using System.Text.Json;

namespace pulumi_yoyo.config;

public class ProjectConfiguration
{
    public string Name { get; init; } = "";
    public EnvironmentConfig? Environment { get; init; }
    public string? DefaultPathForRelativeReferences { get; set; }
    public IList<StackConfig> Stacks { get; init; } = new List<StackConfig>();

    public static ProjectConfiguration? ReadFromFromFile(string yoyoProjectFile)
    {
        return ReadFromData(File.ReadAllText(yoyoProjectFile));
    }

    public static ProjectConfiguration? ReadFromData(string data)
    {
        // json de-serialize
        return JsonSerializer.Deserialize<ProjectConfiguration>(data);
    }
    
    public string DirectoryPathForStack(StackConfig forStack)
    {
        return DirectoryPathForStack(forStack.DirectoryPath);
    }
    
    public string DirectoryPathForStack(string? directoryPath)
    {
        return Path.Combine(DefaultPathForRelativeReferences ?? Directory.GetCurrentDirectory(), directoryPath);
    }

    public bool ResolveDefaultPathForRelativeReferences(string configurationFile)
    {
        var dirOfConfigFile = Path.GetDirectoryName(configurationFile);
        if (null != dirOfConfigFile)
        {
            DefaultPathForRelativeReferences = dirOfConfigFile;
            return true;
        }

        return false;
    }

}
