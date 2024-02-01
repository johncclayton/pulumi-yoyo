using System.Diagnostics;

namespace pulumi_yoyo;

public interface IProcess
{
    void Start();
    void BeginOutputReadLine();
    void BeginErrorReadLine();
    void WaitForExit();
    int ExitCode { get; }
    ProcessStartInfo StartInfo { get; set; }
}

public class DotNetProcess : IProcess
{
    public Process Process { get; set; }

    public DotNetProcess(Process process)
    {
        Process = process;
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
    }

    public void BeginOutputReadLine()
    {
        Process.BeginOutputReadLine();
    }

    public void BeginErrorReadLine()
    {
        Process.BeginErrorReadLine();
    }

    public void WaitForExit()
    {
        Process.WaitForExit();
    }
}

public class PulumiProcessFactory
{
    public class ProcessWrapper
    {
        public IProcess process;
        public bool waitForExit;

        public ProcessWrapper(IProcess p, bool b)
        {
            process = p;
            waitForExit = b;
        }
    }
    
    public static ProcessWrapper CreatePulumiProcess(string workingDirectory, IEnumerable<string> args, bool waitForExit = true)
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

        var theWrappedProcess = new DotNetProcess(new Process { StartInfo = startInfo });
        
        return new ProcessWrapper(theWrappedProcess, waitForExit);
    }
    
    public static IProcess RunPulumiProcessWithConsole(ProcessWrapper wrapper)
    {
        var process = wrapper.process;
        var dotnetProcess = process as Process;
        if (dotnetProcess != null)
        {
            dotnetProcess.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Console.WriteLine(e.Data);
                }
            };

            dotnetProcess.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Console.Error.WriteLine(e.Data);
                }
            };
        }

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        if(wrapper.waitForExit)
            process.WaitForExit();

        return process;
    }
}