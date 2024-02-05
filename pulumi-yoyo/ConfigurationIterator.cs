using config;
using Microsoft.Extensions.DependencyInjection;
using pulumi_yoyo.config;
using QuikGraph;
using QuikGraph.Algorithms.Search;

namespace pulumi_yoyo;

public class ConfigurationIterator
{
    public ConfigurationIterator(ProjectConfiguration config)
    {
        Configuration = config;
    }

    public ProjectConfiguration Configuration { get; set; }

    public IList<StackConfig> GetHierarchyAsExecutionList()
    {
        var response = new List<StackConfig>();
        IterateStacks(response, Configuration.Stacks, null);
        return response;
        /*
        var graph = GetGraph();
        var dfs = new DepthFirstSearchAlgorithm<string, Edge<string>>(graph);
        dfs.TreeEdge += (action) =>
        {
            StackConfig? sourceConfig = Configuration.Stacks.FirstOrDefault(x => x.ShortName == action.Source);
            StackConfig? targetConfig = Configuration.Stacks.FirstOrDefault(x => x.ShortName == action.Target);
            if (sourceConfig == null || targetConfig == null)
                return;

            if (response.All(c => c.ShortName != action.Source))
                response.Add(sourceConfig);

            if (response.All(c => c.ShortName != action.Target))
                response.Add(targetConfig);
        };
        
        dfs.Compute();
        response.Reverse();
        */
        
        return response;
    }
    
    private void IterateStacks(IList<StackConfig> response, IList<StackConfig> stacks, StackConfig?  parentStack, 
        Func<StackConfig, StackConfig?, bool>? func = null)
    {
        foreach (var oneStack in stacks)
        {
            if (oneStack.DependsOn is { Count: > 0 })
            {
                var dependsOn = new List<StackConfig>();
                foreach (var dep in oneStack.DependsOn)
                {
                    var depStack = Configuration.Stacks.FirstOrDefault(x => x.ShortName == dep);
                    if (null != depStack)
                        dependsOn.Add(depStack);
                }
                
                // recurse into the dependent stacks
                IterateStacks(response, dependsOn, oneStack, func);
            }
    
            if (response.All(x => x.ShortName != oneStack.ShortName))
            {
                response.Add(oneStack);
                func?.Invoke(oneStack, parentStack);
            }
        }
    }

    public BidirectionalGraph<string, Edge<string>> GetGraph()
    {
        var response = new BidirectionalGraph<string, Edge<string>>();
        foreach(var stack in Configuration.Stacks)
        {
            response.AddVertex(stack.ShortName);
            if (stack.DependsOn != null)
                foreach (var dependsOn in stack.DependsOn)
                {
                    response.AddVerticesAndEdge(new Edge<string>(stack.ShortName, dependsOn));
                }
        }

        return response;
    }
}