namespace Catalog.Domain.Entities;

public sealed class Book
{
    private Book() { }

    public Book(string isbn, string title, string author, string genre, int publicationYear, decimal price, int totalCopies)
    {
        Id = Guid.NewGuid();
        Isbn = Require(isbn, nameof(isbn), 32);
        Title = Require(title, nameof(title), 180);
        Author = Require(author, nameof(author), 120);
        Genre = Require(genre, nameof(genre), 80);
        PublicationYear = publicationYear is < 1450 or > 2100 ? throw new ArgumentOutOfRangeException(nameof(publicationYear)) : publicationYear;
        Price = price < 0 ? throw new ArgumentOutOfRangeException(nameof(price)) : price;
        TotalCopies = totalCopies < 0 ? throw new ArgumentOutOfRangeException(nameof(totalCopies)) : totalCopies;
        AvailableCopies = totalCopies;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Isbn { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public string Genre { get; private set; } = string.Empty;
    public int PublicationYear { get; private set; }
    public decimal Price { get; private set; }
    public int TotalCopies { get; private set; }
    public int AvailableCopies { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public void UpdateDetails(string title, string author, string genre, int publicationYear, decimal price)
    {
        Title = Require(title, nameof(title), 180);
        Author = Require(author, nameof(author), 120);
        Genre = Require(genre, nameof(genre), 80);
        PublicationYear = publicationYear is < 1450 or > 2100 ? throw new ArgumentOutOfRangeException(nameof(publicationYear)) : publicationYear;
        Price = price < 0 ? throw new ArgumentOutOfRangeException(nameof(price)) : price;
    }

    public void AddCopies(int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        TotalCopies += quantity;
        AvailableCopies += quantity;
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (AvailableCopies < quantity) throw new InvalidOperationException("Not enough copies available.");
        AvailableCopies -= quantity;
    }

    public void Return(int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (AvailableCopies + quantity > TotalCopies) throw new InvalidOperationException("Return quantity exceeds rented copies.");
        AvailableCopies += quantity;
    }

    private static string Require(string value, string name, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{name} is required.");
        value = value.Trim();
        return value.Length > max ? throw new ArgumentException($"{name} is too long.") : value;
    }
}
