using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeMerger.Core;

internal class CommentRemovalRewriter : CSharpSyntaxRewriter
{
    public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
    {
        // Remove comments and region directives
        if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
            trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
            trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
            trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia) ||
            trivia.IsKind(SyntaxKind.RegionDirectiveTrivia) ||
            trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
        {
            return default;
        }

        return base.VisitTrivia(trivia);
    }

    public override SyntaxNode? Visit(SyntaxNode? node)
    {
        if (node == null)
            return null;

        node = base.Visit(node);

        // Clean up trivia
        var newNode = node
            .WithLeadingTrivia(RemoveUnwantedTrivia(node.GetLeadingTrivia()))
            .WithTrailingTrivia(RemoveUnwantedTrivia(node.GetTrailingTrivia()));

        return newNode;
    }

    private static SyntaxTriviaList RemoveUnwantedTrivia(SyntaxTriviaList triviaList)
    {
        return new SyntaxTriviaList(triviaList.Where(t =>
            !t.IsKind(SyntaxKind.SingleLineCommentTrivia) &&
            !t.IsKind(SyntaxKind.MultiLineCommentTrivia) &&
            !t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) &&
            !t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia) &&
            !t.IsKind(SyntaxKind.RegionDirectiveTrivia) &&
            !t.IsKind(SyntaxKind.EndRegionDirectiveTrivia)));
    }
}
