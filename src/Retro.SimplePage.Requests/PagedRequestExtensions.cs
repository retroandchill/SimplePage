using Microsoft.Extensions.DependencyInjection;
using Retro.SimplePage.Requests.Binding;

namespace Retro.SimplePage.Requests;

/// <summary>
/// Provides extension methods for configuring pagination support in MVC applications.
/// </summary>
public static class PagedRequestExtensions {
  /// <summary>
  /// Adds support for custom pagination model binding in an MVC application.
  /// </summary>
  /// <param name="builder">The <see cref="IMvcBuilder"/> used to configure MVC services.</param>
  /// <returns>The <see cref="IMvcBuilder"/> instance to allow for chaining additional configurations.</returns>
  public static IMvcBuilder AddPagination(this IMvcBuilder builder) {
    return builder.AddMvcOptions(options => {
      options.ModelBinderProviders.Insert(0, new PaginationModelBinderProvider());
    });
  }
}