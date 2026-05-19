using Catalog.Application.Abstractions;
using Catalog.Domain.Entities;
using Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories;

public sealed class BookRepository : IBookRepository
{
    private readonly CatalogDbContext _db;

    public BookRepository(CatalogDbContext db) => _db = db;

    public IQueryable<Book> Query() => _db.Books.AsNoTracking();

    public Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Books.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(Book book, CancellationToken cancellationToken = default) =>
        await _db.Books.AddAsync(book, cancellationToken);

    public Task DeleteAsync(Book book, CancellationToken cancellationToken = default)
    {
        _db.Books.Remove(book);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
