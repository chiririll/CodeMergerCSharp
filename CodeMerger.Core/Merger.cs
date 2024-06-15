using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace CodeMerger.Core;

public class Merger
{
    private readonly CodeMergingOptions options;

    public Merger(CodeMergingOptions options)
    {
        this.options = options;
    }

    public async Task<MergeResult> Run(string solutionPath, params string[] projectNames)
    {
        var workspace = MSBuildWorkspace.Create();

        var solution = await OpenSolution(workspace, solutionPath);
        if (solution == null)
        {
            return MergeResult.FailedOpenSolution;
        }

        Directory.CreateDirectory(options.OutputPath);

        var projects = projectNames.Length > 0
            ? solution.Projects.Where(p => projectNames.Contains(p.Name))
            : solution.Projects;

        foreach (var project in projects)
        {
            var merger = new ProjectMerger(project);

            var result = await merger.Merge();
            result = RepeatedSpace().Replace(result.Replace("\r", "").Replace("\n", ""), " ");

            File.WriteAllText(Path.Join(options.OutputPath, project.Name + ".cs"), result, System.Text.Encoding.UTF8);
        }

        return MergeResult.Success;
    }


    private async Task<Solution?> OpenSolution(MSBuildWorkspace workspace, string path)
    {
        try
        {
            return await workspace.OpenSolutionAsync(path);
        }
        catch (Exception e)
        {
            Trace.TraceError("Failed to open solution at path '{0}': {1}", path, e.Message);
            return null;
        }
    }

    [GeneratedRegex(" +")]
    private static partial Regex RepeatedSpace();
}
