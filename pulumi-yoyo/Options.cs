﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using CommandLine;
namespace pulumi_yoyo;

public class Options
{
    public string[]? args { get; set; }
    
    [Option('c', "config", Required = false, HelpText = "Path to the project configuration file, by default yoyo will look for a .env file containing YOYO_PROJECT_PATH, but you can override this behaviour by using this flag.")]
    public string? ConfigFile { get; set; }
    
    [Option('f', "from-stack", Required = false, HelpText = "Deploy from this stack short name onwards")]
    public string? FromStack { get; set; }
    
    [Option('t', "to-stack", Required = false, HelpText = "Deploy up to this stack short name, and no further")]
    public string? ToStack { get; set; }
    
    [Option('p', "pre-stage", Required = false, Default = false, HelpText = "Use the pre-stage scripts as well - by default these are ignored.")]
    public bool UsePreStageScripts { get; set; }
    
    [Option('d', "dry-run", Required = false, Default = false, HelpText = "Just print what would be done, but don't actually do it.")]
    public bool DryRun { get; set; }
    
    [Option('v', "verbose", Required = false, Default = false, HelpText = "Print verbose logging information during the operation")]
    public bool Verbose { get; set; }
    
    public Options OverrideOptionsUsingEnv()
    {
        foreach (var property in GetType().GetProperties())
        {
            var attrs = property.GetCustomAttributes(typeof(OptionAttribute), false);
            if (attrs.Length == 0)
            {
                continue;
            }
            
            var optionAttr = attrs[0] as OptionAttribute;
            if (optionAttr is null)
            {
                continue;
            }
            
            var name = optionAttr.LongName.ToUpper().Replace("-", "_");
            var value = Environment.GetEnvironmentVariable("YOYO_OPTION_" + name);
            if (value != null)
            {
                property.SetValue(this, Convert.ChangeType(value, property.PropertyType));
            }
        }

        return this;
    }

    public void SetArgumentsAndStripCommandWord(string[] newArgs, string word)
    {
        args = newArgs.Where(val => val != word).ToArray();
    }
}

public class StackBasedOptions : Options
{
}

[Verb("preview", HelpText = "Run a pulumi preview command against all the stacks in the project")]
public class PreviewOptions : StackBasedOptions 
{
}

[Verb("stack", HelpText = "Operate on the stack, you can specify the stack with the name of the YoYo stack and this will be replaced with the actual stack name")]
public class StackOptions : StackBasedOptions 
{
}

[Verb("up", HelpText = "Run a pulumi up command against all the stacks in the project")]
public class UpOptions : StackBasedOptions
{
}

[Verb(name: "destroy", aliases: new string[] {"delete", "down"}, HelpText = "Run a pulumi destroy command against all the stacks in the project")]
public class DestroyOptions : StackBasedOptions
{
}

[Verb(name: "show", isDefault: true, aliases: new string[] {"info"}, HelpText = "Show the project configuration")]
public class ShowOptions 
{
}

