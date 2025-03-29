using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Retro.SimplePage.Analyzer.Tests;

public class ToPageUsageCodeFixProviderTests : CodeFixVerifier {
  [Test]
  public async Task CodeFix_Should_Replace_ToPage_With_ToPageAsync()
  {
    // Arrange
    const string testCode = """
                            using System.Threading.Tasks;
                            namespace Retro.SimplePage.EntityFrameworkCore {
                              public static class PagedQueryExtensions {
                                public static object ToPage(this object source) => null!;
                                public static Task<object> ToPageAsync(this object source) => Task.FromResult<object>(null!);
                              }
                            
                              public class TestClass {
                                public async Task TestMethod() {
                                  var result = new object().ToPage(); // Diagnostic triggered here
                                }
                              }
                            }
                            """;

    const string fixedCode = """
                             using System.Threading.Tasks;
                             namespace Retro.SimplePage.EntityFrameworkCore {
                               public static class PagedQueryExtensions {
                                 public static object ToPage(this object source) => null!;
                                 public static Task<object> ToPageAsync(this object source) => Task.FromResult<object>(null!);
                               }
                             
                               public class TestClass {
                                 public async Task TestMethod() {
                                   var result = await new object().ToPageAsync(); // Diagnostic triggered here
                                 }
                               }
                             }
                             """;

    // Act and Assert
    await VerifyCSharpFixAsync(testCode, fixedCode);
  }

  protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
  {
    return new ToPageUsageAnalyzer();
  }

  protected override CodeFixProvider GetCSharpCodeFixProvider()
  {
    return new ToPageUsageCodeFixProvider();
  }
}