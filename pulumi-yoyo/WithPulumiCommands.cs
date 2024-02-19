using config;
using pulumi_yoyo.api;
using pulumi_yoyo.config;
using pulumi_yoyo.process;
using QuikGraph;
using QuikGraph.Algorithms.Search;
using Spectre.Console;

namespace pulumi_yoyo;

public class WithPulumiCommands
{
    private readonly ConfigurationIterator _commandIterator;
    private readonly IList<StackConfig> _execList;

    public WithPulumiCommands(ProjectConfiguration projectConfig)
    {
        _commandIterator = new ConfigurationIterator(projectConfig);
        _execList = _commandIterator.GetHierarchyAsExecutionList();
    }

    public int RunPreviewStage(PreviewOptions options)
    {
        return RunEach(GetCommands(Stage.Preview, options), options);
    }

    public int RunUpStage(UpOptions options)
    {
        return RunEach(GetCommands(Stage.Up, options), options);
    }

    public int RunStackStage(StackOptions options)
    {
        return RunEach(GetCommands(Stage.Stack, options), options);
    }

    public int RunDestroyStage(DestroyOptions options)
    {
        return RunEach(GetCommands(Stage.Destroy, options), options, true);
    }

    private int RunEach(IEnumerable<RunnableFactory.ProcessWrapper> getCommands, Options options, bool reverse = false)
    {
        var processWrappers = reverse ? getCommands.Reverse() : getCommands;

        var startAtStack = options.FromStack;
        foreach (var command in processWrappers)
        {
            if (null != startAtStack && command.process.Stack?.ShortName != startAtStack) continue;

            startAtStack = null;

            if (options.DryRun)
            {
                Console.WriteLine($"Would run command: {command.process}");
            }
            else
            {
                command.process.AddOptionsToEnvironment(options);

                command.process.Start();
                command.process.WaitForExit();

                if (command.process.ExitCode != (int)ExitCodeMeaning.Success)
                {
                    Console.WriteLine($"Error running command: {command}");
                    return command.process.ExitCode;
                }
            }

            // if this is the "to" stack, stop here...
            if (null != options.ToStack && command.process.Stack?.ShortName == options.ToStack) break;
        }

        return 0;
    }

    public IEnumerable<RunnableFactory.ProcessWrapper> GetCommands(Stage stage, Options options)
    {
        // if we find -s or --stack - the user wants just one specific stack, 
        // and might use the name of a yoyo stack, in which case replace it with the name 
        // of the real stack and ONLY use this stack. 
        IList<StackConfig> stacksToProcess = _execList;
        
        if (options.args != null)
            for (var index = 0; index < options.args.Length; ++index)
            {
                var theVar = options.args[index];
                if (theVar == "-s" || theVar == "--stack")
                {
                    var matchingStackRef = _execList.FirstOrDefault(s => s.ShortName == options.args[index + 1]);
                    if (matchingStackRef != null)
                    {
                        stacksToProcess = new List<StackConfig>();
                        stacksToProcess.Add(matchingStackRef);
                    }
                }
            }

        foreach (var stack in stacksToProcess)
        {
            var workingDirectory = _commandIterator.Configuration.DirectoryPathForStack(stack);

            var pulumiArgs = new List<string>();
            switch (stage)
            {
                case Stage.Stack:
                    pulumiArgs.AddRange(new[] { "stack" });
                    break;
                case Stage.Preview:
                    pulumiArgs.AddRange(new[] { "preview" });
                    break;
                case Stage.Up:
                    pulumiArgs.AddRange(new[] { "up", "--yes" });
                    break;
                case Stage.Destroy:
                    pulumiArgs.AddRange(new[] { "destroy", "--yes" });
                    break;
            }

            // by default, create a pulumi command.
            pulumiArgs.AddRange(new[] { "-s", $"{stack.FullStackName}", "--non-interactive" });

            var pulumiTask = RunnableFactory.CreatePulumiProcess(workingDirectory, pulumiArgs, msg =>
            {
                Console.WriteLine(msg);
                return true;
            }, msg =>
            {
                Console.WriteLine(msg);
                return false;
            });

            pulumiTask.AddStackAndStageToEnvironment(stack, stage);

            IProcess actualTask;

            // check the yoyo configuration for the pre and post scripts.  If they exist, then we need to run them
            // as well, and then we use a LinkedProcess to chain them together.
            (var exists, var preScriptPath) = PreScript(stack, stage);
            if (exists && options.UsePreStageScripts)
            {
                var preTask = RunnableFactory.CreateScriptProcess(preScriptPath, workingDirectory,
                    new[] { preScriptPath }, msg =>
                    {
                        Console.WriteLine(msg);
                        return true;
                    }, msg =>
                    {
                        Console.WriteLine(msg);
                        return false;
                    });

                preTask.AddStackAndStageToEnvironment(stack, stage);

                actualTask = new LinkedProcess(preTask, pulumiTask);
            }
            else
            {
                actualTask = pulumiTask;
            }

            yield return new RunnableFactory.ProcessWrapper(actualTask, true);
        }
    }

    private (bool exists, string preScriptPath) PreScript(StackConfig stack, Stage stage)
    {
        var validExtensions = new[] { "ps1", "sh", "bash", "python", "node" };
        foreach (var fileExtension in validExtensions)
        {
            // looking first in the working directory of the stack, pre-stage scripts might be there, if not
            // check in the current directory for the same thing. 
            var workingDirectory = _commandIterator.Configuration.DirectoryPathForStack(stack);

            var preFilename = Path.Combine(_commandIterator.Configuration.Name.ToLower(), stack.ShortName.ToLower(),
                $"pre-stage.{fileExtension}");

            var preScriptPath = Path.GetFullPath(Path.Combine(workingDirectory, ".yoyo", preFilename));
            if (File.Exists(preScriptPath)) return (true, preScriptPath);

            preScriptPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ".yoyo", preFilename));
            if (File.Exists(preScriptPath)) return (true, preScriptPath);
        }

        return (false, string.Empty);
    }

    public void ShowViaSpectre()
    {
        // show config as a hierarchy/tree in spectre
        var ruler = new Rule("[green]Project Graph[/]")
        {
            Justification = Justify.Left
        };

        AnsiConsole.Write(ruler);

        var tree = new Tree("");

        IDictionary<string, TreeNode> theNodes = new Dictionary<string, TreeNode>();

        var graph = _commandIterator.GetGraph();
        var dfs = new DepthFirstSearchAlgorithm<string, Edge<string>>(graph);
        dfs.ExamineEdge += action =>
        {
            var sourceConfig = _commandIterator.Configuration.Stacks.FirstOrDefault(x => x.ShortName == action.Source);
            var targetConfig = _commandIterator.Configuration.Stacks.FirstOrDefault(x => x.ShortName == action.Target);

            if (!theNodes.ContainsKey(action.Source))
                theNodes[action.Source] = tree.AddNode($"{action.Source}: {sourceConfig?.FullStackName}");

            if (!theNodes.ContainsKey(action.Target))
            {
                var target = theNodes[action.Source].AddNode($"{action.Target}: {targetConfig?.FullStackName}");
                theNodes[action.Target] = target;
            }
        };

        dfs.Compute();

        AnsiConsole.Write(tree);
    }
}