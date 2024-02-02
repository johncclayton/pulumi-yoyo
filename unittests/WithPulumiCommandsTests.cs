using pulumi_yoyo;
using pulumi_yoyo.config;

public class WithPulumiCommandsTests
{
    private readonly ProjectConfiguration? _projectConfiguration;

    public WithPulumiCommandsTests()
    {
        var data = File.ReadAllText("../../../test-projectconfig-deserialize.json");
        _projectConfiguration = ProjectConfiguration.ReadFromData(data);
        Assert.NotNull(_projectConfiguration);
    }
    
    [InlineData(Stage.Preview, 4)]
    [InlineData(Stage.Up, 4)]
    [Theory]
    public void Test_GetCommandsProducesAppropriateSeries(Stage theStage, int expectedCommandCount)
    {
        var commands = new WithPulumiCommands(_projectConfiguration!).GetCommands(theStage).ToArray();
        Assert.NotEmpty(commands);
        Assert.Equal(expectedCommandCount, commands.Length);
    }
}