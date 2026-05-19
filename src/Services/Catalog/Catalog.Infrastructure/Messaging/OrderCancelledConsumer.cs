using Catalog.Application.Books;
using EventBus.Events;
using MassTransit;

namespace Catalog.Infrastructure.Messaging;

public sealed class OrderCancelledConsumer : IConsumer<OrderCancelledIntegrationEvent>
{
    private readonly BookService _books;

    public OrderCancelledConsumer(BookService books) => _books = books;

    public async Task Consume(ConsumeContext<OrderCancelledIntegrationEvent> context)
    {
        foreach (var item in context.Message.Items)
            await _books.ReturnAsync(item.BookId, item.Quantity, context.CancellationToken);
    }
}
