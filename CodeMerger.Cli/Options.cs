using System.Diagnostics.CodeAnalysis;
using CommandLine;

internal sealed class Options
{
    [Option('s', "sln", Required = true, HelpText = "Solution file")]
    public string SolutionFile { get; private set; } = string.Empty;

    [Option('p', "project", HelpText = "Project name", Separator = ','), AllowNull]
    public IEnumerable<string> Projects { get; private set; }

    [Option('o', "output", Required = true, HelpText = "Output folder")]
    public string OutputFolder { get; private set; } = string.Empty;
}
