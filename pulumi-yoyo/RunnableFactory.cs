using System.Diagnostics;
using pulumi_yoyo.process;

namespace pulumi_yoyo;

public enum ExitCodeMeaning : int
{
    Success = 0, // which for LinkedProcess this is a synonym for "no error and stop"
    SuccessAndLinkedStop = 0,
    SuccessAndLinkedContinue = 100
}

public class RunnableFactory
{
    public class ProcessWrapper
    {
        // ReSharper disable once InconsistentNaming
        public readonly IProcess process;
        // ReSharper disable once InconsistentNaming
        public readonly bool waitForExit;

        // ReSharper disable once ConvertToPrimaryConstructor
        public ProcessWrapper(IProcess p, bool b)
        {
            process = p;
            waitForExit = b;
        }
    }
    
    public static IProcess CreatePulumiProcess(string workingDirectory, 
        IEnumerable<string> args, Func<string, bool> outputFunc, Func<string, bool> errorFunc)
    {
        return ProcessWithCommandAndArgs("pulumi", workingDirectory, args, outputFunc, errorFunc);
    }
    
    public static IProcess CreateScriptProcess(string fullScriptPath, string workingDirectory, 
        IEnumerable<string> args, Func<string, bool> outputFunc, Func<string, bool> errorFunc)
    {
        string filename;
        if(fullScriptPath.EndsWith("ps1"))
            filename = "pwsh";
        else if(fullScriptPath.EndsWith(".sh"))
            filename = "bash";
        else if(fullScriptPath.EndsWith(".py"))
            filename = "python3";
        else if(fullScriptPath.EndsWith(".js"))
            filename = "node";
        else
            throw new Exception($"Unknown script type, for script filename: {fullScriptPath}");
        
        return ProcessWithCommandAndArgs(filename, workingDirectory, args, outputFunc, errorFunc);
    }
    
    private static IProcess ProcessWithCommandAndArgs(string filename, string workingDirectory, IEnumerable<string> args,
        Func<string, bool> outputFunc, Func<string, bool> errorFunc)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = filename,
            Arguments = string.Join(" ", args),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = workingDirectory,
            CreateNoWindow = true,
        };

        var env = Environment.GetEnvironmentVariables();
        foreach (var key in env.Keys)
        {
            var keyString = key?.ToString();
            if (keyString != null)
            {
                startInfo.EnvironmentVariables[keyString] = env[keyString]?.ToString();
            }
        }

        var thePulumiProcess = new ProcessWithOutputFunctions(new Process { StartInfo = startInfo },
            (string s, bool b) =>
            {
                if (b)
                {
                    return outputFunc(s);
                }
                else
                {
                    return errorFunc(s);
                }
            });
        
        return thePulumiProcess;
    }
}