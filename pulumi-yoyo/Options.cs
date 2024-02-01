using System.Xml.Serialization;
using CommandLine;
namespace pulumi_yoyo;

public class Options
{
    [Option('c', "config", Required = false, HelpText = "Path to the project configuration file, by default yoyo will look for a .env file containing YOYO_PROJECT_PATH, but you can override this behaviour by using this flag.")]
    public string? ConfigFile { get; set; }
    
    [Option('f', "from-stack", Required = false, HelpText = "Deploy from this stack short name onwards")]
    public string? FromStack { get; set; }
    
    [Option('t', "to-stack", Required = false, HelpText = "Deploy up to this stack short name, and no further")]
    public string? ToStack { get; set; }
    
    [Option("dry-run", Required = false, Default = false, HelpText = "Just print what would be done, but don't actually do it.")]
    public bool DryRun { get; set; }
    
    [Option('v', "verbose", Required = false, Default = false, HelpText = "Print verbose logging information during the operation")]
    public bool Verbose { get; set; }
}

[Verb("preview", isDefault: true, HelpText = "Run a pulumi preview command against all the stacks in the project")]
public class PreviewOptions : Options 
{
}

[Verb("up", HelpText = "Run a pulumi up command against all the stacks in the project")]
public class UpOptions : Options
{
    [Option("try-start", Required = false, Default = true, HelpText = "For cluster stacks, start them if possible, otherwise create/up")]
    public bool TryToStart { get; set; }
}

[Verb(name: "destroy", aliases: new string[] {"delete", "down"}, HelpText = "Run a pulumi destroy command against all the stacks in the project")]
public class DestroyOptions : Options
{
    [Option("try-stop", Required = false, Default = true, HelpText = "For cluster stacks, if automation is active just stop the cluster - don't destroy it.")]
    public bool TryToStop { get; set; }
}

