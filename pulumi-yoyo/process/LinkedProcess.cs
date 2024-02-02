using config;
using pulumi_yoyo.process;

namespace pulumi_yoyo;

public class LinkedProcess : IProcess
{
    private ExitCodeMeaning _exitCode;

    // ReSharper disable once ConvertToPrimaryConstructor
    public LinkedProcess(IProcess first, IProcess then)
    {
        FirstProcess = first;
        ThenProcess = then;
        _exitCode = ExitCodeMeaning.Success;
    }

    public IProcess ThenProcess { get; set; }

    public IProcess FirstProcess { get; set; }
    
    // note: only the first process exit code is returned, if you ever need the second, then you
    // automatically need to use the "ThenProcess" property to get it, and you know it. 
    public int ExitCode => (int)_exitCode;

    public void Start()
    {
        FirstProcess.Start();
    }

    public void WaitForExit()
    {
        _exitCode = 0;
        
        FirstProcess.WaitForExit();
        if (FirstProcess.ExitCode == (int)ExitCodeMeaning.SuccessAndLinkedContinue)
        {
            Console.WriteLine("The first process was successful, now running second process.");

            ThenProcess.Start();
            ThenProcess.WaitForExit();
            
            _exitCode = (ExitCodeMeaning)ThenProcess.ExitCode;
        }
        else
        {
            if (FirstProcess.ExitCode == (int)ExitCodeMeaning.Success)
            {
                Console.WriteLine(
                    "The first process was successful, and requested that the second process is not run.");
            }
            else
            {
                _exitCode = (ExitCodeMeaning)FirstProcess.ExitCode;
            }
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

    public void AddOptionsToEnvironment(Options options)
    {
        FirstProcess.AddOptionsToEnvironment(options);
        ThenProcess.AddOptionsToEnvironment(options);
    }
    
    public IDictionary<string, string> Environment => FirstProcess.Environment;
}