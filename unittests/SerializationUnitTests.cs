using Pulumi;
using pulumi_yoyo.config;

namespace unittests;

public class SerializationUnitTests
{
    [Fact]
    public void TestCanSerializeConfiguration()
    {
        var data = "../../../test-projectconfig-deserialize.json";
        ProjectConfiguration? obj = ProjectConfiguration.ReadFromFromFile(data);
        
        Assert.NotNull(obj);
        Assert.NotNull(obj.Environment);
        Assert.NotNull(obj.Stacks);
        Assert.NotNull(obj.Name);
        
        Assert.Equal("test", obj.Name);
        Assert.Equal(4, obj.Stacks.Count);
        Assert.Equal("wahaay", obj.Environment.Description);
        
        var stack = obj.Stacks[0];
        Assert.Equal("cluster", stack.ShortName);
        
        var stack2 = obj.Stacks[1];
        Assert.Equal("lastapp", stack2.ShortName);
        
        var stack3 = obj.Stacks[2];
        Assert.Equal("app", stack3.ShortName);
        
        var stack4 = obj.Stacks[3];
        Assert.Equal("mssql", stack4.ShortName);
    }
}