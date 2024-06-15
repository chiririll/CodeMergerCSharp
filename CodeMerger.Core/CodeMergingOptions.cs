namespace CodeMerger.Core;

public class CodeMergingOptions
{
    public CodeMergingOptions(string outputPath)
    {
        OutputPath = outputPath;
    }

    public string OutputPath { get; }
}
