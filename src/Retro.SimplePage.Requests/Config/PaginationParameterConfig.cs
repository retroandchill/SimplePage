namespace Retro.SimplePage.Requests.Config;

/// <summary>
/// Represents configuration properties for pagination parameters in requests.
/// </summary>
public class PaginationParameterConfig {

  /// <summary>
  /// Specifies the name of the configuration section used for pagination settings in the application.
  /// </summary>
  public const string SectionName = "Pagination";

  /// <summary>
  /// Defines the parameter name used to specify the page number in query strings for pagination.
  /// </summary>
  public string PageNumberParamName { get; set; } = "page";

  /// <summary>
  /// Defines the query parameter name used to specify the number of items per page in a paginated request.
  /// </summary>
  public string PageSizeParamName { get; set; } = "size";

  /// <summary>
  /// Defines the default number of items to be displayed per page when the page size is not explicitly specified in a request.
  /// </summary>
  public int DefaultPageSize { get; set; } = 10;

  /// <summary>
  /// Specifies the maximum allowable page size that can be set for pagination.
  /// </summary>
  public int MaxPageSize { get; set; } = 100;

}