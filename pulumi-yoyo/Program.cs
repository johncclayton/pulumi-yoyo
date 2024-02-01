// See https://aka.ms/new-console-template for more information
using dotenv.net;
using dotenv.net.Utilities;
using scan_pulumi;

DotEnv.Fluent()
    .WithProbeForEnv(4)
    .WithTrimValues()
    .Load();

string? pulumiAccessToken = EnvReader.GetStringValue("PULUMI_ACCESS_TOKEN");
string pulumiOrg = EnvReaderExtensions.GetOptionalStringValue("PULUMI_ORG", "soxes");

if (pulumiAccessToken is null)
{
    Console.WriteLine("PULUMI_ACCESS_TOKEN is not set - cannot continue.  You can create and .env file with PULUMI_ACCESS_TOKEN in it.");
    return;
}

var client = new PulumiServiceApiClient(pulumiAccessToken);
