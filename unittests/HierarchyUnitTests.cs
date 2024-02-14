using Microsoft.Extensions.Logging;
using pulumi_yoyo;
using pulumi_yoyo.config;
using Xunit.Abstractions;

namespace unittests;

public class HierarchyUnitTests
{
    private ProjectConfiguration? _projectConfiguration;
    
    public HierarchyUnitTests(ITestOutputHelper output)
    {
        var data = "../../../test-projectconfig-deserialize.json";
        _projectConfiguration = ProjectConfiguration.ReadFromFromFile(data);
    }

    [InlineData("c:\\monkey\\yoyo.json", "testing", "c:\\monkey\\testing")]
    [InlineData("c:\\monkey\\", "testing", "c:\\monkey\\testing")]
    [InlineData("c:\\monkey", "testing", "c:\\testing")]
    [Theory]
    public void TestDirectoryPathForStackCanBeRelative(string jsonPathname, string? stackPathname, string expectedFullPath)
    {
        // the YOYO_PROJECT_PATH is absolute - and points to the .json configuration file 
        //
        // with that configuration file, we have Environment.DirectoryPathForStack set to either
        // - a relative path
        // - an absolute path
        //
        // if it is a relative path, we should resolve it to the directory of the .json file
        Assert.True(_projectConfiguration?.ResolveDefaultPathForRelativeReferences(jsonPathname));
        Assert.Equal(expectedFullPath, _projectConfiguration?.DirectoryPathForStack(stackPathname));
    }
    
    [Fact]
    public void TestFullPathResolutionFromConfiguration()
    {
        _projectConfiguration?.ResolveDefaultPathForRelativeReferences("c:\\monkey\\yoyo.json");
        var firstStack = _projectConfiguration?.Stacks.FirstOrDefault((x) => x.ShortName == "app");
        Assert.NotNull(firstStack);
        var fullPath = _projectConfiguration?.DirectoryPathForStack(firstStack!);
        Assert.Equal(Path.Combine("c:\\monkey", "example-app"), fullPath);
    }
    
    [Fact]
    public void TestCanIterateTheHierarchy()
    {
        // fetch a hierarchy iterator, and iterate it - we are looking for a specific flow...
        var it = new ConfigurationIterator(_projectConfiguration ?? throw new InvalidOperationException());
        var commands = it.GetHierarchyAsExecutionList();
        
        Assert.Equal(4, commands.Count);
        
        var first = commands.First();
        Assert.Equal("cluster", first.ShortName);
        
        var second = commands.Skip(1).First();
        Assert.Equal("mssql", second.ShortName);
        
        var third = commands.Skip(2).First();
        Assert.Equal("app", third.ShortName);
        
        var last = commands.Skip(3).First();
        Assert.Equal("lastapp", last.ShortName);
        
    }
}