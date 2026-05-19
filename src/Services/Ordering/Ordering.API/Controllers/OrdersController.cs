using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.Orders;

namespace Ordering.API.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly OrderService _orders;

    public OrdersController(OrderService orders) => _orders = orders;

    /// <summary>Creates and places a rental order, then publishes an OrderPlaced event.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderDto>> Place(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _orders.PlaceAsync(CurrentUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    /// <summary>Returns orders for current user, or all orders for Admin.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<OrderDto>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<OrderDto>> List() =>
        Ok(_orders.ListForUser(CurrentUserId(), IsAdmin()));

    /// <summary>Returns one order by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await _orders.GetAsync(id, CurrentUserId(), IsAdmin(), cancellationToken));

    /// <summary>Cancels a placed order and publishes an OrderCancelled event.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> Cancel(Guid id, CancellationToken cancellationToken) =>
        Ok(await _orders.CancelAsync(id, CurrentUserId(), cancellationToken));

    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin() => User.IsInRole("Admin");
}
