using config;
using pulumi_yoyo.api;

namespace pulumi_yoyo;

public class PulumiModel
{
    public static async Task<IList<StackConfig>> GetStackHierarchy(string org, string stackName)
    {
        var api = new PulumiServiceApiClient();

        // cache all the "state" data for all stacks.
        var cacheStackData = new Dictionary<string, StackData>();
        var cacheStackState = new Dictionary<string, ExportStackStateResponseData>();
        await foreach (var stacks in api.GetAllStacksAsync(org))
        {
            foreach (var stack in stacks)
            {
                cacheStackData.Add(stack.FullyQualifiedStackName, stack);
                cacheStackState.Add(stack.FullyQualifiedStackName,
                    await api.ExportStackStateAsync(stack.FullyQualifiedStackName));
            }
        }

        var results = new List<StackConfig>();
        var parentStackQueue = new Stack<string>();
        parentStackQueue.Push(stackName);
        while (parentStackQueue.Count > 0)
        {
            var parentStackName = parentStackQueue.Pop();
            
            // pull information for the given stackName
            if (cacheStackData.TryGetValue(stackName, out var parentStackData))
            {
                StackConfig config = new StackConfig(parentStackData.FullyQualifiedStackName, 
                    null, parentStackData.FullyQualifiedStackName, new List<string>());
                
                var parentStackState = cacheStackState[stackName];
                if (parentStackState.Deployment.Resources != null)
                {
                    var children = parentStackState.Deployment.Resources.Where(r =>
                        r.Type == "pulumi:pulumi:StackReference" && r.Id == parentStackName);
                    
                    foreach (var child in children)
                    {
                        parentStackQueue.Push(child.Id);
                        config.DependsOn?.Add(child.Id);
                    }
                }
                
                results.Add(config);
            }
        }

        return results;
    }
}