using System;
using NUnit.Framework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Linq;

namespace Retro.SimplePage.Analyzer.Tests;

public class ToPageUsageAnalyzerTests
{
  [Test]
  public async Task Analyzer_Should_Report_Diagnostic_When_Using_ToPage_In_Async_Method()
  {
    // Arrange
    const string testCode = @"
                using System.Threading.Tasks;
                namespace Retro.SimplePage.EntityFrameworkCore {
                    public static class PagedQueryExtensions {
                        public static object ToPage(this object source) => null!;
                        public static Task<object> ToPageAsync(this object source) => Task.FromResult<object>(null!);
                    }

                    public class TestClass {
                        public async Task TestMethod() {
                            var result = new object().ToPage(); // Warning expected here
                        }
                    }
                }
            ";
    var analyzer = new ToPageUsageAnalyzer();

    // Create a compilation for the test
    var compilation = CreateCompilation(testCode);

    // Act
    var diagnostics = await GetDiagnosticsAsync(compilation, analyzer);

    // Assert
    Assert.That(diagnostics, Has.Length.EqualTo(1), "Expected one diagnostic to be reported.");
    var diagnostic = diagnostics.First();
    using (Assert.EnterMultipleScope())
    {
      Assert.That(diagnostic.Id, Is.EqualTo("SP0001"), "Expected diagnostic ID to match.");
      Assert.That(diagnostic.GetMessage(), Is.EqualTo("'ToPage' should not be used in async methods. Use 'ToPageAsync' instead."), "Diagnostic message mismatch.");
    }
  }

  [Test]
  public async Task Analyzer_Should_Not_Report_Diagnostic_When_Using_ToPage_Outside_Async_Method()
  {
    // Arrange
    const string testCode = @"
                namespace Retro.SimplePage.EntityFrameworkCore {
                    public static class PagedQueryExtensions {
                        public static object ToPage(this object source) => null!;
                        public static Task<object> ToPageAsync(this object source) => Task.FromResult<object>(null!);
                    }

                    public class TestClass {
                        public object TestMethod() {
                            var result = new object().ToPage(); // No warning here
                            return result;
                        }
                    }
                }
            ";
    var analyzer = new ToPageUsageAnalyzer();

    // Create a compilation for the test
    var compilation = CreateCompilation(testCode);

    // Act
    var diagnostics = await GetDiagnosticsAsync(compilation, analyzer);

    // Assert
    Assert.That(diagnostics, Has.Length.EqualTo(0), "Expected no diagnostics to be reported.");
  }

  private static Compilation CreateCompilation(string sourceCode)
  {
    var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(sourceCode));
    var references = AppDomain.CurrentDomain.GetAssemblies()
        .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
        .Select(a => MetadataReference.CreateFromFile(a.Location))
        .Cast<MetadataReference>()
        .ToArray();

    return CSharpCompilation.Create(
        "AnalyzerTest",
        new[] { syntaxTree },
        references,
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
  }

  private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(Compilation compilation, DiagnosticAnalyzer analyzer)
  {
    var options = new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty);
    var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer), options);
    return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
  }
}