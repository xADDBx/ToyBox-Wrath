using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ToyBox.Generator {
    [Generator]
    public class LocalizationGenerator : IIncrementalGenerator {
        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var localizedProperties = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, ct) => IsCandidate(node),
                transform: static (ctx, ct) => GetLocalizedProperty(ctx))
                .Where(static m => m is not null);
            var compilationAndProperties = context.CompilationProvider.Combine(localizedProperties.Collect());
            context.RegisterSourceOutput(compilationAndProperties, (spc, source) => {
                Execute(source.Left, source.Right, spc);
            });
        }

        private static bool IsCandidate(SyntaxNode node) {
            return node is PropertyDeclarationSyntax propertyDeclaration &&
                   propertyDeclaration.AttributeLists.Count > 0;
        }

        private static LocalizedProperty? GetLocalizedProperty(GeneratorSyntaxContext context) {
            if (context.Node is not PropertyDeclarationSyntax propertyDeclaration)
                return null;

            if (!propertyDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)) {
                return null;
            }
            var semanticModel = context.SemanticModel;
            var propSymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration) as IPropertySymbol;
            if (propSymbol == null)
                return null;
            foreach (var attribute in propSymbol.GetAttributes()) {
                var attributeName = attribute.AttributeClass?.ToDisplayString();
                if (attributeName is not null && (attributeName == "LocalizedString" || attributeName == "LocalizedStringAttribute" ||
                    attributeName.EndsWith(".LocalizedString") || attributeName.EndsWith(".LocalizedStringAttribute"))) {
                    if (attribute.ConstructorArguments.Length == 2) {
                        var keyArg = attribute.ConstructorArguments[0];
                        var defaultArg = attribute.ConstructorArguments[1];
                        if (keyArg.Value is string keyValue && defaultArg.Value is string defaultValue) {
                            return new LocalizedProperty {
                                PropertySymbol = propSymbol,
                                LocalizationKey = keyValue,
                                DefaultValue = defaultValue,
                                IsStatic = propertyDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword),
                                ClassSyntax = propertyDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>()!,
                                PropertyString = propertyDeclaration.WithAccessorList(null).WithInitializer(null).WithAttributeLists([]).ToString()
                            };
                        }
                    }
                }
            }

            return null;
        }

        private static void Execute(Compilation compilation, ImmutableArray<LocalizedProperty?> localizedProperties, SourceProductionContext context) {
            var properties = localizedProperties.Where(p => p is not null).Select(p => p!).ToList();
            if (properties.Count == 0) {
                return;
            }

            Dictionary<string, string> localizationEntries = new();
            foreach (var prop in properties) {
                var fieldName = prop.LocalizationKey;
                if (!localizationEntries.ContainsKey(fieldName)) {
                    localizationEntries.Add(fieldName, prop.DefaultValue);
                }
            }

            var languageSource = GenerateLanguageClass(localizationEntries);
            context.AddSource("Language.g.cs", SourceText.From(languageSource, Encoding.UTF8));

            var groupedByType = properties.GroupBy<LocalizedProperty, (INamedTypeSymbol, ClassDeclarationSyntax)>(p => (p.PropertySymbol.ContainingType, p.ClassSyntax));
            foreach (var group in groupedByType) {
                var typeSource = GeneratePartialProperties(group.Key.Item1, group.Key.Item2, group);
                var hintName = $"{group.Key.Item1.ContainingNamespace.ToDisplayString().Replace(".", "_")}_{group.Key.Item1.Name}.g.cs";
                context.AddSource(hintName, SourceText.From(typeSource, Encoding.UTF8));
            }
        }

        private static string GenerateLanguageClass(Dictionary<string, string> localizationEntries) {
            var sb = new StringBuilder();
            sb.AppendLine("namespace ToyBox.Infrastructure.Localization;");
            sb.AppendLine("public partial class Language {");
            foreach (var entry in localizationEntries) {
                sb.AppendLine($"    public (string Original, string Translated) {entry.Key} = (@\"{entry.Value}\", @\"{entry.Value}\");");
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string GeneratePartialProperties(INamedTypeSymbol containingType, ClassDeclarationSyntax syntax, IEnumerable<LocalizedProperty> properties) {
            var sb = new StringBuilder();
            var ns = containingType.ContainingNamespace.ToDisplayString();

            HashSet<string> usings = new();
            var compilationUnit = syntax.SyntaxTree.GetCompilationUnitRoot();
            string curUsing;
            foreach (var usingDirective in compilationUnit.Usings) {
                curUsing = usingDirective.ToFullString().Trim();
                usings.Add(curUsing);
                sb.AppendLine(curUsing);
            }
            curUsing = "using ToyBox.Infrastructure.Localization;";
            if (!usings.Contains(curUsing)) {
                sb.AppendLine(curUsing);
            }
            sb.AppendLine($"");
            if (!string.IsNullOrEmpty(ns)) {
                sb.AppendLine($"namespace {ns};");
            }
            sb.AppendLine($"{syntax.WithAttributeLists([]).WithMembers([]).WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.None)).WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.None))} {{");
            foreach (var prop in properties) {
                var propName = prop.PropertySymbol.Name;
                var generatedFieldName = prop.LocalizationKey;
                sb.AppendLine($"    {prop.PropertyString} => LocalizationManager.CurrentLocalization.{generatedFieldName}.Translated;");
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

        private class LocalizedProperty {
            public IPropertySymbol PropertySymbol { get; set; } = default!;
            public string LocalizationKey { get; set; } = "";
            public string DefaultValue { get; set; } = "";
            public bool IsStatic { get; set; } = true;
            public ClassDeclarationSyntax ClassSyntax { get; set; } = default!;
            public string PropertyString { get; set; } = "";
        }
    }
}
