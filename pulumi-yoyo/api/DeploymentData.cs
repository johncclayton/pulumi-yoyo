namespace scan_pulumi;

public record ManifestData(string Time, string Magic, string Version);

public record SecretsProvidersData(string Type, Dictionary<string, object> State)
{
    public bool IsAzureKeyVault()
    {
        return Type == "cloud";
    }
}

public record ResourceData(
    string Urn,
    bool Custom,
    string Id,
    string Type,
    string Parent,
    string Provider,
    Dictionary<string, object> Inputs,
    Dictionary<string, object> Outputs);

public record DeploymentData(ManifestData Manifest, SecretsProvidersData SecretsProviders, ResourceData[]? Resources);