using config;
using Microsoft.AspNetCore.Mvc.TagHelpers;
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
        StackConfig test = new StackConfig("shortname", "bleh", "fullname", null);
        var t = RunnableFactory.CreatePulumiProcess(
            workingDirectory,
            new string[] { "ponies" }, (msg) => true, (string msg) => true);
        Assert.Equal(workingDirectory, t.WorkingDirectory);
    }

    [Fact]
    public void RunPulumiProcessWithConsole_ShouldStartProcess_WhenCalledWithValidWrapper()
    {
        // Arrange
        var mockProcess = new Mock<IProcess>();
        var wrapper = new RunnableFactory.ProcessWrapper(mockProcess.Object, true);

        // Act
        wrapper.process.Start();
        wrapper.process.WaitForExit();

        // Assert
        mockProcess.Verify(p => p.Start(), Times.Once);
        mockProcess.Verify(p => p.WaitForExit(), Times.Once);

    }
    
    [Fact]
    public void RunPulumiProcessWithConsole_ShouldNotWaitForExit_WhenCalledWithWrapperThatDoesNotWaitForExit()
    {
        // Arrange
        var mockProcess = new Mock<IProcess>();
        var wrapper = new RunnableFactory.ProcessWrapper(mockProcess.Object, false);

        // Act
        wrapper.process.Start();

        // Assert
        mockProcess.Verify(p => p.WaitForExit(), Times.Never);
    }
}