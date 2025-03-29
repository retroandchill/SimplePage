using System.Runtime.CompilerServices;

namespace Retro.SimplePage;

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
  /// Converts a collection of items into a single page of results based on the specified page number and page size.
  /// </summary>
  /// <typeparam name="T">
  /// The type of the items in the collection.
  /// </typeparam>
  /// <param name="source">
  /// The source collection to be paginated.
  /// </param>
  /// <param name="pageNumber">
  /// The page number to retrieve. Must be greater than or equal to 1.
  /// </param>
  /// <param name="pageSize">
  /// The number of items per page. Must be greater than 0.
  /// </param>
  /// <returns>
  /// A <see cref="Page{T}"/> containing the items for the specified page and pagination details.
  /// </returns>
  public static Page<T> ToPage<T>(this IReadOnlyCollection<T> source, int pageNumber, int pageSize) {
    return new Page<T>(source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(), source.Count, pageNumber, pageSize);
  }

  /// <summary>
  /// Converts the given collection to a paginated page based on the specified page number and page size.
  /// </summary>
  /// <typeparam name="T">
  /// The type of the items in the source collection.
  /// </typeparam>
  /// <param name="source">
  /// The collection of items to be paginated.
  /// </param>
  /// <param name="pageable">
  /// Passed pagination settings.
  /// </param>
  /// <returns>
  /// A <see cref="Page{T}"/> containing the items of the specified page.
  /// </returns>
  public static Page<T> ToPage<T>(this IReadOnlyCollection<T> source, Pageable pageable) {
    return pageable.Match((pageNumber, pageSize) => ToPage(source, pageNumber, pageSize),
        () => new Page<T>(source.ToList()));
  }
}