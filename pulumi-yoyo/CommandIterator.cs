using config;
using pulumi_yoyo.config;

namespace pulumi_yoyo;

using Response = Tuple<StackConfig, Func<ProjectConfiguration, StackConfig, bool>> ;

public class CommandIterator
{
    public CommandIterator(ProjectConfiguration config)
    {
        Configuration = config;
    }

    public ProjectConfiguration Configuration { get; set; }

    public IList<Response> RunCommand(Func<ProjectConfiguration, StackConfig, bool> func)
    {
        var response = new List<Response>();
        return IterateStacks(response, Configuration.Stacks, func);
    }
    
    private IList<Response> IterateStacks(List<Response> response, IList<StackConfig> stacks,
        Func<ProjectConfiguration, StackConfig, bool> func)
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

                foreach (var theStack in dependentStacks)
                {
                    // if it is not in response already, add it
                    if (response.All(x => x.Item1.ShortName != theStack.ShortName))
                        response.Add(new Response(theStack, func));
                }
            }
            
            // append the func to the stack...
            response.Add(new Response(oneStack, func));
        }

        return response;
    }
}