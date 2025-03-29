# Simple Page
[![GitHub Release](https://img.shields.io/github/v/release/retroandchill/SimplePage)](https://github.com/retroandchill/SimplePage/releases)[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=retroandchill_SimplePage&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=retroandchill_SimplePage) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=retroandchill_SimplePage&metric=coverage)](https://sonarcloud.io/summary/new_code?id=retroandchill_SimplePage)


This is a collection of packages for handling pagination within EF Core and ASP.NET.

## Packages
The following packages are included:

| Name                                 | Version                                                                                                                                                      |
|--------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Retro.SimplePage                     | [![NuGet Version](https://img.shields.io/nuget/v/Retro.SimplePage)](https://www.nuget.org/packages/Retro.SimplePage)                                         |
| Retro.SimplePage.EntityFrameworkCore | [![NuGet Version](https://img.shields.io/nuget/v/Retro.SimplePage.EntityFrameworkCore)](https://www.nuget.org/packages/Retro.SimplePage.EntityFrameworkCore) | 
| Retro.SimplePage.Requests            | [![NuGet Version](https://img.shields.io/nuget/v/Retro.SimplePage.Requests)](https://www.nuget.org/packages/Retro.SimplePage.Requests)                       | 
| Retro.SimplePage.Swashbuckle         | [![NuGet Version](https://img.shields.io/nuget/v/Retro.SimplePage.Swashbuckle)](https://www.nuget.org/packages/Retro.SimplePage.Swashbuckle)                 | 

## How to use

Using this package is fairly straightforward. Simply use the following extension method to get a page.

```csharp
List<int> list = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
var page = list.ToPage(1, 5);
```

Here you can take any collection of known size and get a page from it.

You can also break an enumerable list up into pages as well.

```csharp
var originalList = Enumerable.Range(1, 100).ToList();
var asPages = originalList.AsPages(10).ToList();
```

This example creates a list of pages each containing 10 elements.

### With EF Core
However, as enticing as breaking up collections is, the main reason you'd want to use something like this is likely for 
usage with EF Core.

In that case it would look something like this:
```csharp
var posts = await _context.Posts
    .OrderBy(x => x.Id)
    .ToPageAsync(1, 10);
```

### Configuring Paginated Requests

You are also able to configure the API using `Retro.SimplePage.Requests` to link the `Pageable` type to a request 
parameter. You simply have to add the following line to your setup.

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .AddPagination();
```

By default this binds two optional query parameters named `page` and `size` to the query string. With `page` defaulting 
to the first page is not specified and `size` defaulting to `10`. The value of `size` also cannot exceed `100`.

You can easily modify your `appsettings.json` file to change any of those options.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Pagination": {
    "DefaultPageSize": 10,
    "MaxPageSize": 100
  }
}

```

### Swashbuckle Support

The `Retro.SimplePage.Swashbuckle` library allows you to configure Swashbuckle to handle the above alteration as well as 
creating model types for the Page type. This will create a schema model named `<MyType>Page`. This can then be used with
various generated OpenApi Clients to have common types. While these technically are all distinct classes, in the case of
dynamically typed languages you should be able to create common bridge types that will allow any page type to be 
accepted.

Here are a few examples:

#### Typescript
```typescript
interface Page<T> {
  pageNumber: number;
  totalPages: number;
  pageSize: number;
  count: number;
  items: Array<T>;
}
```

#### Python
```python
from typing import TypeVar, Protocol, Optional, Self, Annotated, List

from pydantic import Field

T = TypeVar('T')

class Page(Protocol[T]):
    page_number: Annotated[int, Field(strict=True, ge=1)]
    total_pages: Annotated[int, Field(strict=True, ge=1)]
    page_size: Annotated[int, Field(strict=True, ge=1)]
    count: Annotated[int, Field(strict=True, ge=0)]
    items: List[T]

    def to_str(self) -> str:
        pass

    def to_json(self) -> str:
        pass

    @classmethod
    def from_json(cls, json_str: str) -> Optional[Self]:
        pass

    def to_dict(self) -> dict[str, any]:
        pass

    @classmethod
    def from_dict(cls, obj: Optional[dict[str, any]]) -> Optional[Self]:
        pass
```

### C++
```c++
template <typename T, typename I>
concept Page = std::derived_from<T, ModelBase> && requires(T t) {
    { t.getPageNumber() } -> std::convertible_to<int32_t>;
    { t.getTotalPages() } -> std::convertible_to<int32_t>;
    { t.getPageSize() } -> std::convertible_to<int32_t>;
    { t.getCount() } -> std::convertible_to<int32_t>;
    { t.getItems() } -> std::convertible_to<std::vector<std::shared_ptr<I>>&>;
}
```

### Acknowledgements
<a href="https://www.flaticon.com/free-icons/pagination" title="pagination icons">Pagination icons created by Three musketeers - Flaticon</a>