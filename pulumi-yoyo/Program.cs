using CommandLine;
using dotenv.net;
using pulumi_yoyo;
using pulumi_yoyo.config;

string[] GetEnvFilePaths(int limit = 5)
{
    var filePaths = new List<string>();
    string? currentDir = Directory.GetCurrentDirectory();
    while(null != currentDir)
    {
        var envFilePath = Path.Combine(currentDir, ".env");
        if (File.Exists(envFilePath))
        {
            filePaths.Add(envFilePath);
        }

        currentDir = Path.GetDirectoryName(currentDir);
    }

    return filePaths.ToArray();
}

var envFilePaths = GetEnvFilePaths();
DotEnv.Load(new DotEnvOptions(envFilePaths: envFilePaths, trimValues: true, overwriteExistingVars: true));

var yoyoProjectFile = Environment.GetEnvironmentVariable("YOYO_PROJECT_PATH");
if (yoyoProjectFile is null)
{
    Console.WriteLine("YOYO_PROJECT_PATH is not set - cannot continue.");
    return;
}

bool found = File.Exists(yoyoProjectFile);
if (!found)
{
    // of course, envFilePaths is where we need to be looking, walk up this chain
    // from end to top.
    foreach (var envFilePathName in envFilePaths.Reverse())
    {
        var directoryForEnvFile = Path.GetDirectoryName(envFilePathName);
        if (null != directoryForEnvFile)
        {
            var yoyoAbsProjectPath = Path.GetFullPath(
                Path.Combine(directoryForEnvFile, yoyoProjectFile));
            found = File.Exists(yoyoAbsProjectPath);
            if (found)
            {
                yoyoProjectFile = yoyoAbsProjectPath;
                break;
            }
        }
        else
            break;
    }
}

if(!found)
{
    Console.WriteLine($"The YOYO_PROJECT_PATH file '{yoyoProjectFile}' does not exist - cannot continue.");
    return;
}

Console.WriteLine($"Using project file at: {yoyoProjectFile}");
var projectConfig = ProjectConfiguration.ReadFromFromFile(yoyoProjectFile);
if (projectConfig is null)
{
    Console.WriteLine("The project file does not contain a valid project configuration - cannot continue.");
    return;
}

projectConfig.ResolveDefaultPathForRelativeReferences(yoyoProjectFile);

// var pulumiAccessToken = EnvReaderExtensions.GetOptionalStringValue("PULUMI_ACCESS_TOKEN");
// var pulumiOrg = EnvReaderExtensions.GetOptionalStringValue("PULUMI_ORG", "soxes");
// if (pulumiAccessToken is null)
// {
//     Console.WriteLine("PULUMI_ACCESS_TOKEN is not set - cannot continue.  You can create and .env file with PULUMI_ACCESS_TOKEN in it.");
//     return;
// }

// var client = new PulumiServiceApiClient(pulumiAccessToken);

var cmds = new WithPulumiCommands(projectConfig);

var parser = new Parser(settings =>
{
    settings.IgnoreUnknownArguments = true;
    settings.AutoHelp = true;
});

parser.ParseArguments<PreviewOptions, StackOptions, UpOptions,
        DestroyOptions, ShowOptions>(args)
    .WithParsed<PreviewOptions>(options =>
    {
        options.SetArgumentsAndStripCommandWord(args, "preview");
        cmds.RunPreviewStage(options);
    })
    .WithParsed<UpOptions>(options =>
    {
        options.SetArgumentsAndStripCommandWord(args, "up");
        cmds.RunUpStage(options);
    })
    .WithParsed<DestroyOptions>(options =>
    {
        options.SetArgumentsAndStripCommandWord(args, "destroy");
        if(null != options.args)
            options.SetArgumentsAndStripCommandWord(options.args, "down");
        cmds.RunDestroyStage(options);
    })
    .WithParsed<StackOptions>(options =>
    {
        options.SetArgumentsAndStripCommandWord(args, "stack");
        cmds.RunStackStage(options);
    })
    .WithParsed<ShowOptions>(options => { cmds.ShowViaSpectre(); })
    ;