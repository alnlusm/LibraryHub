namespace Ordering.Domain.Entities;

public sealed class Order
{
    private readonly List<OrderItem> _items = [];
    private Order() { }

    public Order(Guid userId, DateTime startDateUtc, DateTime endDateUtc)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User id is required.");
        if (endDateUtc <= startDateUtc) throw new InvalidOperationException("End date must be after start date.");
        if ((endDateUtc - startDateUtc).TotalDays > 30) throw new InvalidOperationException("Rental period cannot exceed 30 days.");

        Id = Guid.NewGuid();
        UserId = userId;
        StartDateUtc = startDateUtc;
        EndDateUtc = endDateUtc;
        Status = OrderStatus.Draft;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime StartDateUtc { get; private set; }
    public DateTime EndDateUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public int RentalDays => Math.Max(1, (int)Math.Ceiling((EndDateUtc - StartDateUtc).TotalDays));
    public decimal TotalAmount => _items.Sum(x => x.LineTotal) * RentalDays;

    public void AddItem(Guid bookId, string title, int quantity, decimal unitPrice)
    {
        if (Status != OrderStatus.Draft) throw new InvalidOperationException("Only draft order can be changed.");
        var existing = _items.FirstOrDefault(x => x.BookId == bookId);
        if (existing is not null) throw new InvalidOperationException("Book is already added to this order.");
        _items.Add(new OrderItem(bookId, title, quantity, unitPrice));
    }

    public void Place()
    {
        if (Status != OrderStatus.Draft) throw new InvalidOperationException("Only draft order can be placed.");
        if (_items.Count == 0) throw new InvalidOperationException("Order must contain at least one item.");
        Status = OrderStatus.Placed;
    }

    public void Cancel()
    {
        if (Status != OrderStatus.Placed) throw new InvalidOperationException("Only placed order can be cancelled.");
        Status = OrderStatus.Cancelled;
    }

    public void Complete()
    {
        if (Status != OrderStatus.Placed) throw new InvalidOperationException("Only placed order can be completed.");
        Status = OrderStatus.Completed;
    }
}
