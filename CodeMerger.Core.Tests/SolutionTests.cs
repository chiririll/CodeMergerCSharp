namespace CodeMerger.Core.Tests;

public class SolutionTests
{
    private const string InvalidSolutionPath = @"/";
    private const string ValidSolutionPath = @"CodeMerger.sln";
    private const string OutputPath = @"out/";

    private readonly Merger merger;

    public SolutionTests()
    {
        var options = new CodeMergingOptions(OutputPath);
        merger = new Merger(options);
    }

    [Fact]
    public void TestInvalidSolutionPath()
    {
        var task = merger.Run(InvalidSolutionPath);
        task.Wait();

        Assert.Equal(MergeResult.FailedOpenSolution, task.Result);
    }

    [Fact]
    public void TestValidSolutionPath()
    {
        var task = merger.Run(ValidSolutionPath);
        task.Wait();

        Assert.NotEqual(MergeResult.FailedOpenSolution, task.Result);
    }
}
