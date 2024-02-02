using System.Diagnostics;
using Moq;
using pulumi_yoyo;

namespace unittests;

public class ProcessTests
{
    [InlineData(".", true)]
    [InlineData(".", false)]
    [InlineData("somewhere", false)]
    [Theory]
    public void Test_CreateProcess(string workingDirectory, bool shouldWait)
    {
        var t = PulumiRunFactory.CreateViaProcess(
            workingDirectory,
            new string[] { "up", "--skip-preview" }, (msg) => true, (string msg) => true, waitForExit: shouldWait);
        Assert.Equal(shouldWait, t.waitForExit);
        Assert.Equal("pulumi", t.process.StartInfo.FileName);
        Assert.Equal("up --skip-preview", t.process.StartInfo.Arguments);
        Assert.Equal(workingDirectory, t.process.StartInfo.WorkingDirectory);
    }

    [Fact]
    public void RunPulumiProcessWithConsole_ShouldStartProcess_WhenCalledWithValidWrapper()
    {
        // Arrange
        var mockProcess = new Mock<IProcess>();
        var wrapper = new PulumiRunFactory.ProcessWrapper(mockProcess.Object, true);

        // Act
        PulumiRunFactory.RunPulumiProcessWithConsole(wrapper);

        // Assert
        mockProcess.Verify(p => p.Start(), Times.Once);
        mockProcess.Verify(p => p.WaitForExit(), Times.Once);

    }
    
    [Fact]
    public void RunPulumiProcessWithConsole_ShouldNotWaitForExit_WhenCalledWithWrapperThatDoesNotWaitForExit()
    {
        // Arrange
        var mockProcess = new Mock<IProcess>();
        var wrapper = new PulumiRunFactory.ProcessWrapper(mockProcess.Object, false);

        // Act
        PulumiRunFactory.RunPulumiProcessWithConsole(wrapper);

        // Assert
        mockProcess.Verify(p => p.WaitForExit(), Times.Never);
    }
}