using config;
using pulumi_yoyo.config;
using pulumi_yoyo.process;
using QuikGraph;
using QuikGraph.Algorithms.Search;
using Spectre.Console;

namespace pulumi_yoyo;

public class WithPulumiCommands
{
    private readonly IList<StackConfig> _execList;
    private readonly ConfigurationIterator _commandIterator;

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

    public int RunDestroyStage(DestroyOptions options)
    {
        return RunEach(GetCommands(Stage.Destroy, options), options, reverse: true);
    }

    private int RunEach(IEnumerable<RunnableFactory.ProcessWrapper> getCommands, Options options, bool reverse = false)
    {
        var processWrappers = reverse ? getCommands.Reverse() : getCommands;

        string? startAtStack = options.FromStack;
        foreach (var command in processWrappers)
        {
            if (null != startAtStack && command.process.Stack?.ShortName != startAtStack)
            {
                continue;
            }

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
            if (null != options.ToStack && command.process.Stack?.ShortName == options.ToStack)
            {
                break;
            }
        }

        return 0;
    }

    public IEnumerable<RunnableFactory.ProcessWrapper> GetCommands(Stage stage, Options options)
    {
        foreach (var stack in _execList)
        {
            var workingDirectory = _commandIterator.Configuration.DirectoryPathForStack(stack);

            var pulumiArgs = new List<string>();
            switch (stage)
            {
                case Stage.Preview:
                    pulumiArgs.AddRange(new string[] { "preview" });
                    break;
                case Stage.Up:
                    pulumiArgs.AddRange(new string[] { "up", "--skip-preview" });
                    break;
                case Stage.Destroy:
                    pulumiArgs.AddRange(new string[] { "destroy", "--yes" });
                    break;
            }

            // by default, create a pulumi command.
            pulumiArgs.AddRange(new string[] { "-s", $"{stack.FullStackName}", "--non-interactive" });

            var pulumiTask = RunnableFactory.CreatePulumiProcess(workingDirectory, pulumiArgs, (string msg) =>
            {
                Console.WriteLine(msg);
                return true;
            }, (string msg) =>
            {
                Console.WriteLine(msg);
                return false;
            });

            pulumiTask.AddStackAndStageToEnvironment(stack, stage);

            IProcess actualTask;

            // check the yoyo configuration for the pre and post scripts.  If they exist, then we need to run them
            // as well, and then we use a LinkedProcess to chain them together.
            (bool exists, string preScriptPath) = PreScript(stack, stage);
            if (exists && options.UsePreStageScripts)
            {
                var preTask = RunnableFactory.CreateScriptProcess(preScriptPath, workingDirectory,
                    new string[] { preScriptPath }, (string msg) =>
                    {
                        Console.WriteLine(msg);
                        return true;
                    }, (string msg) =>
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
        var validExtensions = new string[] { "ps1", "sh", "bash", "python", "node" };
        foreach (var fileExtension in validExtensions)
        {
            // looking first in the working directory of the stack, pre-stage scripts might be there, if not
            // check in the current directory for the same thing. 
            var workingDirectory = _commandIterator.Configuration.DirectoryPathForStack(stack);

            var preFilename = Path.Combine(_commandIterator.Configuration.Name.ToLower(), stack.ShortName.ToLower(),
                $"pre-stage.{fileExtension}");

            var preScriptPath = Path.GetFullPath(Path.Combine(workingDirectory, ".yoyo", preFilename));
            if (File.Exists(preScriptPath))
            {
                return (true, preScriptPath);
            }

            preScriptPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ".yoyo", preFilename));
            if (File.Exists(preScriptPath))
            {
                return (true, preScriptPath);
            }
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
        dfs.ExamineEdge += (action) =>
        {
            StackConfig sourceConfig =
                _commandIterator.Configuration.Stacks.First(x => x.ShortName == action.Source);
            StackConfig targetConfig =
                _commandIterator.Configuration.Stacks.First(x => x.ShortName == action.Target);

            if (!theNodes.ContainsKey(action.Source))
                theNodes[action.Source] = tree.AddNode($"{action.Source}: {_commandIterator.Configuration.DirectoryPathForStack(sourceConfig)} {sourceConfig.FullStackName}");

            if (!theNodes.ContainsKey(action.Target))
            {
                var target = theNodes[action.Source].AddNode($"{action.Target}: {_commandIterator.Configuration.DirectoryPathForStack(targetConfig)} {targetConfig.FullStackName}");
                theNodes[action.Target] = target;
            }
        };

        dfs.Compute();

        AnsiConsole.Write(tree);
    }
}