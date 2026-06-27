using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Rx3.SourceGenerator;

/// <summary>
/// Analyzer that flags IDisposable members not disposed in Dispose methods.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Rx3Analyzer : DiagnosticAnalyzer
{
    private const string MissingDisposeId = "RX3_001";

    private static readonly DiagnosticDescriptor MissingDisposeRule = new(
        id: MissingDisposeId,
        title: "IDisposable field is not disposed",
        messageFormat: "Field '{0}' implements IDisposable but is not disposed in Dispose(). "
                     + "Add '{1}.Dispose()' or '{1}.AddTo(ref DisposableBag)' in Dispose.",
        category: "Reliability",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Fields implementing IDisposable should be explicitly disposed in the Dispose method.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [MissingDisposeRule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
    }

    private static void AnalyzeType(SymbolAnalysisContext ctx)
    {
        var type = (INamedTypeSymbol)ctx.Symbol;

        if (type.TypeKind != TypeKind.Class)
            return;

        if (!InheritsFrom(type, "Rx3.ReactiveObject"))
            return;

        var disposableMembers = GetDisposableMembers(type);

        if (disposableMembers.Count == 0)
            return;

        var handled = GetHandledMembers(type);

        foreach (var member in disposableMembers)
        {
            if (!handled.Contains(member.Name))
                Report(ctx, member);
        }
    }

    private static void Report(SymbolAnalysisContext ctx, ISymbol member)
    {
        ctx.ReportDiagnostic(
            Diagnostic.Create(
                MissingDisposeRule,
                member.Locations[0],
                member.Name,
                member.Name));
    }

    private static bool ImplementsIDisposable(ITypeSymbol type)
    {
        return type.SpecialType == SpecialType.System_IDisposable
               || type.AllInterfaces.Any(i => i.SpecialType == SpecialType.System_IDisposable);
    }

    private static bool InheritsFrom(ITypeSymbol? type, string baseType)
    {
        while (type is not null)
        {
            if (type.ToDisplayString() == baseType) return true;
            type = type.BaseType;
        }
        return false;
    }
    
    private static HashSet<string> GetHandledMembers(INamedTypeSymbol type)
    {
        var handled = new HashSet<string>();

        foreach (var syntaxRef in type.DeclaringSyntaxReferences)
        {
            if (syntaxRef.GetSyntax() is not ClassDeclarationSyntax classDecl)
                continue;

            CollectDisposeCalls(classDecl, handled);
            CollectDirectAddToCalls(classDecl, handled);
            CollectAssignmentAddToCalls(classDecl, handled);
        }

        return handled;
    }

    #region Collector

    private static void CollectDisposeCalls(
        ClassDeclarationSyntax classDecl,
        HashSet<string> handled)
    {
        foreach (var invoke in classDecl.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (!TryGetMethodCall(invoke, out var method, out var receiver))
                continue;

            if (method != "Dispose")
                continue;

            if (TryGetMemberName(receiver, out var name))
                handled.Add(name);
        }
    }
    
    private static void CollectDirectAddToCalls(
        ClassDeclarationSyntax classDecl,
        HashSet<string> handled)
    {
        foreach (var invoke in classDecl.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (!TryGetMethodCall(invoke, out var method, out var receiver))
                continue;

            if (method != "AddTo")
                continue;

            if (TryGetMemberName(receiver, out var name))
                handled.Add(name);
        }
    }
    
    private static void CollectAssignmentAddToCalls(
        ClassDeclarationSyntax classDecl,
        HashSet<string> handled)
    {
        foreach (var assignment in classDecl.DescendantNodes()
                     .OfType<AssignmentExpressionSyntax>())
        {
            if (!TryGetMemberName(assignment.Left, out var member))
                continue;

            if (assignment.Right is not InvocationExpressionSyntax invoke)
                continue;

            if (!TryGetMethodCall(invoke, out var method, out _))
                continue;

            if (method == "AddTo")
                handled.Add(member);
        }
    }

    #endregion
    
    private static bool TryGetMethodCall(
        InvocationExpressionSyntax invoke,
        out string method,
        out ExpressionSyntax receiver)
    {
        switch (invoke.Expression)
        {
            case MemberAccessExpressionSyntax access:
                method = access.Name.Identifier.ValueText;
                receiver = access.Expression;
                return true;

            case MemberBindingExpressionSyntax binding
                when invoke.Parent is ConditionalAccessExpressionSyntax conditional:
                method = binding.Name.Identifier.ValueText;
                receiver = conditional.Expression;
                return true;

            default:
                method = "";
                receiver = null!;
                return false;
        }
    }
    
    private static bool TryGetMemberName(
        ExpressionSyntax expression,
        out string name)
    {
        switch (expression)
        {
            case IdentifierNameSyntax id:
                name = id.Identifier.ValueText;
                return true;

            case MemberAccessExpressionSyntax
            {
                Expression: ThisExpressionSyntax,
                Name: IdentifierNameSyntax id
            }:
                name = id.Identifier.ValueText;
                return true;

            case PostfixUnaryExpressionSyntax postfix
                when postfix.IsKind(SyntaxKind.SuppressNullableWarningExpression):
                return TryGetMemberName(postfix.Operand, out name);

            default:
                name = "";
                return false;
        }
    }
    
    private static IReadOnlyList<ISymbol> GetDisposableMembers(INamedTypeSymbol type)
    {
        return type.GetMembers()
            .Where(m => !m.IsStatic)
            .Where(m => m is IFieldSymbol { AssociatedSymbol: null } or IPropertySymbol)
            .Where(m =>
            {
                var memberType = m switch
                {
                    IFieldSymbol f => f.Type,
                    IPropertySymbol p => p.Type,
                    _ => null
                };

                return memberType is not null &&
                       ImplementsIDisposable(memberType);
            })
            .ToList();
    }
}
