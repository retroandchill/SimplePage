using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Retro.SimplePage.Analyzer;

/// <summary>
/// Represents a diagnostic analyzer that identifies improper usage of the `ToPage` method
/// in asynchronous methods within C# code. This analyzer ensures that developers are warned
/// when using non-async pagination methods (`ToPage`) inside asynchronous contexts where
/// their async counterparts (`ToPageAsync`) should be used instead.
/// </summary>
/// <remarks>
/// The <c>ToPageUsageAnalyzer</c> is part of the Roslyn analysis framework and is designed
/// to integrate seamlessly with the tooling to provide real-time feedback on invalid API usage.
/// The diagnostics produced by the analyzer can be configured or suppressed using standard
/// Roslyn mechanisms.
/// </remarks>
/// <example>
/// A scenario where the analyzer produces a warning:
/// - Using the `ToPage` method inside an `async` method, where it is expected to use `ToPageAsync`.
/// </example>
/// <diagnostics>
/// Produces a warning with the diagnostic ID <c>SP001</c>.
/// </diagnostics>
/// <threadsafety>
/// This class is thread-safe and supports concurrent execution as enabled by the Roslyn analysis framework.
/// </threadsafety>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ToPageUsageAnalyzer : DiagnosticAnalyzer {
  internal const string DiagnosticId = "SP001";
  private const string Title = "Use ToPageAsync in async methods";
  private const string MessageFormat = "'ToPage' should not be used in async methods. Use 'ToPageAsync' instead.";
  private const string Category = "Usage";
  private const string Description = "Suggest using 'ToPageAsync' when 'ToPage' is called inside an async method.";

  private static readonly DiagnosticDescriptor Rule = new(
      DiagnosticId,
      Title,
      MessageFormat,
      Category,
      DiagnosticSeverity.Warning,
      isEnabledByDefault: true,
      description: Description);

  /// <inheritdoc />
  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

  /// <inheritdoc />
  public override void Initialize(AnalysisContext context) {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();
    context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
  }

  private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context) {
    var invocation = (InvocationExpressionSyntax)context.Node;

    // Check if the method being called is a member access expression
    if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) {
      return;
    }

    // Get the symbol for the method being called
    var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess);
    
    // Ensure the symbol exists and is a method
    if (symbolInfo.Symbol is not IMethodSymbol methodSymbol) {
      return;
    }

    // Get the fully qualified name of the method
    var fullyQualifiedName = $"{methodSymbol.ContainingNamespace}.{methodSymbol.ContainingType.Name}.{methodSymbol.Name}";

    // Check if the fully qualified name matches "Namespace.Type.MethodName"
    if (fullyQualifiedName != "Retro.SimplePage.EntityFrameworkCore.PagedQueryExtensions.ToPage") {
      return;
    }

    // Check whether this invocation is inside an async method
    var methodSyntax = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
    if (methodSyntax is null || !methodSyntax.Modifiers.Any(SyntaxKind.AsyncKeyword)) {
      return;
    }

    // Warn the user
    var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
    context.ReportDiagnostic(diagnostic);

  }
}