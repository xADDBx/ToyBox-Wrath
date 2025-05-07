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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ToyBoxAnalyzerPatchFeatureFixProvider)), Shared]
    public class ToyBoxAnalyzerPatchFeatureFixProvider : CodeFixProvider {
        public sealed override ImmutableArray<string> FixableDiagnosticIds {
            get { return ImmutableArray.Create(["HAR001", "HAR002"]); }
        }

        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context) {
            if (context.Document is null || context.Document is SourceGeneratedDocument)
                return Task.CompletedTask;

            var diagnostic = context.Diagnostics.First();
            string title = diagnostic.Id == "HAR001"
                ? "Add missing Harmony attributes"
                : "Add/fix HarmonyName property";
            context.RegisterCodeFix(
                CodeAction.Create(
                    title,
                    c => ApplyFixAsync(context.Document, diagnostic, c),
                    equivalenceKey: title),
                diagnostic);
            return Task.CompletedTask;
        }

        private async Task<Document> ApplyFixAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken) {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            // Determine if the diagnostic is for attributes or property.
            if (diagnostic.Id == "HAR001") {
                // Get the class declaration.
                var classDecl = root.FindNode(diagnostic.Location.SourceSpan) as ClassDeclarationSyntax
                    ?? root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf()
                           .OfType<ClassDeclarationSyntax>().FirstOrDefault();
                if (classDecl == null)
                    return document;

                // Get full type name.
                var classSymbol = semanticModel.GetDeclaredSymbol(classDecl, cancellationToken);
                string fullName = classSymbol.ToDisplayString();

                // Create attributes:
                // [HarmonyPatch]
                var harmonyPatchAttr = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("HarmonyPatch"));
                // [HarmonyPatchCategory("FullName")]
                var harmonyPatchCategoryAttr = SyntaxFactory.Attribute(
                    SyntaxFactory.IdentifierName("HarmonyPatchCategory"),
                    SyntaxFactory.AttributeArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.AttributeArgument(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal(fullName))))));

                // Create a single attribute list containing both attributes.
                var newAttrList = SyntaxFactory.AttributeList(
                    SyntaxFactory.SeparatedList(new[] { harmonyPatchAttr, harmonyPatchCategoryAttr }));

                // Create a new list for the attribute lists.
                var oldAttrLists = classDecl.AttributeLists;
                var newAttrLists = SyntaxFactory.List<AttributeListSyntax>();

                // Remove any existing HarmonyPatch or HarmonyPatchCategory attributes.
                foreach (var list in oldAttrLists) {
                    // Filter out attributes that end with "HarmonyPatch" or "HarmonyPatchCategory".
                    var remainingAttributes = list.Attributes
                        .Where(attr => !(attr.Name.ToString().EndsWith("HarmonyPatch") ||
                                         attr.Name.ToString().EndsWith("HarmonyPatchCategory")))
                        .ToList();

                    if (remainingAttributes.Any()) {
                        // Replace the list with one that only has the remaining attributes.
                        newAttrLists = newAttrLists.Add(list.WithAttributes(SyntaxFactory.SeparatedList(remainingAttributes)));
                    }
                }

                // Add our new attribute list.
                newAttrLists = newAttrLists.Add(newAttrList);

                // Replace the class declaration with the new attribute lists.
                var newClassDecl = classDecl.WithAttributeLists(newAttrLists);
                var newRoot = root.ReplaceNode(classDecl, newClassDecl);
                return document.WithSyntaxRoot(newRoot);
            } else if (diagnostic.Id == "HAR002") {
                // Get the class declaration.
                var classDecl = root.FindNode(diagnostic.Location.SourceSpan) as ClassDeclarationSyntax
                    ?? root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf()
                           .OfType<ClassDeclarationSyntax>().FirstOrDefault();
                if (classDecl == null)
                    return document;

                // Get full type name.
                var classSymbol = semanticModel.GetDeclaredSymbol(classDecl, cancellationToken);
                string fullName = classSymbol.ToDisplayString();

                // Find an existing HarmonyName property (if any).
                var existingProp = classDecl.Members.OfType<PropertyDeclarationSyntax>()
                    .FirstOrDefault(p => p.Identifier.Text == "HarmonyName");

                if (existingProp != null) {
                    // Replace the getter with one that returns the correct literal.
                    var newLiteral = SyntaxFactory.LiteralExpression(
                        SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(fullName));

                    // If the property uses an expression body.
                    if (existingProp.ExpressionBody != null) {
                        var newProp = existingProp.WithExpressionBody(
                            SyntaxFactory.ArrowExpressionClause(newLiteral));
                        var newRoot = root.ReplaceNode(existingProp, newProp);
                        return document.WithSyntaxRoot(newRoot);
                    } else if (existingProp.AccessorList != null) {
                        // Find the getter.
                        var getter = existingProp.AccessorList.Accessors
                            .FirstOrDefault(a => a.Kind() == SyntaxKind.GetAccessorDeclaration);
                        if (getter != null) {
                            // Create a new getter body that returns the correct literal.
                            var returnStmt = SyntaxFactory.ReturnStatement(newLiteral);
                            var newGetter = getter.WithBody(SyntaxFactory.Block(returnStmt))
                                                  .WithExpressionBody(null)
                                                  .WithSemicolonToken(default);
                            var newAccessorList = existingProp.AccessorList.ReplaceNode(getter, newGetter);
                            var newProp = existingProp.WithAccessorList(newAccessorList);
                            var newRoot = root.ReplaceNode(existingProp, newProp);
                            return document.WithSyntaxRoot(newRoot);
                        }
                    }
                } else {
                    // No HarmonyName property exists, so add one.
                    // Create: protected override string HarmonyName => "FullName";
                    var propDeclaration = SyntaxFactory.PropertyDeclaration(
                        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                        "HarmonyName")
                        .AddModifiers(
                            SyntaxFactory.Token(SyntaxKind.ProtectedKeyword),
                            SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
                        .WithExpressionBody(
                            SyntaxFactory.ArrowExpressionClause(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal(fullName))))
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                    // Add the new property to the end of the class.
                    var newClassDecl = classDecl.AddMembers(propDeclaration);
                    var newRoot = root.ReplaceNode(classDecl, newClassDecl);
                    return document.WithSyntaxRoot(newRoot);
                }
            }

            return document;
        }
    }
}
