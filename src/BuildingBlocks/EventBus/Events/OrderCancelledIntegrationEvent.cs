namespace EventBus.Events;

public sealed record OrderCancelledIntegrationEvent(
    Guid OrderId,
    Guid UserId,
    IReadOnlyCollection<OrderCancelledItem> Items) : IntegrationEvent;

public sealed record OrderCancelledItem(Guid BookId, int Quantity);
