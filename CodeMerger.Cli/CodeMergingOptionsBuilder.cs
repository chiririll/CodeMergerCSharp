using CodeMerger.Core;

namespace CodeMerger.Cli;

internal class CodeMergingOptionsBuilder
{
    public static CodeMergingOptions Build(Options options)
    {
        return new(options.OutputFolder);
    }
}
