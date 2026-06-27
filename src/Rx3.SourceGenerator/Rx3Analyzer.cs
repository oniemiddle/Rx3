using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Rx3.SourceGenerator;

/// <summary>
/// Analyzer that flags potential resource leaks in ReactiveObject subclasses.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Rx3Analyzer : DiagnosticAnalyzer
{
    public const string MissingDisposeId = "RX3_001";
    public const string MissingAddToId = "RX3_002";

    private static readonly DiagnosticDescriptor MissingDisposeRule = new(
        id: MissingDisposeId,
        title: "ReactiveObject subclass should dispose resources",
        messageFormat: "Type '{0}' extends ReactiveObject but does not override Dispose(bool). "
                     + "Add a Dispose method or suppress this warning if resources are managed elsewhere.",
        category: "Reliability",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "ReactiveObject subclasses that acquire disposable resources should override Dispose(bool) to clean them up.");

    private static readonly DiagnosticDescriptor MissingAddToRule = new(
        id: MissingAddToId,
        title: "Subscribe result should be added to DisposableBag",
        messageFormat: "The return value of 'Subscribe' is not assigned or added to any DisposableBag. "
                     + "Use '.AddTo(ref DisposableBag)' to prevent subscription leaks.",
        category: "Reliability",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Subscription results from Observable.Subscribe() should be added to a DisposableBag to prevent memory leaks.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [MissingDisposeRule, MissingAddToRule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeDispose, SymbolKind.NamedType);
    }

    private static void AnalyzeDispose(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;
        if (namedType.TypeKind != TypeKind.Class)
            return;

        // Only check types that extend ReactiveObject
        if (!InheritsFrom(namedType, "Rx3.ReactiveObject"))
            return;

        // Skip if already has Dispose override
        if (HasDisposeOverride(namedType))
            return;

        // Check if the type has any IDisposable fields
        bool hasDisposableField = namedType.GetMembers()
            .OfType<IFieldSymbol>()
            .Any(f => f.Type.AllInterfaces.Any(i => i.ToDisplayString() == "System.IDisposable"));

        if (hasDisposableField)
        {
            var diagnostic = Diagnostic.Create(MissingDisposeRule, namedType.Locations[0], namedType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool HasDisposeOverride(INamedTypeSymbol type)
    {
        while (type is not null)
        {
            var disposeMethods = type.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.Name is "Dispose" or "Dispose" && m.DeclaredAccessibility == Accessibility.Protected);

            if (disposeMethods.Any(m =>
                m.Parameters.Length == 1 &&
                m.Parameters[0].Type.SpecialType == SpecialType.System_Boolean))
                return true;

            type = type.BaseType;
            // Don't search above ReactiveObject
            if (type?.ToDisplayString() == "Rx3.ReactiveObject" || type?.ToDisplayString() == "System.Object")
                break;
        }
        return false;
    }

    private static bool InheritsFrom(ITypeSymbol? type, string baseTypeFullName)
    {
        while (type is not null)
        {
            if (type.ToDisplayString() == baseTypeFullName)
                return true;
            type = type.BaseType;
        }
        return false;
    }
}
