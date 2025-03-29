﻿using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Retro.SimplePage.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ToPageUsageCodeFixProvider)), Shared]
public class ToPageUsageCodeFixProvider : CodeFixProvider {
  private const string Title = "Use ToPageAsync";

  public override ImmutableArray<string> FixableDiagnosticIds =>
      ImmutableArray.Create(ToPageUsageAnalyzer.DiagnosticId);

  public override FixAllProvider GetFixAllProvider() =>
      WellKnownFixAllProviders.BatchFixer;

  public override async Task RegisterCodeFixesAsync(CodeFixContext context) {
    var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
    if (root == null) return;

    var diagnostic = context.Diagnostics[0];
    var diagnosticSpan = diagnostic.Location.SourceSpan;

    if (root.FindNode(diagnosticSpan) is not InvocationExpressionSyntax invocation) return;

    context.RegisterCodeFix(
        Microsoft.CodeAnalysis.CodeActions.CodeAction.Create(
            Title,
            cancellationToken => ReplaceWithToPageAsync(context.Document, invocation, cancellationToken),
            nameof(ToPageUsageCodeFixProvider)),
        diagnostic);
  }

  private static async Task<Document> ReplaceWithToPageAsync(Document document, InvocationExpressionSyntax invocation,
                                                             CancellationToken cancellationToken) {
    var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

    // Replace "ToPage" with "ToPageAsync"
    var memberAccess = (MemberAccessExpressionSyntax) invocation.Expression;
    var newMemberAccess = memberAccess.WithName(SyntaxFactory.IdentifierName("ToPageAsync"));

    var newInvocation = invocation.WithExpression(newMemberAccess);
    
    // Add an 'await' statement to the updated invocation
    var awaitExpression = SyntaxFactory.AwaitExpression(newInvocation);

    editor.ReplaceNode(invocation, awaitExpression);
    return editor.GetChangedDocument();

  }
}