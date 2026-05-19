namespace Ordering.Domain.Entities;

public sealed class OrderItem
{
    private OrderItem() { }

    internal OrderItem(Guid bookId, string title, int quantity, decimal unitPrice)
    {
        if (bookId == Guid.Empty) throw new ArgumentException("Book id is required.");
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (unitPrice < 0) throw new ArgumentOutOfRangeException(nameof(unitPrice));

        Id = Guid.NewGuid();
        BookId = bookId;
        Title = string.IsNullOrWhiteSpace(title) ? "Unknown book" : title.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid BookId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal => Quantity * UnitPrice;
}
