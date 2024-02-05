using config;
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
        
        var graph = GetGraph();
        var dfs = new DepthFirstSearchAlgorithm<string, Edge<string>>(graph);
       
        dfs.ExamineEdge += (action) =>
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
        
        return response;
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