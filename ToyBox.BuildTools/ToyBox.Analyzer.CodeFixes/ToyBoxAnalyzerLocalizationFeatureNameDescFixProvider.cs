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
    public class ToyBoxAnalyzerLocalizationFeatureNameDescFixProvider : CodeFixProvider {
        public sealed override ImmutableArray<string> FixableDiagnosticIds {
            get { return ["LOC004"]; }
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

            if (context.Document is null || context.Document is SourceGeneratedDocument)
                return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Add [LocalizedString] to Name/Description",
                    createChangedDocument: c => LocalizeNameAndDescriptionAsync(context.Document, node, c),
                    equivalenceKey: "LocalizeNameDescription"),
                diagnostic);
        }

        private string ReplaceBadChar(string s) {
            return s.Replace('.', '_').Replace(':', '_').Replace('?', '_').Replace(';', '_').Replace('!', '_').Replace('"', '_').Replace('\'', '_')
                    .Replace('-', '_').Replace(',', '_').Replace('§', '_').Replace('%', '_').Replace('&', '_').Replace('/', '_').Replace('(', '_')
                    .Replace(')', '_').Replace('=', '_').Replace('{', '_').Replace('[', '_').Replace(']', '_').Replace('}', '_').Replace('#', '_')
                    .Replace('+', '_').Replace('*', '_').Replace('<', '_').Replace('>', '_').Replace('|', '_').Replace('~', '_');
        }
        private async Task<Document> LocalizeNameAndDescriptionAsync(Document document, SyntaxNode node, CancellationToken cancellationToken) {
            try {
                var root = await document.GetSyntaxRootAsync(cancellationToken);
                if (root == null)
                    return document;

                var classDecl = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
                if (classDecl == null)
                    return document;

                var members = classDecl.Members;
                var updatedClass = classDecl;
                foreach (var propName in new[] { "Name", "Description" }) {
                    var prop = updatedClass.Members
                        .OfType<PropertyDeclarationSyntax>()
                        .FirstOrDefault(p => p.Identifier.Text == propName);
                    if (prop == null) continue;

                    var hasAttr = prop.AttributeLists
                        .SelectMany(al => al.Attributes)
                        .Any(a => a.Name.ToString().Contains("LocalizedString"));
                    if (hasAttr) continue;

                    var indent = prop.GetLeadingTrivia().Where(t => t.IsKind(SyntaxKind.WhitespaceTrivia)).ToSyntaxTriviaList();

                    var ns = GetNamespaceAndClassName(updatedClass);
                    var key = $"{ns}.{propName}";
                    var attr = AttributeList(
                    SingletonSeparatedList(
                        Attribute(
                            IdentifierName("LocalizedString"))
                        .WithArgumentList(
                            AttributeArgumentList(
                                SeparatedList<AttributeArgumentSyntax>(
                                    new SyntaxNodeOrToken[]{
                                        AttributeArgument(
                                            LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                Literal(ReplaceBadChar($"{GetNamespaceAndClassName(classDecl)}.{propName}")))),
                                        Token(SyntaxKind.CommaToken),
                                        AttributeArgument(
                                            LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                Literal($"Default {propName}")))})))))
                        .WithLeadingTrivia(indent)
                        .WithTrailingTrivia(TriviaList(LineFeed));
                    var newProperty = PropertyDeclaration(prop.Type, prop.Identifier)
                                .WithAttributeLists(SingletonList(attr))
                                .WithModifiers(
                                    TokenList(
                                        [
                                        Token(SyntaxKind.PublicKeyword),
                                        Token(SyntaxKind.OverrideKeyword),
                                        Token(SyntaxKind.PartialKeyword)
                                        ]))
                                .WithAccessorList(
                                    AccessorList(
                                        SingletonList(
                                            AccessorDeclaration(
                                                SyntaxKind.GetAccessorDeclaration)
                                            .WithSemicolonToken(
                                                Token(SyntaxKind.SemicolonToken)))))
                                .WithTrailingTrivia(TriviaList(LineFeed));

                    updatedClass = updatedClass.ReplaceNode(prop, newProperty);
                }
                if (!updatedClass.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PartialKeyword))) {
                    var mods = TokenList(updatedClass.Modifiers.Select(t => t.IsKind(SyntaxKind.InternalKeyword) ? Token(SyntaxKind.PublicKeyword) : t));
                    mods = mods.Add(Token(SyntaxKind.PartialKeyword));
                    updatedClass = updatedClass.WithModifiers(mods);
                }
                var newRoot = root.ReplaceNode(classDecl, updatedClass);

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
