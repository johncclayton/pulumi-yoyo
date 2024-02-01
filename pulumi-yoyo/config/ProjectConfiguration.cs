using config;
using System.Text.Json;

namespace pulumi_yoyo.config;
public record ProjectConfiguration(
    EnvironmentConfig Environment,
    IList<StackConfig> Stacks
)
{
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
        // if the path is defined - use it
        var directoryPath = Environment.DefaultDirectoryForEnvironment ?? Directory.GetCurrentDirectory();
        return Path.Combine(directoryPath, forStack.DirectoryPath);
    }

}
