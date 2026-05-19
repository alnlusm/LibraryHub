using EventBus.Events;
using MassTransit;
using Moq;
using Ordering.Application.Abstractions;
using Ordering.Application.Orders;
using Ordering.Domain.Entities;

namespace Ordering.Tests;

public sealed class OrderServiceTests
{
    [Fact]
    public void Order_CalculatesTotal_ForRentalDays()
    {
        var start = DateTime.UtcNow;
        var order = new Order(Guid.NewGuid(), start, start.AddDays(3));
        order.AddItem(Guid.NewGuid(), "Book", 2, 5);
        order.Place();

        Assert.Equal(30, order.TotalAmount);
        Assert.Equal(OrderStatus.Placed, order.Status);
    }

    [Fact]
    public void Order_RejectsRentalLongerThanThirtyDays()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new Order(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddDays(31)));
    }

    [Fact]
    public async Task PlaceAsync_PublishesOrderPlacedEvent()
    {
        var repo = new Mock<IOrderRepository>();
        var publisher = new Mock<IPublishEndpoint>();
        var service = new OrderService(repo.Object, publisher.Object);
        var request = new CreateOrderRequest(DateTime.UtcNow, DateTime.UtcNow.AddDays(5),
            new[] { new CreateOrderItemRequest(Guid.NewGuid(), "Book", 1, 10) });

        var result = await service.PlaceAsync(Guid.NewGuid(), request);

        Assert.Equal(OrderStatus.Placed, result.Status);
        publisher.Verify(x => x.Publish(It.IsAny<OrderPlacedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_RejectsDifferentUser()
    {
        var owner = Guid.NewGuid();
        var order = new Order(owner, DateTime.UtcNow, DateTime.UtcNow.AddDays(2));
        order.AddItem(Guid.NewGuid(), "Book", 1, 10);
        order.Place();

        var repo = new Mock<IOrderRepository>();
        repo.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var service = new OrderService(repo.Object, Mock.Of<IPublishEndpoint>());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.CancelAsync(order.Id, Guid.NewGuid()));
    }
}
