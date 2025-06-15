using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CodeMerger.Core;

internal class ConditionalDirectiveRemover : CSharpSyntaxRewriter
{
    private readonly HashSet<string> definedSymbols;

    public ConditionalDirectiveRemover(IEnumerable<string> definedSymbols)
    {
        this.definedSymbols = new HashSet<string>(definedSymbols);
    }

    public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
    {
        if (!trivia.IsDirective)
            return base.VisitTrivia(trivia);

        var directive = trivia.GetStructure();
        if (directive is IfDirectiveTriviaSyntax ifDir && !ShouldInclude(ifDir))
        {
            // Remove the entire inactive block
            var span = GetConditionalBlockSpan(ifDir);
            return default;
        }

        if (directive is ElseDirectiveTriviaSyntax ||
            directive is ElifDirectiveTriviaSyntax ||
            directive is EndIfDirectiveTriviaSyntax)
        {
            // Always remove these after evaluating the main condition
            return default;
        }

        return base.VisitTrivia(trivia);
    }

    public override SyntaxNode? Visit(SyntaxNode? node)
    {
        if (node == null)
            return null;

        // Remove conditional code blocks as trivia
        var newTrivia = node.DescendantTrivia()
            .Where(t => !IsInactiveDirective(t))
            .ToSyntaxTriviaList();

        var cleanedNode = base.Visit(node);
        return cleanedNode?.WithLeadingTrivia(RemoveUnwantedTrivia(cleanedNode.GetLeadingTrivia()))
                          .WithTrailingTrivia(RemoveUnwantedTrivia(cleanedNode.GetTrailingTrivia()));
    }

    private bool IsInactiveDirective(SyntaxTrivia trivia)
    {
        if (!trivia.IsDirective)
            return false;

        var directive = trivia.GetStructure();
        if (directive is IfDirectiveTriviaSyntax ifDir)
        {
            return !ShouldInclude(ifDir);
        }

        return directive is ElseDirectiveTriviaSyntax ||
               directive is ElifDirectiveTriviaSyntax ||
               directive is EndIfDirectiveTriviaSyntax;
    }

    private static SyntaxTriviaList RemoveUnwantedTrivia(SyntaxTriviaList list)
    {
        return new SyntaxTriviaList(list.Where(t =>
            !t.IsKind(SyntaxKind.IfDirectiveTrivia) &&
            !t.IsKind(SyntaxKind.ElseDirectiveTrivia) &&
            !t.IsKind(SyntaxKind.ElifDirectiveTrivia) &&
            !t.IsKind(SyntaxKind.EndIfDirectiveTrivia)));
    }

    private bool ShouldInclude(IfDirectiveTriviaSyntax directive)
    {
        var condition = directive.Condition.ToString().Trim();

        // Handle only simple symbols (no expressions like &&, ||)
        return definedSymbols.Contains(condition);
    }

    private TextSpan GetConditionalBlockSpan(IfDirectiveTriviaSyntax ifDirective)
    {
        var related = ifDirective.GetRelatedDirectives();
        var end = related.OfType<EndIfDirectiveTriviaSyntax>().FirstOrDefault();

        return end != null
            ? TextSpan.FromBounds(ifDirective.FullSpan.Start, end.FullSpan.End)
            : ifDirective.FullSpan;
    }
}
