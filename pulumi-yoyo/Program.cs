// See https://aka.ms/new-console-template for more information
using dotenv.net;
using dotenv.net.Utilities;
using pulumi_yoyo;
using pulumi_yoyo.config;
using scan_pulumi;

DotEnv.Fluent()
    .WithProbeForEnv(4)
    .WithTrimValues()
    .Load();

var pulumiAccessToken = EnvReaderExtensions.GetOptionalStringValue("PULUMI_ACCESS_TOKEN");
// var pulumiOrg = EnvReaderExtensions.GetOptionalStringValue("PULUMI_ORG", "soxes");
// if (pulumiAccessToken is null)
// {
//     Console.WriteLine("PULUMI_ACCESS_TOKEN is not set - cannot continue.  You can create and .env file with PULUMI_ACCESS_TOKEN in it.");
//     return;
// }

// var client = new PulumiServiceApiClient(pulumiAccessToken);

// an idea... if you can walk up the tree, e.g. DOTNET-env style configuration, and find a YOYO_PROJECT_PATH file, read it. 
var yoyoProjectFile = EnvReaderExtensions.GetOptionalStringValue("YOYO_PROJECT_PATH");
if(yoyoProjectFile is null)
{
    Console.WriteLine("YOYO_PROJECT_PATH is not set - cannot continue.");
    return;
}

if (!Path.Exists(yoyoProjectFile))
{
    Console.WriteLine("The YOYO_PROJECT_PATH file does not exist - cannot continue.");
    return;
}

Console.WriteLine($"Using project file at: {yoyoProjectFile}");

var projectConfig = ProjectConfiguration.ReadFromFromFile(yoyoProjectFile);
if (projectConfig is null)
{
    Console.WriteLine("The project file does not contain a valid project configuration - cannot continue.");
    return;
}

var commandIterator = new CommandIterator(projectConfig);
var commandsToRun = commandIterator.GetHierarchyAsExecutionList();
foreach (var stack in commandsToRun)
{
    Console.WriteLine($"Stack: {stack.ShortName}, Directory: {stack.DirectoryPath}");    
}

// next step: use the PulumiController to: 
// 1. check if the stack is already deployed - perhaps the cluster is not started, so start it? 
// 2. pulumi up if not deployed

// when doing pulumi up, we need to know to "where" along the hierarchy path we need to go.  
// e.g. mssql does not deploy app
// 1. by default we deploy everything. 
// 2. must also be an option to skip the hierarchy steps and just pulumi up the stack we are dealing with (e.g. app) 

// examples: 
// yoyo up --name app --skip-hierarchy (-s)
// yoyo up --to mssql 
// yoyo down (defaults to everything)
// yoyo down --stop-at cluster (apps and mssql are destroyed, cluster is left running)