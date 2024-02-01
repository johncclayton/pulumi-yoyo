namespace pulumi_yoyo.api;

public record StackData(
    string OrgName,
    string ProjectName,
    string StackName,
    long LastUpdate,
    int ResourceCount,
    bool Orphaned = false
)
{
    public string FullyQualifiedStackName => $"{OrgName}/{ProjectName}/{StackName}";
    public DateTime LastUpdateDateTime => new DateTime(1970, 1, 1).AddSeconds(LastUpdate);
    
    public static StackData OrphanFromFullyQualifiedStackName(string fullyQualifiedStackName)
    {
        var parts = fullyQualifiedStackName.Split("/");
        if(parts.Length != 3)
            throw new Exception($"Invalid fully qualified stack name {fullyQualifiedStackName}");
        return new StackData(parts[0], parts[1], parts[2], 0, 0, Orphaned: true);
    }
}

