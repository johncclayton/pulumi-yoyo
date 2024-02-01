using Flurl.Http;

namespace scan_pulumi;

public class PulumiServiceApiClient
{
    private string RootPulumiApi { get; set; }
    private string? PulumiAccessToken { get; set; }
    
    public PulumiServiceApiClient(string? accessToken = null)
    {
        RootPulumiApi = "https://api.pulumi.com";
        PulumiAccessToken = accessToken ?? Environment.GetEnvironmentVariable("PULUMI_ACCESS_TOKEN");
    }

    public async Task<StackListResponseData> GetAllStacksAsync(string? pulumiOrganization = null,
                                                               string? continuationToken = null,
                                                               string? projectNameFilter = null,
                                                               string? tagNameFilter = null)
    {
        var query = CreateRequest().AppendPathSegment("api/user/stacks");
        
        if(pulumiOrganization != null)
            query = query.SetQueryParam("organization", pulumiOrganization);
        if (projectNameFilter != null)
            query = query.SetQueryParam("project", projectNameFilter);
        if (tagNameFilter != null)
            query = query.SetQueryParam("tagName", tagNameFilter);
        if (continuationToken != null)
            query = query.SetQueryParam("continuationToken", continuationToken);
            
        return await query.GetJsonAsync<StackListResponseData>();
    }

    public async Task<StackResponseData> GetStackAsync(string fullyQualifiedStackName)
    {
        return await CreateRequest()
            .AppendPathSegment($"api/stacks/{fullyQualifiedStackName}")
            .GetJsonAsync<StackResponseData>();
    }

    public async Task<ExportStackStateResponseData> ExportStackStateAsync(string fullyQualifiedStackName)
    {
        return await CreateRequest()
            .AppendPathSegment($"api/stacks/{fullyQualifiedStackName}/export")
            .GetJsonAsync<ExportStackStateResponseData>();
    }
    
    public async Task<StackUpdateResponseData> GetStackLastUpdateAsync(string fullyQualifiedStackName)
    {
        return await CreateRequest()
            .AppendPathSegment($"api/stacks/{fullyQualifiedStackName}/updates")
            .SetQueryParam("pageSize", 1)
            .SetQueryParam("page", 1)
            .SetQueryParam("output-type", "service")
            .GetJsonAsync<StackUpdateResponseData>();
    }

    // public async Task<string> DecryptCipherTextForStackAsync(string encryptedBase64, string fullyQualifiedStackName)
    // {
    //     var response = await CreateRequest()
    //         .AppendPathSegment($"api/stacks/{fullyQualifiedStackName}/decrypt")
    //         .PostJsonAsync(new { ciphertext = encryptedBase64 });
    //     dynamic value = await response.GetJsonAsync<dynamic>();
    //     var valueBytes = Convert.FromBase64String(value["plaintext"].ToString());
    //     return System.Text.Encoding.UTF8.GetString(valueBytes);
    // }

    private IFlurlRequest CreateRequest()
    {
        return RootPulumiApi
            .WithHeader("Authorization", $"token {PulumiAccessToken}")
            .WithHeader("Accept", "application/vnd.pulumi+8")
            .WithHeader("Content-Type", "application/json");
    }
}