using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace Retro.SimplePage.Analyzer.Tests;

public abstract class CodeFixVerifier {
  protected abstract DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer();

  protected abstract CodeFixProvider GetCSharpCodeFixProvider();

  protected async Task VerifyCSharpFixAsync(string source, string fixedSource) {
    var analyzer = GetCSharpDiagnosticAnalyzer();
    var provider = GetCSharpCodeFixProvider();

    var diagnostics = await GetDiagnosticsAsync(source, analyzer);
    Assert.That(diagnostics, Has.Length.EqualTo(1), "Expected one diagnostic to be found in source.");

    var fixedDocument = await ApplyCodeFixAsync(source, provider, diagnostics[0]);
    var updatedSource = await GetSourceAsync(fixedDocument);

    Assert.That(updatedSource, Is.EqualTo(fixedSource), "The code fix did not produce the expected result.");
  }

  private static async Task<ImmutableArray<Diagnostic>>
      GetDiagnosticsAsync(string source, DiagnosticAnalyzer analyzer) {
    var compilation = CreateCompilation(source);
    var analyzerOptions = new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty);
    var compilationWithAnalyzers = compilation.WithAnalyzers([analyzer], analyzerOptions);
    return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
  }
  
  private static Project CreateProject(string source)
  {
    // Create an AdhocWorkspace to dynamically create and work with Roslyn projects
    var workspace = new AdhocWorkspace();

    // Add a "project" to this workspace
    var project = workspace.CurrentSolution.AddProject("TestProject", "TestProject", LanguageNames.CSharp);

    // Add metadata references necessary for basic C# functionality
    project = project.AddMetadataReferences(
    [
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // Core assembly
            MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),  // System.Threading.Tasks
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location) // LINQ
    ]);

    // Add the provided source code as a document to the project
    var sourceText = SourceText.From(source);
    var document = project.AddDocument("TestDocument.cs", sourceText);

    // Return the project containing the document
    return document.Project;
  }


  private static async Task<Document>
      ApplyCodeFixAsync(string source, CodeFixProvider provider, Diagnostic diagnostic) {
    var project = CreateProject(source);
    var document = project.Documents.First();

    // Register code fixes
    var actions = new List<CodeAction>();
    var context = new CodeFixContext(document, diagnostic,
        (a, _) => actions.Add(a), CancellationToken.None);

    await provider.RegisterCodeFixesAsync(context);

    if (actions.Count == 0)
    {
      throw new InvalidOperationException("No code fixes were available for the given diagnostic.");
    }

    // Apply the first suggested code fix
    var action = actions.First();
    var operations = await action.GetOperationsAsync(CancellationToken.None);
    var solution = operations.OfType<ApplyChangesOperation>().First().ChangedSolution;

    var doc = solution.GetDocument(document.Id);
    Assert.That(doc, Is.Not.Null);
    return doc;

  }

  private static Compilation CreateCompilation(string source) {
    var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(source));
    var references = AppDomain.CurrentDomain.GetAssemblies()
        .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
        .Select(a => MetadataReference.CreateFromFile(a.Location))
        .Cast<MetadataReference>()
        .ToArray();

    return CSharpCompilation.Create(
        "AnalyzerTest",
        [syntaxTree],
        references,
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
  }

  private static async Task<string> GetSourceAsync(Document document) {
    var sourceText = await document.GetTextAsync();
    return sourceText.ToString();
  }

}