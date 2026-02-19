using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using MiniLibraryManagementSystem.Data;
using MiniLibraryManagementSystem.DTOs;
using MiniLibraryManagementSystem.Entities;

namespace MiniLibraryManagementSystem.Services;

public class BookService : IBookService
{
    private readonly ApplicationDbContext _db;
    private readonly AuthenticationStateProvider _authStateProvider;

    public BookService(ApplicationDbContext db, AuthenticationStateProvider authStateProvider)
    {
        _db = db;
        _authStateProvider = authStateProvider;
    }

    private async Task<bool> IsAdminOrLibrarianAsync()
    {
        var state = await _authStateProvider.GetAuthenticationStateAsync();
        var user = state.User;
        return user?.IsInRole("Admin") == true || user?.IsInRole("Librarian") == true;
    }

    public async Task<List<BookDto>> GetBooksAsync(CancellationToken ct = default)
    {
        var list = await _db.Books
            .Include(b => b.Genre)
            .OrderBy(b => b.Title)
            .Select(b => BookDto.FromEntity(b))
            .ToListAsync(ct);
        return list;
    }

    public async Task<(bool Success, BookDto? Book, string? Error)> CreateBookAsync(CreateBookDto dto, CancellationToken ct = default)
    {
        if (!await IsAdminOrLibrarianAsync())
            return (false, null, "You do not have permission to add books. Admin or Librarian role required.");

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
        return (true, BookDto.FromEntity(book), null);
    }

    public async Task<(bool Success, BookDto? Book, string? Error)> UpdateBookAsync(int id, UpdateBookDto dto, CancellationToken ct = default)
    {
        if (!await IsAdminOrLibrarianAsync())
            return (false, null, "You do not have permission to edit books. Admin or Librarian role required.");

        var book = await _db.Books.Include(b => b.Genre).FirstOrDefaultAsync(b => b.Id == id, ct);
        if (book is null)
            return (false, null, "Book not found.");

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
        return (true, BookDto.FromEntity(book), null);
    }
}
