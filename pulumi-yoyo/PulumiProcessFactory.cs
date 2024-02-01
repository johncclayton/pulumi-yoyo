using System.Diagnostics;

namespace pulumi_yoyo;

public class PulumiProcessFactory
{
    public class ProcessWrapper
    {
        public Process process;
        public bool waitForExit;

        public ProcessWrapper(Process p, bool b)
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

        return new ProcessWrapper(new Process { StartInfo = startInfo }, waitForExit);
    }
    
    public static Process RunPulumiProcessWithConsole(ProcessWrapper wrapper)
    {
        var process = wrapper.process;
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.Error.WriteLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        if(wrapper.waitForExit)
            process.WaitForExit();

        return process;
    }
}