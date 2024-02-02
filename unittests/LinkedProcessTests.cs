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
            
            Assert.Equal((int)ExitCodeMeaning.SuccessAndLinkedContinue, linkedProcess.ExitCode);
            Assert.Equal(34, linkedProcess.ThenProcess.ExitCode);
        }
    }
}