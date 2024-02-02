using System.Diagnostics;
using config;

namespace pulumi_yoyo.process;

public class ProcessWithOutputFunctions : IProcess
{
    public Process Process { get; set; }
    public Options? Options { get; set; }

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

    public void AddOptionsToEnvironment(Options options)
    {
        Options = options;
        
        // iterate all the properties of the options, and add them to the environment
        foreach (var property in options.GetType().GetProperties())
        {
            var name = property.Name.ToUpper().Replace("-", "_");
            var value = property.GetValue(options)?.ToString();
            if (value != null)
            {
                Process.StartInfo.EnvironmentVariables["YOYO_OPTION_" + name] = value;
            }
        }
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