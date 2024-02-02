using pulumi_yoyo;

namespace config;

public record StackConfig(
    string ShortName,
    string DirectoryPath,
    string FullStackName,
    IList<string>? DependsOn
);

