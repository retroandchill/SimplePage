# Simple Page
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=retroandchill_SimplePage&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=retroandchill_SimplePage) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=retroandchill_SimplePage&metric=coverage)](https://sonarcloud.io/summary/new_code?id=retroandchill_SimplePage)


This is a collection of packages for handling pagination within EF Core and ASP.NET.

## Packages
The following packages are included:

| Name                                 | Version                                                                                                                                                      |
|--------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Retro.SimplePage                     | [![NuGet Version](https://img.shields.io/nuget/v/Retro.SimplePage)](https://www.nuget.org/packages/Retro.SimplePage)                                         |
| Retro.SimplePage.EntityFrameworkCore | [![NuGet Version](https://img.shields.io/nuget/v/Retro.SimplePage.EntityFrameworkCore)](https://www.nuget.org/packages/Retro.SimplePage.EntityFrameworkCore) | 

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

#### Acknowledgements
<a href="https://www.flaticon.com/free-icons/pagination" title="pagination icons">Pagination icons created by Three musketeers - Flaticon</a>