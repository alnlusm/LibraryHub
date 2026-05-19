using Catalog.Domain.Entities;

namespace Catalog.Application.Abstractions;

public interface IBookRepository
{
    IQueryable<Book> Query();
    Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Book book, CancellationToken cancellationToken = default);
    Task DeleteAsync(Book book, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
