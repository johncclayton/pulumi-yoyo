using config;
using pulumi_yoyo.config;

namespace pulumi_yoyo;

public class RuntimeCommands
{
    private readonly IList<StackConfig> _execList;
    private readonly ConfigurationIterator _commandIterator;

    public RuntimeCommands(ProjectConfiguration projectConfig)
    {
        _commandIterator = new ConfigurationIterator(projectConfig);
        _execList = _commandIterator.GetHierarchyAsExecutionList();
    }

    public int RunPreviewCommand(PreviewOptions options)
    {
        return RunEach(GetCommands(new string[] {"preview"}), options);
    }

    public int RunUpCommand(UpOptions options)
    {
        return RunEach(GetCommands(new string[] {"up", "--skip-preview"}), options);
    }
    
    public int RunDestroyCommand(DestroyOptions options)
    {
        return RunEach(GetCommands(new string[] {"destroy"}), options);
    }
    
    private int RunEach(IEnumerable<PulumiProcessFactory.ProcessWrapper> getCommands, Options options)
    {
        foreach (var command in getCommands)
        {
            if(options.DryRun)
            {
                Console.WriteLine($"Would run command: {command.process.StartInfo.FileName} {command.process.StartInfo.Arguments}");
            }
            else
            {
                var process = PulumiProcessFactory.RunPulumiProcessWithConsole(command);
                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"Error running command: {command}");
                    return process.ExitCode;
                }
            }
        }

        return 0;
    }

    private IEnumerable<PulumiProcessFactory.ProcessWrapper> GetCommands(string[] commands)
    {
        foreach (var stack in _execList)
        {
            var workingDirectory = _commandIterator.Configuration.DirectoryPathForStack(stack);
   
            var theStackArgs = new List<string>
            {
                "-s", $"{stack.FullStackName}",
            };
        
            theStackArgs.AddRange(commands);

            yield return PulumiProcessFactory.CreatePulumiProcess(workingDirectory, theStackArgs, waitForExit: true);
        }
    }
}