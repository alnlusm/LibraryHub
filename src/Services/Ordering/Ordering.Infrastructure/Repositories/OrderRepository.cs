using Microsoft.EntityFrameworkCore;
using Ordering.Application.Abstractions;
using Ordering.Domain.Entities;
using Ordering.Infrastructure.Data;

namespace Ordering.Infrastructure.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly OrderingDbContext _db;

    public OrderRepository(OrderingDbContext db) => _db = db;

    public IQueryable<Order> Query() => _db.Orders.Include(x => x.Items).AsNoTracking();

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Orders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default) =>
        await _db.Orders.AddAsync(order, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
