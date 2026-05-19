using Ordering.Domain.Entities;

namespace Ordering.Application.Abstractions;

public interface IOrderRepository
{
    IQueryable<Order> Query();
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
