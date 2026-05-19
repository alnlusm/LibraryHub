using Ordering.Domain.Entities;

namespace Ordering.Application.Orders;

public sealed record CreateOrderRequest(DateTime StartDateUtc, DateTime EndDateUtc, IReadOnlyCollection<CreateOrderItemRequest> Items);
public sealed record CreateOrderItemRequest(Guid BookId, string Title, int Quantity, decimal UnitPrice);
public sealed record OrderItemDto(Guid BookId, string Title, int Quantity, decimal UnitPrice, decimal LineTotal);
public sealed record OrderDto(Guid Id, Guid UserId, DateTime StartDateUtc, DateTime EndDateUtc, int RentalDays, OrderStatus Status, decimal TotalAmount, IReadOnlyCollection<OrderItemDto> Items);
