using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ToyBox.Analyzer {
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ToyBoxAnalyzerCodeFixProvider)), Shared]
    public class ToyBoxAnalyzerCodeFixProvider : CodeFixProvider {
        public sealed override ImmutableArray<string> FixableDiagnosticIds {
            get { return ImmutableArray.Create([ToyBoxAnalyzer.DiagnosticId, ToyBoxAnalyzer.DiagnosticId2]); }
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
        // TODO: Hide info
        // TODO: Replace literal with reference to newly created symbol
        private async Task<Document> MoveToLocalizedStringAsync(Document document, SyntaxNode node, CancellationToken cancellationToken) {
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
            var val2 = "m_" + string.Join("", 
                ((val ?? "") + " Text").Split(' ')
                .Select(s => s?.Trim())
                .Where(s => s != null && s != "")
                .Select(s => string.Concat(s[0].ToString().ToUpper(), new(s.Skip(1).ToArray()))));
            string identifier = (val2 ?? $"m_Generated{Guid.NewGuid():N}").Substring(0, Math.Min(val2?.Length ?? 16, 16));

            var newField = FieldDeclaration(VariableDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)))
                            .WithVariables(SingletonSeparatedList<VariableDeclaratorSyntax>(
                                VariableDeclarator(
                                    Identifier(
                                        identifier))
                                .WithInitializer(
                                    EqualsValueClause(
                                        initializerExpr)))))
                        .WithAttributeLists(
                            SingletonList<AttributeListSyntax>(
                                AttributeList(
                                    SingletonSeparatedList<AttributeSyntax>(
                                        Attribute(
                                            IdentifierName("LocalizedString"))
                                        .WithArgumentList(
                                            AttributeArgumentList(
                                                SingletonSeparatedList<AttributeArgumentSyntax>(
                                                    AttributeArgument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal($"{GetNamespaceAndClassName(classDeclaration)}.{identifier}"))))))))))
                        .WithModifiers(
                            TokenList(
                                [
                                    Token(SyntaxKind.PrivateKeyword),
                                    Token(SyntaxKind.StaticKeyword)
                                ]));
            var fieldReference = IdentifierName(identifier);
            var updatedClassDeclaration = classDeclaration.ReplaceNode(initializerExpr, fieldReference);
            var newClassDeclaration = updatedClassDeclaration.AddMembers(newField);
            var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }
        private static string GetNamespaceAndClassName(ClassDeclarationSyntax classDeclaration) {
            var className = classDeclaration.Identifier.Text;
            var namespaceDeclaration = classDeclaration.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault();
            string namespaceName = namespaceDeclaration != null ? namespaceDeclaration.Name.ToString() : "Global";
            return $"{namespaceName}.{className}";
        }
    }
}
