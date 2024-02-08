
using pulumi_yoyo;

namespace unittests;

public class OptionsTests
{
    [Fact]
    public void Test_OptionsOverride()
    {
        var opts = new Options();
        Assert.NotNull(opts);
        
        Assert.False(opts.DryRun);
        
        Environment.SetEnvironmentVariable("YOYO_OPTION_DRY_RUN", "True");
        opts.OverrideOptionsUsingEnv();
        Assert.True(opts.DryRun);
        
        Environment.SetEnvironmentVariable("YOYO_OPTION_DRY_RUN", "False");
        opts.OverrideOptionsUsingEnv();
        Assert.False(opts.DryRun);
        
        Environment.SetEnvironmentVariable("YOYO_OPTION_DRY_RUN", "false");
        opts.OverrideOptionsUsingEnv();
        Assert.False(opts.DryRun);        
    }
}