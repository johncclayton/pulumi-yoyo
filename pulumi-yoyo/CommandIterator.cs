using config;
using pulumi_yoyo.config;

namespace pulumi_yoyo;

using ResponseWithFunc = Tuple<StackConfig, Func<ProjectConfiguration, StackConfig, bool>>;
using Response = StackConfig;

public class CommandIterator
{
    public CommandIterator(ProjectConfiguration config)
    {
        Configuration = config;
    }

    public ProjectConfiguration Configuration { get; set; }

    public IList<Response> GetHierarchyAsExecutionList()
    {
        var response = new List<Response>();
        return IterateStacks(response, Configuration.Stacks);
    }
    
    private IList<Response> IterateStacks(List<Response> response, IList<StackConfig> stacks)
    {
        // iterate the hierarchy, calling func() on each object.
        foreach (var oneStack in stacks)
        {
            if (oneStack.DependsOn is { Count: > 0 })
            {
                // create a list of the dependant stacks... 
                var dependentStacks = new List<StackConfig>();
                foreach (var dep in oneStack.DependsOn)
                {
                    var depStack = Configuration.Stacks.FirstOrDefault(x => x.ShortName == dep);
                    if (null != depStack)
                        dependentStacks.Add(depStack);
                }
                
                // recurse into the dependent stacks
                IterateStacks(response, dependentStacks);
            }
            
            // if the oneStack is NOT in the response list, add it
            if(response.All(x => x.ShortName != oneStack.ShortName))
                response.Add(oneStack);
        }

        return response;
    }
}