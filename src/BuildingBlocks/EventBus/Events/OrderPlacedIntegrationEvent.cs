namespace EventBus.Events;

public sealed record OrderPlacedIntegrationEvent(
    Guid OrderId,
    Guid UserId,
    IReadOnlyCollection<OrderPlacedItem> Items) : IntegrationEvent;

public sealed record OrderPlacedItem(Guid BookId, int Quantity);
