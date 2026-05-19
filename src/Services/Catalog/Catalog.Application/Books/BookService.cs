using Catalog.Application.Abstractions;
using Catalog.Domain.Entities;

namespace Catalog.Application.Books;

public sealed class BookService
{
    private const string CachePrefix = "catalog:books";
    private readonly IBookRepository _books;
    private readonly ICacheService _cache;

    public BookService(IBookRepository books, ICacheService cache)
    {
        _books = books;
        _cache = cache;
    }

    public async Task<BookDto> CreateAsync(CreateBookRequest request, CancellationToken cancellationToken = default)
    {
        var book = new Book(request.Isbn, request.Title, request.Author, request.Genre, request.PublicationYear, request.Price, request.TotalCopies);
        await _books.AddAsync(book, cancellationToken);
        await _books.SaveChangesAsync(cancellationToken);
        await _cache.RemoveByPrefixAsync(CachePrefix, cancellationToken);
        return Map(book);
    }

    public async Task<BookDto> UpdateAsync(Guid id, UpdateBookRequest request, CancellationToken cancellationToken = default)
    {
        var book = await GetRequiredAsync(id, cancellationToken);
        book.UpdateDetails(request.Title, request.Author, request.Genre, request.PublicationYear, request.Price);
        await _books.SaveChangesAsync(cancellationToken);
        await _cache.RemoveByPrefixAsync(CachePrefix, cancellationToken);
        return Map(book);
    }

    public async Task AddCopiesAsync(Guid id, int quantity, CancellationToken cancellationToken = default)
    {
        var book = await GetRequiredAsync(id, cancellationToken);
        book.AddCopies(quantity);
        await _books.SaveChangesAsync(cancellationToken);
        await _cache.RemoveByPrefixAsync(CachePrefix, cancellationToken);
    }

    public async Task ReserveAsync(Guid id, int quantity, CancellationToken cancellationToken = default)
    {
        var book = await GetRequiredAsync(id, cancellationToken);
        book.Reserve(quantity);
        await _books.SaveChangesAsync(cancellationToken);
        await _cache.RemoveByPrefixAsync(CachePrefix, cancellationToken);
    }

    public async Task ReturnAsync(Guid id, int quantity, CancellationToken cancellationToken = default)
    {
        var book = await GetRequiredAsync(id, cancellationToken);
        book.Return(quantity);
        await _books.SaveChangesAsync(cancellationToken);
        await _cache.RemoveByPrefixAsync(CachePrefix, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var book = await GetRequiredAsync(id, cancellationToken);
        await _books.DeleteAsync(book, cancellationToken);
        await _books.SaveChangesAsync(cancellationToken);
        await _cache.RemoveByPrefixAsync(CachePrefix, cancellationToken);
    }

    public async Task<BookDto> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Map(await GetRequiredAsync(id, cancellationToken));

    public async Task<PagedResult<BookDto>> SearchAsync(BookFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var key = $"{CachePrefix}:{filter.GetHashCode()}";
        var cached = await _cache.GetAsync<PagedResult<BookDto>>(key, cancellationToken);
        if (cached is not null) return cached;

        var query = _books.Query();

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(x => x.Title.ToLower().Contains(filter.Search.ToLower()) || x.Isbn.ToLower().Contains(filter.Search.ToLower()));
        if (!string.IsNullOrWhiteSpace(filter.Author))
            query = query.Where(x => x.Author.ToLower().Contains(filter.Author.ToLower()));
        if (!string.IsNullOrWhiteSpace(filter.Genre))
            query = query.Where(x => x.Genre.ToLower() == filter.Genre.ToLower());
        if (filter.MinYear.HasValue)
            query = query.Where(x => x.PublicationYear >= filter.MinYear.Value);
        if (filter.MaxYear.HasValue)
            query = query.Where(x => x.PublicationYear <= filter.MaxYear.Value);
        if (filter.MinPrice.HasValue)
            query = query.Where(x => x.Price >= filter.MinPrice.Value);
        if (filter.MaxPrice.HasValue)
            query = query.Where(x => x.Price <= filter.MaxPrice.Value);
        if (filter.AvailableOnly)
            query = query.Where(x => x.AvailableCopies > 0);

        query = (filter.SortBy.ToLowerInvariant(), filter.Desc) switch
        {
            ("author", false) => query.OrderBy(x => x.Author),
            ("author", true) => query.OrderByDescending(x => x.Author),
            ("year", false) => query.OrderBy(x => x.PublicationYear),
            ("year", true) => query.OrderByDescending(x => x.PublicationYear),
            ("price", false) => query.OrderBy(x => x.Price),
            ("price", true) => query.OrderByDescending(x => x.Price),
            ("available", false) => query.OrderBy(x => x.AvailableCopies),
            ("available", true) => query.OrderByDescending(x => x.AvailableCopies),
            (_, true) => query.OrderByDescending(x => x.Title),
            _ => query.OrderBy(x => x.Title)
        };

        var total = query.Count();
        var page = Math.Max(1, filter.Page);
        var size = Math.Clamp(filter.PageSize, 1, 100);
        var items = query.Skip((page - 1) * size).Take(size).AsEnumerable().Select(Map).ToArray();
        var result = new PagedResult<BookDto>(items, page, size, total);
        await _cache.SetAsync(key, result, TimeSpan.FromMinutes(3), cancellationToken);
        return result;
    }

    private async Task<Book> GetRequiredAsync(Guid id, CancellationToken cancellationToken) =>
        await _books.GetByIdAsync(id, cancellationToken) ?? throw new KeyNotFoundException("Book not found.");

    private static BookDto Map(Book x) =>
        new(x.Id, x.Isbn, x.Title, x.Author, x.Genre, x.PublicationYear, x.Price, x.TotalCopies, x.AvailableCopies);
}
