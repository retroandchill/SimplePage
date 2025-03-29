using System.Runtime.CompilerServices;

namespace Retro.SimplePage;

/// <summary>
/// Provides extension methods for handling pagination of collections.
/// </summary>
public static class PageableExtensions {
  /// <summary>
  /// Iterates over a data source, performing paginated requests until reaching the end of the source.
  /// </summary>
  /// <typeparam name="T">
  /// The type of the items in the source collection.
  /// </typeparam>
  /// <typeparam name="TResult">
  /// The type of the items contained in each page of the results.
  /// </typeparam>
  /// <param name="source">
  /// The initial data source that will be used to determine the starting point of the pagination.
  /// </param>
  /// <param name="func">
  /// A function that takes an item from the source and a <see cref="PageRequest"/>, and returns a <see cref="Page{TResult}"/>.
  /// </param>
  /// <param name="pageSize">
  /// The number of items per page. Defaults to 10 if not specified.
  /// </param>
  /// <returns>
  /// An <see cref="IEnumerable{T}"/> containing all the results from the paginated requests.
  /// </returns>
  public static IEnumerable<TResult> PageToEnd<T, TResult>(this IEnumerable<T> source,
                                                           Func<T, PageRequest, Page<TResult>> func,
                                                           int pageSize) {
    var pageable = new PageRequest(1, pageSize);
    var first = source.First();
    var currentPage = func(first, pageable);
    IEnumerable<TResult> result = currentPage;
    while (!currentPage.IsLastPage) {
      pageable = pageable.Next;
      currentPage = func(first, pageable);
      result = result.Concat(currentPage);
    }

    return result;
  }
  
  /// <summary>
  /// Asynchronously iterates through all pages in a paginated sequence and yields the results.
  /// </summary>
  /// <typeparam name="T">
  /// The type of the elements in the source sequence.
  /// </typeparam>
  /// <typeparam name="TResult">
  /// The type of the elements in the paginated results.
  /// </typeparam>
  /// <param name="source">
  /// The source sequence to apply pagination on.
  /// </param>
  /// <param name="func">
  /// A function that takes an element from the source sequence and a <see cref="PageRequest"/> to return a <see cref="Task{TResult}"/> containing a page of results.
  /// </param>
  /// <param name="pageSize">
  /// The size of each page. Defaults to 10 if not specified.
  /// </param>
  /// <param name="cancellationToken">
  /// A <see cref="CancellationToken"/> that can be used to cancel the iteration.
  /// </param>
  /// <returns>
  /// An asynchronous enumerable sequence of type <typeparamref name="TResult"/> containing the elements from all pages.
  /// </returns>
  public static async IAsyncEnumerable<TResult> PageToEndAsync<T, TResult>(
      this IEnumerable<T> source, Func<T, PageRequest, Task<Page<TResult>>> func, int pageSize,
      [EnumeratorCancellation] CancellationToken cancellationToken = default) {
    var pageable = new PageRequest(1, pageSize);
    var first = source.First();
    var currentPage = await func(first, pageable);
    foreach (var item in currentPage) {
      yield return item;
    }

    while (!currentPage.IsLastPage) {
      pageable = pageable.Next;
      currentPage = await func(first, pageable);
      foreach (var item in currentPage) {
        yield return item;
      }
    }
  }

  /// <summary>
  /// Asynchronously iterates through all pages in a paginated sequence and yields the results.
  /// </summary>
  /// <typeparam name="T">
  /// The type of the elements in the source sequence.
  /// </typeparam>
  /// <typeparam name="TResult">
  /// The type of the elements in the paginated results.
  /// </typeparam>
  /// <param name="source">
  /// The source sequence to apply pagination on.
  /// </param>
  /// <param name="func">
  /// A function that takes an element from the source sequence and a <see cref="PageRequest"/> to return a <see cref="Task{TResult}"/> containing a page of results.
  /// </param>
  /// <param name="pageSize">
  /// The size of each page. Defaults to 10 if not specified.
  /// </param>
  /// <param name="cancellationToken">
  /// A <see cref="CancellationToken"/> that can be used to cancel the iteration.
  /// </param>
  /// <returns>
  /// An asynchronous enumerable sequence of type <typeparamref name="TResult"/> containing the elements from all pages.
  /// </returns>
  public static async IAsyncEnumerable<TResult> PageToEndAsync<T, TResult>(
      this IEnumerable<T> source, Func<T, PageRequest, CancellationToken, Task<Page<TResult>>> func, int pageSize,
      [EnumeratorCancellation] CancellationToken cancellationToken = default) {
    var pageable = new PageRequest(1, pageSize);
    var first = source.First();
    var currentPage = await func(first, pageable, cancellationToken);
    foreach (var item in currentPage) {
      yield return item;
    }

    while (!currentPage.IsLastPage) {
      pageable = pageable.Next;
      currentPage = await func(first, pageable, cancellationToken);
      foreach (var item in currentPage) {
        yield return item;
      }
    }
  }

  /// <summary>
  /// Divides an <see cref="IEnumerable{T}"/> into a collection of <see cref="Page{T}"/> instances with specified page size.
  /// </summary>
  /// <param name="source">
  /// The source collection to be paginated.
  /// </param>
  /// <param name="pageSize">
  /// The maximum number of items that each page can contain.
  /// </param>
  /// <typeparam name="T">
  /// The type of elements in the source collection.
  /// </typeparam>
  /// <returns>
  /// An <see cref="IEnumerable{T}"/> of <see cref="Page{T}"/> that represents the paginated items.
  /// </returns>
  public static IEnumerable<Page<T>> AsPages<T>(this IEnumerable<T> source, int pageSize) {
    var chunks = source
        .Select((x, i) => new { Index = i, Value = x })
        .GroupBy(x => x.Index / pageSize)
        .Select(x => x.Select(v => v.Value).ToList())
        .ToList();

    var totalSize = chunks.Sum(x => x.Count);
    return chunks.Select((x, i) => new Page<T>(x, totalSize, i + 1, pageSize));
  }

  /// <summary>
  /// Converts a sequence of items into a paginated collection that includes metadata such as total elements, page number, and page size.
  /// </summary>
  /// <typeparam name="T">
  /// The type of the items in the sequence.
  /// </typeparam>
  /// <param name="source">
  /// The sequence of items to be paginated.
  /// </param>
  /// <param name="pageNumber">
  /// The current page number in the paginated collection.
  /// </param>
  /// <param name="pageSize">
  /// The number of items to be included in each page.
  /// </param>
  /// <returns>
  /// A <see cref="Page{T}"/> containing the items for the specified page along with pagination metadata.
  /// </returns>
  /// <exception cref="InvalidOperationException">
  /// If the supplied enumerator lacks an easily determined size.
  /// </exception>
  public static Page<T> ToPage<T>(this IEnumerable<T> source, int pageNumber, int pageSize) {
    if (!source.TryGetNonEnumeratedCount(out var count)) {
      throw new InvalidOperationException("The source sequence cannot be enumerated more than once.");
    }
    
    return new Page<T>(source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(), count, pageNumber, pageSize);
  }

  /// <summary>
  /// Converts a collection into a paginated representation based on the specified page settings.
  /// </summary>
  /// <typeparam name="T">
  /// The type of items in the source collection.
  /// </typeparam>
  /// <param name="source">
  /// The source collection to be paginated.
  /// </param>
  /// <param name="pageable">
  /// The pagination details encapsulated in a <see cref="Pageable"/> structure, including page number and page size.
  /// </param>
  /// <returns>
  /// A <see cref="Page{T}"/> object representing the requested page and containing the items of that page.
  /// </returns>
  /// <exception cref="InvalidOperationException">
  /// If the supplied enumerator lacks an easily determined size.
  /// </exception>
  public static Page<T> ToPage<T>(this IEnumerable<T> source, Pageable pageable) {
    return pageable.Match((pageNumber, pageSize) => ToPage(source, pageNumber, pageSize),
        () => new Page<T>(source.ToList()));
  }
}