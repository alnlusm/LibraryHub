namespace Catalog.Application.Books;

public sealed record BookDto(Guid Id, string Isbn, string Title, string Author, string Genre, int PublicationYear, decimal Price, int TotalCopies, int AvailableCopies);
public sealed record CreateBookRequest(string Isbn, string Title, string Author, string Genre, int PublicationYear, decimal Price, int TotalCopies);
public sealed record UpdateBookRequest(string Title, string Author, string Genre, int PublicationYear, decimal Price);
public sealed record AddCopiesRequest(int Quantity);
public sealed record BookFilterRequest(
    string? Search,
    string? Author,
    string? Genre,
    int? MinYear,
    int? MaxYear,
    decimal? MinPrice,
    decimal? MaxPrice,
    bool AvailableOnly = false,
    string SortBy = "title",
    bool Desc = false,
    int Page = 1,
    int PageSize = 20);

public sealed record PagedResult<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, int TotalCount);
