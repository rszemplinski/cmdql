using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace QL.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CommandAttributeAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "CommandAttributeAnalyzer";
    private static readonly LocalizableString Title = "Missing Command Attribute or ExecuteCommand method";
    private static readonly LocalizableString MessageFormat = "The struct {0} does not have a Cmd attribute or an ExecuteCommand method.";
    private static readonly LocalizableString Description = "All structs should have a Cmd attribute or an ExecuteCommand method.";
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

    public override void Initialize(AnalysisContext context)
    {
        // You must call this method to avoid analyzing generated code.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        // You must call this method to enable the Concurrent Execution.
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    /// <summary>
    /// Executed for each Syntax Node with 'SyntaxKind' is 'ClassDeclaration'.
    /// </summary>
    /// <param name="context">Operation context.</param>
    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        // Check if the struct has the Cmd attribute or the ExecuteCommand method
        var hasCmdAttribute = namedTypeSymbol.GetAttributes().Any(a => a.AttributeClass.Name == "CmdAttribute");
        var hasExecuteCommandMethod = namedTypeSymbol.GetMembers("ExecuteCommand").Any();

        if (!hasCmdAttribute && !hasExecuteCommandMethod)
        {
            var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}