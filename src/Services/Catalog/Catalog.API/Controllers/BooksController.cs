using Catalog.Application.Books;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers;

[ApiController]
[Route("api/books")]
public sealed class BooksController : ControllerBase
{
    private readonly BookService _books;

    public BooksController(BookService books) => _books = books;

    /// <summary>Returns books with advanced LINQ filtering, sorting, and paging.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<BookDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<BookDto>>> Search([FromQuery] BookFilterRequest filter, CancellationToken cancellationToken) =>
        Ok(await _books.SearchAsync(filter, cancellationToken));

    /// <summary>Returns a single book by id.</summary>
    /// <param name="id">Book identifier.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookDto>> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await _books.GetAsync(id, cancellationToken));

    /// <summary>Creates a new book. Admin role is required.</summary>
    /// <param name="request">Book payload.</param>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BookDto>> Create(CreateBookRequest request, CancellationToken cancellationToken)
    {
        var result = await _books.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    /// <summary>Updates book metadata. Admin role is required.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookDto>> Update(Guid id, UpdateBookRequest request, CancellationToken cancellationToken) =>
        Ok(await _books.UpdateAsync(id, request, cancellationToken));

    /// <summary>Adds physical copies to the inventory. Admin role is required.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/copies")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AddCopies(Guid id, AddCopiesRequest request, CancellationToken cancellationToken)
    {
        await _books.AddCopiesAsync(id, request.Quantity, cancellationToken);
        return NoContent();
    }

    /// <summary>Deletes a book. Admin role is required.</summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _books.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
