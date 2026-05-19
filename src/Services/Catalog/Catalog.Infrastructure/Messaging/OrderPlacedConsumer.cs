using Catalog.Application.Books;
using EventBus.Events;
using MassTransit;

namespace Catalog.Infrastructure.Messaging;

public sealed class OrderPlacedConsumer : IConsumer<OrderPlacedIntegrationEvent>
{
    private readonly BookService _books;

    public OrderPlacedConsumer(BookService books) => _books = books;

    public async Task Consume(ConsumeContext<OrderPlacedIntegrationEvent> context)
    {
        foreach (var item in context.Message.Items)
            await _books.ReserveAsync(item.BookId, item.Quantity, context.CancellationToken);
    }
}
