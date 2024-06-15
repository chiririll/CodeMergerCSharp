using CodeMerger.Core;

namespace CodeMerger.Cli;

internal sealed class Program
{
    private readonly Options options;

    internal Program(Options options)
    {
        this.options = options;
    }

    public static void Main(string[] args)
    {
        var options = CommandLine.Parser.Default.ParseArguments<Options>(args);
        if (options.Tag != CommandLine.ParserResultType.Parsed)
        {
            return;
        }

        var program = new Program(options.Value);
        program.Run();
    }

    public void Run()
    {
        var mergerOptions = CodeMergingOptionsBuilder.Build(options);
        var merger = new Merger(mergerOptions);

        merger.Run(options.SolutionFile, options.Projects.ToArray()).Wait();
    }
}
