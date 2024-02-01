using pulumi_yoyo;
using pulumi_yoyo.config;

namespace unittests;

public class HierarchyUnitTests
{
    private readonly ProjectConfiguration? _projectConfiguration;

    public HierarchyUnitTests()
    {
        var data = File.ReadAllText("../../../test-projectconfig-deserialize.json");
        _projectConfiguration = ProjectConfiguration.ReadFromData(data);
    }
    
    [Fact]
    public void TestFullPathResolutionFromConfiguration()
    {
        var firstStack = _projectConfiguration?.Stacks.FirstOrDefault((x) => x.ShortName == "app");
        Assert.NotNull(firstStack);
        var fullPath = _projectConfiguration?.DirectoryPathForStack(firstStack!);
        Assert.Equal(Path.Combine("monkey", "example-app"), fullPath);
    }
    
    [Fact]
    public void TestCanIterateTheHierarchy()
    {
        // fetch a hierarchy iterator, and iterate it - we are looking for a specific flow...
        var it = new CommandIterator(_projectConfiguration);
        var commands = it.RunCommand((config, s) => true);
        
        var first = commands.First();
        Assert.Equal("cluster", first.Item1.ShortName);
        
        var second = commands.Skip(1).First();
        Assert.Equal("mssql", second.Item1.ShortName);
        
        var third = commands.Skip(2).First();
        Assert.Equal("app", third.Item1.ShortName);
    }
}