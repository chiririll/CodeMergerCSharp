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
        try
        {
            program.Run().Wait();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public async Task Run()
    {
        var mergerOptions = CodeMergingOptionsBuilder.Build(options);
        var merger = new Merger(mergerOptions);

        var result = await merger.Run(options.SolutionFile, options.Projects.ToArray());
        Console.WriteLine($"Result: {result}");
    }
}
