using System.Diagnostics;

namespace pulumi_yoyo;

public interface IProcess
{
    void Start();
    void WaitForExit();
    int ExitCode { get; }
    ProcessStartInfo StartInfo { get; set; }
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
}

public class PulumiRunFactory
{
    public class ProcessWrapper
    {
        // ReSharper disable once InconsistentNaming
        public readonly IProcess process;
        // ReSharper disable once InconsistentNaming
        public readonly bool waitForExit;

        public ProcessWrapper(IProcess p, bool b)
        {
            process = p;
            waitForExit = b;
        }
    }
    
    public static ProcessWrapper CreateViaProcess(string workingDirectory, 
        IEnumerable<string> args, Func<string, bool> outputFunc, Func<string, bool> errorFunc, bool waitForExit = true)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "pulumi",
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

        var theWrappedProcess = new ProcessWithOutputFunctions(new Process { StartInfo = startInfo },
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
        
        return new ProcessWrapper(theWrappedProcess, waitForExit);
    }
    
    public static IProcess RunPulumiProcessWithConsole(ProcessWrapper wrapper)
    {
        var process = wrapper.process;
        
        process.Start();

        if(wrapper.waitForExit)
            process.WaitForExit();

        return process;
    }
}