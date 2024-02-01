namespace scan_pulumi;

public record StackResponseData
(
    string OrgName,
    string ProjectName,
    string StackName,
    StackOperation? CurrentOperation,
    string ActiveUpdate,
    Dictionary<string, string> Tags,
    int Version
);