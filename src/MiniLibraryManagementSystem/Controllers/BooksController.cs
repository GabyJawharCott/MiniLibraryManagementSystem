using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniLibraryManagementSystem.Data;
using MiniLibraryManagementSystem.Entities;
using MiniLibraryManagementSystem.DTOs;
using MiniLibraryManagementSystem.Services;

namespace MiniLibraryManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BooksController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public BooksController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks(CancellationToken ct)
    {
        var list = await _db.Books
            .Include(b => b.Genre)
            .Include(b => b.Loans.Where(l => l.ReturnedAt == null))
            .OrderBy(b => b.Title)
            .ToListAsync(ct);
        return Ok(list.Select(b => BookDto.FromEntity(b, b.Loans.FirstOrDefault()?.DueDate)).ToList());
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<BookDto>> GetBook(int id, CancellationToken ct)
    {
        var book = await _db.Books
            .Include(b => b.Genre)
            .Include(b => b.Loans.Where(l => l.ReturnedAt == null))
            .FirstOrDefaultAsync(b => b.Id == id, ct);
        if (book is null) return NotFound();
        var dueDate = book.Loans.FirstOrDefault()?.DueDate;
        return Ok(BookDto.FromEntity(book, dueDate));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult<BookDto>> CreateBook([FromBody] CreateBookDto dto, CancellationToken ct)
    {
        var book = new Book
        {
            Title = dto.Title,
            Author = dto.Author,
            PageCount = dto.PageCount,
            GenreId = dto.GenreId,
            ISBN = dto.ISBN,
            Description = dto.Description,
            CoverUrl = dto.CoverUrl,
            PublishYear = dto.PublishYear,
            Status = BookStatus.Available,
        };
        book.EstimatedReadingMinutes = ReadingTimeService.EstimateMinutes(book.PageCount);
        book.EaseOfReading = EaseOfReadingService.Estimate(book);

        _db.Books.Add(book);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(book).Reference(b => b.Genre).LoadAsync(ct);
        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, BookDto.FromEntity(book, null));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult<BookDto>> UpdateBook(int id, [FromBody] UpdateBookDto dto, CancellationToken ct)
    {
        var book = await _db.Books.Include(b => b.Genre).FirstOrDefaultAsync(b => b.Id == id, ct);
        if (book is null) return NotFound();

        book.Title = dto.Title;
        book.Author = dto.Author;
        book.PageCount = dto.PageCount;
        book.GenreId = dto.GenreId;
        book.ISBN = dto.ISBN;
        book.Description = dto.Description;
        book.CoverUrl = dto.CoverUrl;
        book.PublishYear = dto.PublishYear;
        book.UpdatedAt = DateTime.UtcNow;
        book.EstimatedReadingMinutes = ReadingTimeService.EstimateMinutes(book.PageCount);
        book.EaseOfReading = EaseOfReadingService.Estimate(book);

        await _db.SaveChangesAsync(ct);
        return Ok(BookDto.FromEntity(book, null));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult> DeleteBook(int id, CancellationToken ct)
    {
        var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (book is null) return NotFound();
        book.IsDeleted = true;
        book.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
