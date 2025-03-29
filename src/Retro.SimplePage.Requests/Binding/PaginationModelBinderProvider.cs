using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Retro.SimplePage.Requests.Config;

namespace Retro.SimplePage.Requests.Binding;

/// <summary>
/// Provides a custom implementation of the <see cref="IModelBinderProvider"/> interface
/// for binding model types related to pagination.
/// </summary>
/// <remarks>
/// This provider specializes in binding models of type <see cref="Pageable"/>
/// using the <see cref="PageableParameterBinding"/> implementation.
/// </remarks>
public class PaginationModelBinderProvider : IModelBinderProvider {
  /// <inheritdoc />
  public IModelBinder? GetBinder(ModelBinderProviderContext context) {
    ArgumentNullException.ThrowIfNull(context);

    var configuration = context.Services.GetRequiredService<IConfiguration>();
    var parameters = new PaginationParameterConfig();
    configuration.GetSection(PaginationParameterConfig.SectionName).Bind(parameters);


    return context.Metadata.ModelType == typeof(Pageable) ? new PageableParameterBinding(parameters) : null;
  }
}