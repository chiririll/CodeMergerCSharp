using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeMerger.Core;

internal class NamespaceInfo
{
    private readonly List<MemberDeclarationSyntax> members = new();

    public NamespaceInfo(BaseNamespaceDeclarationSyntax ns)
    {
        Name = ns.Name.ToString();
        Syntax = ns;
    }

    public string Name { get; }
    public BaseNamespaceDeclarationSyntax Syntax { get; }

    public void AddMembers(SyntaxList<MemberDeclarationSyntax> members) => this.members.AddRange(members);
    public SyntaxList<MemberDeclarationSyntax> GetMembers() => new(members);
}
