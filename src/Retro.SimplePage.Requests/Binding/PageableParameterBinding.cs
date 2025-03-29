using Microsoft.AspNetCore.Mvc.ModelBinding;
using Retro.SimplePage.Requests.Config;

namespace Retro.SimplePage.Requests.Binding;

/// <summary>
/// A custom model binder for binding pagination parameters to a <see cref="Pageable"/> object.
/// </summary>
/// <remarks>
/// This model binder extracts pagination parameters, such as "page" and "size", from the HTTP request query string.
/// It parses these parameters and binds them to an instance of the <see cref="Pageable"/> type.
/// If the "page" or "size" parameters are not present in the query, default values are used.
/// </remarks>
[AutoConstructor]
public partial class PageableParameterBinding : IModelBinder {
  private readonly PaginationParameterConfig _config;
  
  /// <inheritdoc />
  public Task BindModelAsync(ModelBindingContext bindingContext) {
    if (bindingContext.ModelType != typeof(Pageable)) {
      throw new InvalidOperationException();
    }

    var queryParams = bindingContext.HttpContext.Request.Query;
    queryParams.TryGetValue(_config.PageNumberParamName, out var page);
    queryParams.TryGetValue(_config.PageSizeParamName, out var size);
    string? pageString = page;
    string? sizeString = size;
    var pageNumber = pageString is not null ? int.Parse(pageString) : 1;
    var pageSize = sizeString is not null ? int.Parse(sizeString) : _config.DefaultPageSize;

    if (pageNumber < 1) {
      bindingContext.ModelState.TryAddModelError(_config.PageNumberParamName, 
          "Page number must be greater than zero.");
      return Task.CompletedTask;
    }
    
    if (pageSize < 1) {
      bindingContext.ModelState.TryAddModelError(_config.PageSizeParamName, 
          "Page size must be greater than zero.");
      return Task.CompletedTask;
    }

    if (pageSize > _config.MaxPageSize) {
      bindingContext.ModelState.TryAddModelError(_config.PageSizeParamName, 
          $"Page size must not exceed the maximum allowed value of {_config.MaxPageSize}.");
      return Task.CompletedTask;
    }

    bindingContext.Result = ModelBindingResult.Success(new Pageable(pageNumber, pageSize));
    return Task.CompletedTask;
  }
}