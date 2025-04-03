using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ToyBox.Analyzer {
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ToyBoxAnalyzerLocalizationFixProvider)), Shared]
    public class ToyBoxAnalyzerLocalizationFixProvider : CodeFixProvider {
        public sealed override ImmutableArray<string> FixableDiagnosticIds {
            get { return ImmutableArray.Create(["LOC001", "LOC002"]); }
        }

        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
            var diagnostic = context.Diagnostics.First();
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null) return;

            var node = root.FindNode(diagnostic.Location.SourceSpan);
            if (node == null) return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move to LocalizedString field",
                    createChangedDocument: c => MoveToLocalizedStringAsync(context.Document, node, c),
                    equivalenceKey: "MoveToLocalizedString"),
                diagnostic);
        }

        private string ReplaceBadChar(string s) {
            return s.Replace('.', '_').Replace(':', '_').Replace('?', '_').Replace(';', '_').Replace('!', '_').Replace('"', '_').Replace('\'', '_')
                    .Replace('-', '_').Replace(',', '_').Replace('§', '_').Replace('%', '_').Replace('&', '_').Replace('/', '_').Replace('(', '_')
                    .Replace(')', '_').Replace('=', '_').Replace('{', '_').Replace('[', '_').Replace(']', '_').Replace('}', '_').Replace('#', '_')
                    .Replace('+', '_').Replace('*', '_').Replace('<', '_').Replace('>', '_').Replace('|', '_').Replace('~', '_');
        }
        private async Task<Document> MoveToLocalizedStringAsync(Document document, SyntaxNode node, CancellationToken cancellationToken) {
            try {
                var root = await document.GetSyntaxRootAsync(cancellationToken);
                if (root == null)
                    return document;

                var classDeclaration = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
                if (classDeclaration == null)
                    return document;

                // Create a new field that holds the string literal.
                ExpressionSyntax initializerExpr = null;
                string val = null;
                if (node is LiteralExpressionSyntax literal) {
                    initializerExpr = (ExpressionSyntax)node;
                    val = literal.Token.ValueText;
                } else if (node is ArgumentSyntax argument) {
                    initializerExpr = argument.Expression;
                    val = (argument.Expression as LiteralExpressionSyntax).Token.ValueText;
                }
                // Generate a unique field name.
                var val2 = string.Join("",
                    ((val ?? "") + " Text").Split(' ')
                    .Select(s => s?.Trim())
                    .Where(s => s != null && s != "")
                    .Select(s => string.Concat(s[0].ToString().ToUpper(), new(s.Skip(1).ToArray())))) ?? $"Generated{Guid.NewGuid():N}";
                string identifier = val2.Substring(0, Math.Min(val2?.Length ?? 32, 32));
                var newProperty = PropertyDeclaration(
                                    PredefinedType(
                                        Token(SyntaxKind.StringKeyword)),
                                    Identifier(identifier))
                                .WithAttributeLists(
                                    SingletonList<AttributeListSyntax>(
                                        AttributeList(
                                            SingletonSeparatedList<AttributeSyntax>(
                                                Attribute(
                                                    IdentifierName("LocalizedString"))
                                                .WithArgumentList(
                                                    AttributeArgumentList(
                                                        SeparatedList<AttributeArgumentSyntax>(
                                                            new SyntaxNodeOrToken[]{
                                                            AttributeArgument(
                                                                LiteralExpression(
                                                                    SyntaxKind.StringLiteralExpression,
                                                                    Literal(ReplaceBadChar($"{GetNamespaceAndClassName(classDeclaration)}.{identifier}")))),
                                                            Token(SyntaxKind.CommaToken),
                                                            AttributeArgument(
                                                                initializerExpr)})))))))
                                .WithModifiers(
                                    TokenList(
                                        new[]{
                                        Token(SyntaxKind.PrivateKeyword),
                                        Token(SyntaxKind.StaticKeyword),
                                        Token(SyntaxKind.PartialKeyword)}))
                                .WithAccessorList(
                                    AccessorList(
                                        SingletonList<AccessorDeclarationSyntax>(
                                            AccessorDeclaration(
                                                SyntaxKind.GetAccessorDeclaration)
                                            .WithSemicolonToken(
                                                Token(SyntaxKind.SemicolonToken)))));
                var fieldReference = IdentifierName(identifier);
                var updatedClassDeclaration = classDeclaration.ReplaceNode(initializerExpr, fieldReference);
                var newClassDeclaration = updatedClassDeclaration.AddMembers(newProperty);
                if (!newClassDeclaration.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PartialKeyword))) {
                    newClassDeclaration = newClassDeclaration.AddModifiers(Token(SyntaxKind.PartialKeyword));
                }
                var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);

                return document.WithSyntaxRoot(newRoot);
            } catch (Exception) {
                return document;
            }
        }
        private static string GetNamespaceAndClassName(ClassDeclarationSyntax classDeclaration) {
            var className = classDeclaration.Identifier.Text;
            var namespaceDeclaration = classDeclaration.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault();
            string namespaceName = namespaceDeclaration != null ? namespaceDeclaration.Name.ToString() : "Global";
            return $"{namespaceName}.{className}";
        }
    }
}
