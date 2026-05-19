using EventBus.Events;
using MassTransit;
using Ordering.Application.Abstractions;
using Ordering.Domain.Entities;

namespace Ordering.Application.Orders;

public sealed class OrderService
{
    private readonly IOrderRepository _orders;
    private readonly IPublishEndpoint _publisher;

    public OrderService(IOrderRepository orders, IPublishEndpoint publisher)
    {
        _orders = orders;
        _publisher = publisher;
    }

    public async Task<OrderDto> PlaceAsync(Guid userId, CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var order = new Order(userId, request.StartDateUtc, request.EndDateUtc);
        foreach (var item in request.Items)
            order.AddItem(item.BookId, item.Title, item.Quantity, item.UnitPrice);

        order.Place();
        await _orders.AddAsync(order, cancellationToken);
        await _orders.SaveChangesAsync(cancellationToken);

        var integrationEvent = new OrderPlacedIntegrationEvent(
            order.Id,
            order.UserId,
            order.Items.Select(x => new OrderPlacedItem(x.BookId, x.Quantity)).ToArray());

        await _publisher.Publish(integrationEvent, cancellationToken);
        return Map(order);
    }

    public async Task<OrderDto> CancelAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var order = await _orders.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Order not found.");
        if (order.UserId != userId) throw new UnauthorizedAccessException("You cannot cancel another user's order.");

        order.Cancel();
        await _orders.SaveChangesAsync(cancellationToken);
        await _publisher.Publish(new OrderCancelledIntegrationEvent(
            order.Id,
            order.UserId,
            order.Items.Select(x => new OrderCancelledItem(x.BookId, x.Quantity)).ToArray()), cancellationToken);

        return Map(order);
    }

    public async Task<OrderDto> GetAsync(Guid id, Guid userId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var order = await _orders.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Order not found.");
        if (!isAdmin && order.UserId != userId) throw new UnauthorizedAccessException("Access denied.");
        return Map(order);
    }

    public IReadOnlyCollection<OrderDto> ListForUser(Guid userId, bool isAdmin)
    {
        var query = _orders.Query();
        if (!isAdmin) query = query.Where(x => x.UserId == userId);
        return query.OrderByDescending(x => x.CreatedAtUtc).AsEnumerable().Select(Map).ToArray();
    }

    private static OrderDto Map(Order x) =>
        new(x.Id, x.UserId, x.StartDateUtc, x.EndDateUtc, x.RentalDays, x.Status, x.TotalAmount,
            x.Items.Select(i => new OrderItemDto(i.BookId, i.Title, i.Quantity, i.UnitPrice, i.LineTotal)).ToArray());
}
