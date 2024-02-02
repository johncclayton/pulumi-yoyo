using System.Collections;
using config;
using Moq;
using pulumi_yoyo;
using pulumi_yoyo.process;

namespace unittests
{
    public class LinkedProcessTests
    {
        [Fact]
        public void Start_FirstProcessSuccessAndRequiresStop()
        {
            var mockProcess = new Mock<IProcess>();
            var mockProcess2 = new Mock<IProcess>();
            
            mockProcess.Setup(p => p.ExitCode).Returns((int)ExitCodeMeaning.SuccessAndLinkedStop);
            mockProcess2.Setup(p => p.ExitCode).Returns(34);
            
            var linkedProcess = new LinkedProcess(mockProcess.Object, mockProcess2.Object);

            linkedProcess.Start();
            linkedProcess.WaitForExit();

            mockProcess.Verify(p => p.Start(), Times.Once);
            mockProcess.Verify(p => p.WaitForExit(), Times.Once);
            mockProcess2.Verify(p => p.Start(), Times.Never);
            mockProcess2.Verify(p => p.WaitForExit(), Times.Never);
            
            Assert.Equal((int)ExitCodeMeaning.SuccessAndLinkedStop, linkedProcess.ExitCode);
        }

        [Fact]
        public void Test_ThatApplyingStackConfigToLinkedProcessWorks()
        {
            Func<string, bool> func = (s => true);
            var process1 = RunnableFactory.CreateScriptProcess("script.ps1", "hello", new []{"hello"}, func, func);
            var process2 = RunnableFactory.CreateScriptProcess("script.ps1", "world", new []{"world"}, func, func);
            var linkedProcess = new LinkedProcess(process1, process2);

            var test = new StackConfig("shortname", "bleh", "fullname", null);
            var sampleOptions = new Options();
            sampleOptions.Verbose = false;
            sampleOptions.ConfigFile = "configfile";
            sampleOptions.DryRun = true;
            
            linkedProcess.AddStackAndStageToEnvironment(test, Stage.Up);
            linkedProcess.AddOptionsToEnvironment(sampleOptions);

            var theEnvironment = linkedProcess.Environment;
            Assert.Contains("YOYO_STACK_SHORT_NAME", theEnvironment.Keys);
            Assert.Contains("YOYO_STACK_FULL_STACK_NAME", theEnvironment.Keys);
            Assert.Contains("YOYO_STAGE", theEnvironment.Keys);
            
            Assert.Equal("shortname", linkedProcess.Environment["YOYO_STACK_SHORT_NAME"]);
            Assert.Equal("fullname", linkedProcess.Environment["YOYO_STACK_FULL_STACK_NAME"]);

            Assert.Contains("YOYO_OPTION_DRYRUN", theEnvironment.Keys);
            Assert.Contains("YOYO_OPTION_VERBOSE", theEnvironment.Keys);
            Assert.Contains("YOYO_OPTION_CONFIGFILE", theEnvironment.Keys);
            
            Assert.Equal("True", linkedProcess.Environment["YOYO_OPTION_DRYRUN"]);
            Assert.Equal("False", linkedProcess.Environment["YOYO_OPTION_VERBOSE"]);
            Assert.Equal("configfile", linkedProcess.Environment["YOYO_OPTION_CONFIGFILE"]);
        }
        
        [Fact]
        public void Start_FirstProcessSuccessAndRunLinkedProcessToo()
        {
            var mockProcess = new Mock<IProcess>();
            var mockProcess2 = new Mock<IProcess>();
            
            mockProcess.Setup(p => p.ExitCode).Returns((int)ExitCodeMeaning.SuccessAndLinkedContinue);
            mockProcess2.Setup(p => p.ExitCode).Returns(34);
            
            var linkedProcess = new LinkedProcess(mockProcess.Object, mockProcess2.Object);

            linkedProcess.Start();
            linkedProcess.WaitForExit();

            mockProcess.Verify(p => p.Start(), Times.Once);
            mockProcess.Verify(p => p.WaitForExit(), Times.Once);
            mockProcess2.Verify(p => p.Start(), Times.Once);
            mockProcess2.Verify(p => p.WaitForExit(), Times.Once);
            
            Assert.Equal((int)ExitCodeMeaning.SuccessAndLinkedContinue, linkedProcess.FirstProcess.ExitCode);
            Assert.Equal(34, linkedProcess.ExitCode);
        }
    }
}