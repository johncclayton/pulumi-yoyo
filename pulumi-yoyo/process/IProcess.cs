using config;

namespace pulumi_yoyo.process;

public interface IProcess
{
    /// <summary>
    /// Start the process, returning immediately.
    /// </summary>
    void Start();
    /// <summary>
    /// Wait for the processing to finish, after which the exit code can be read.
    /// </summary>
    void WaitForExit();
    /// <summary>
    /// Get the unix-like exit code, but @see ExitCodeMeaning for special meanings.
    /// </summary>
    int ExitCode { get; }
    string ToString();
    /// <summary>
    /// Relevant for local/computer based processing, represents the working directory that the command should execute in.
    /// </summary>
    string WorkingDirectory { get; set; }
    /// <summary>
    /// Adds the stack and stage to the environment variables of the process.  All values exported to the environment
    /// are exported as lower case.
    /// The format is as follows:
    /// YOYO_STACK_SHORT_NAME: The ShortName property of the stack
    /// YOYO_FULL_STACK_NAME: The FullStackName property of the stack
    /// YOYO_STAGE: The stage being run
    /// </summary>
    /// <param name="stack">The stack to add</param>
    /// <param name="stage">The stage being run</param>
    void AddStackAndStageToEnvironment(StackConfig stack, Stage stage);
    /// <summary>
    /// Adds the options to the environment, the format is as follows:
    /// YOYO_OPTION_&lt;PROPERTY NAME&gt;: The value of the property
    /// Dashes in property names are turned into underscores.  The entire name is uppercase.
    /// For example, the Option property called "dry-run" becomes YOYO_OPTION_DRY_RUN
    /// </summary>
    /// <param name="options">The options to add to the environment</param>
    void AddOptionsToEnvironment(Options options);
    /// <summary>
    /// Returns the entire environment of the process as a dictionary.
    /// </summary>
    IDictionary<string, string> Environment { get; }
    /// <summary>
    /// Property to store the stack that this process is running for.
    /// </summary>
    StackConfig? Stack { get; set; }
}