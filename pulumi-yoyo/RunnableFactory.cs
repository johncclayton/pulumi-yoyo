using System.Diagnostics;
using config;

namespace pulumi_yoyo;

public enum ExitCodeMeaning : int
{
    Success = 0, // which for LinkedProcess this is a synonym for "no error and stop"
    SuccessAndLinkedStop = 0,
    SuccessAndLinkedContinue = 100
}

public interface IProcess
{
    void Start();
    void WaitForExit();
    int ExitCode { get; }
    string ToString();
    string WorkingDirectory { get; set; }
    void AddStackAndStageToEnvironment(StackConfig stack, Stage stage);
}

public class LinkedProcess : IProcess
{
    public LinkedProcess(IProcess first, IProcess then)
    {
        FirstProcess = first;
        ThenProcess = then;
    }

    public IProcess ThenProcess { get; set; }

    public IProcess FirstProcess { get; set; }
    
    // note: only the first process exit code is returned, if you ever need the second, then you
    // automatically need to use the "ThenProcess" property to get it, and you know it. 
    public int ExitCode => FirstProcess.ExitCode;

    public void Start()
    {
        FirstProcess.Start();
    }

    public void WaitForExit()
    {
        FirstProcess.WaitForExit();
        if (FirstProcess.ExitCode == (int)ExitCodeMeaning.SuccessAndLinkedContinue)
        {
            ThenProcess.Start();
            ThenProcess.WaitForExit();
        }

        if (FirstProcess.ExitCode == (int)ExitCodeMeaning.SuccessAndLinkedStop)
        {
            Console.WriteLine("The first process was successful, and requested that the second process is not run.");
        }
    }
    
    public override string ToString()
    {
        return $"{FirstProcess} --then-- {ThenProcess}";
    }    
    
    public string WorkingDirectory
    {
        get => FirstProcess.WorkingDirectory;
        set
        {
            FirstProcess.WorkingDirectory = value;
            ThenProcess.WorkingDirectory = value;
        }
    }
    
    public void AddStackAndStageToEnvironment(StackConfig stack, Stage stage)
    {
        FirstProcess.AddStackAndStageToEnvironment(stack, stage);
        ThenProcess.AddStackAndStageToEnvironment(stack, stage);
    }
}

public class ProcessWithOutputFunctions : IProcess
{
    public Process Process { get; set; }

    public ProcessWithOutputFunctions(Process process, Func<string, bool, bool> outputFunc)
    {
        Process = process;
        
        Process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputFunc(e.Data, true);
            }
        };

        Process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputFunc(e.Data, false);
            }
        };
    }

    public ProcessStartInfo StartInfo
    {
        get => Process.StartInfo;
        set => Process.StartInfo = value;
    }
    
    public int ExitCode => Process.ExitCode;
    
    public void Start()
    {
        Process.Start();
        Process.BeginErrorReadLine();
        Process.BeginOutputReadLine();
    }

    public void WaitForExit()
    {
        Process.WaitForExit();
    }
    
    public override string ToString()
    {
        return $"{Process.StartInfo.FileName} {Process.StartInfo.Arguments}";
    }
    
    public string WorkingDirectory
    {
        get => Process.StartInfo.WorkingDirectory;
        set => Process.StartInfo.WorkingDirectory = value;
    }
    
    public void AddStackAndStageToEnvironment(StackConfig stack, Stage stage)
    {
        Process.StartInfo.EnvironmentVariables["YOYO_FULL_STACK_NAME"] = stack.FullStackName;
        Process.StartInfo.EnvironmentVariables["YOYO_SHORT_NAME"] = stack.ShortName;
        Process.StartInfo.EnvironmentVariables["YOYO_STAGE"] = stage.ToString();
    }
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
            filename = "python";
        else if(fullScriptPath.EndsWith(".js"))
            filename = "node";
        else
            throw new System.Exception("Unknown script type");
        
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