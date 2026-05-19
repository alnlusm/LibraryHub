using Catalog.Application.Abstractions;
using Catalog.Application.Books;
using Catalog.Domain.Entities;
using Moq;

namespace Catalog.Tests;

public sealed class BookServiceTests
{
    [Fact]
    public void Reserve_DecreasesAvailableCopies()
    {
        var book = new Book("978-1", "Clean Architecture", "Robert Martin", "Software", 2017, 15, 3);

        book.Reserve(2);

        Assert.Equal(1, book.AvailableCopies);
    }

    [Fact]
    public void Reserve_Throws_WhenStockIsInsufficient()
    {
        var book = new Book("978-2", "DDD", "Eric Evans", "Software", 2003, 18, 1);

        Assert.Throws<InvalidOperationException>(() => book.Reserve(2));
    }

    [Fact]
    public async Task SearchAsync_FiltersByGenreAndAvailability()
    {
        var books = new List<Book>
        {
            new("1", "C# in Depth", "Jon Skeet", "Software", 2019, 10, 2),
            new("2", "War and Peace", "Leo Tolstoy", "Classic", 1869, 8, 1)
        };
        books[1].Reserve(1);

        var repo = new Mock<IBookRepository>();
        repo.Setup(x => x.Query()).Returns(books.AsQueryable());

        var cache = new Mock<ICacheService>();
        cache.Setup(x => x.GetAsync<PagedResult<BookDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagedResult<BookDto>?)null);

        var service = new BookService(repo.Object, cache.Object);

        var result = await service.SearchAsync(new BookFilterRequest(null, null, "Software", null, null, null, null, true));

        Assert.Single(result.Items);
        Assert.Equal("C# in Depth", result.Items.First().Title);
    }

    [Fact]
    public async Task CreateAsync_InvalidatesCatalogCache()
    {
        var repo = new Mock<IBookRepository>();
        var cache = new Mock<ICacheService>();
        var service = new BookService(repo.Object, cache.Object);

        await service.CreateAsync(new CreateBookRequest("3", "Patterns", "GoF", "Software", 1994, 25, 5));

        repo.Verify(x => x.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(x => x.RemoveByPrefixAsync("catalog:books", It.IsAny<CancellationToken>()), Times.Once);
    }
}
