namespace pulumi_yoyo.api;

public class StackUpdateInfo
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string Kind { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public long StartTime { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string Message { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Dictionary<string, string>? Environment { get; set; }
    public Dictionary<string, ConfigurationData>? Config { get; set; }
    
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string Result { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public long EndTime { get; set; }
    public int Version { get; set; }
    
    public StackUpdateResourceChanges? ResourceChanges { get; set; }

    public string GetConfigString(string key) => Config![key].String;
    public string GetConfigSecret(string key) => Config![key].Secret;
    public object GetConfigObject(string key) => Config![key].Object;
    public DateTime StartTimeDateTime => new DateTime(1970, 1, 1).AddSeconds(StartTime);
    public DateTime EndTimeDateTime => new DateTime(1970, 1, 1).AddSeconds(EndTime);

}
