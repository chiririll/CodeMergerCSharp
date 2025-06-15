using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeMerger.Core;

internal class ProjectMerger
{
    private readonly Project project;
    private readonly Dictionary<string, NamespaceInfo> namespaces = new();
    private readonly Dictionary<string, UsingDirectiveSyntax> usings = new();

    public ProjectMerger(Project project)
    {
        this.project = project;
    }

    public async Task<string> Merge()
    {
        var definedSymbols = ((CSharpParseOptions)project.ParseOptions!).PreprocessorSymbolNames;

        var commentRemover = new CommentRemovalRewriter();
        var conditionalRemover = new ConditionalDirectiveRemover(definedSymbols);

        foreach (var document in project.Documents)
        {
            var node = await document.GetSyntaxRootAsync();
            node = commentRemover.Visit(node);
            node = conditionalRemover.Visit(node);

            if (node == null)
                continue;

            ProcessUsings(node);
            ProcessNamespaces(node);
        }

        var namespaces = this.namespaces.ToList();
        namespaces.Sort((a, b) => a.Key.CompareTo(b.Key));

        var nsList = new SyntaxList<MemberDeclarationSyntax>(
            namespaces.Select(ns => SyntaxFactory.NamespaceDeclaration(
                ns.Value.Syntax.Name,
                [], [],
                ns.Value.GetMembers()))
        );

        var usings = this.usings.ToList();
        usings.Sort((a, b) => a.Key.CompareTo(b.Key));

        var unit = SyntaxFactory.CompilationUnit([], new(usings.Select(i => i.Value)), [], nsList);

        return unit.NormalizeWhitespace().ToFullString();
    }

    private void ProcessUsings(SyntaxNode root)
    {
        var nodes = root.DescendantNodes().OfType<UsingDirectiveSyntax>();
        foreach (var node in nodes)
        {
            if (node.Name == null)
            {
                continue;
            }

            var name = node.Name.ToString();
            if (!usings.ContainsKey(name))
            {
                usings.Add(name, node);
            }
        }
    }

    private void ProcessNamespaces(SyntaxNode root)
    {
        var nodes = root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>();

        foreach (var ns in nodes)
        {
            var name = ns.Name.ToString();
            if (!namespaces.TryGetValue(name, out var info))
            {
                info = new(ns);
                namespaces.Add(name, info);
            }
            info.AddMembers(ns.Members);
        }
    }
}
