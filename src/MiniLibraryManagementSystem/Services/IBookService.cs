using MiniLibraryManagementSystem.DTOs;

namespace MiniLibraryManagementSystem.Services;

public interface IBookService
{
    Task<List<BookDto>> GetBooksAsync(CancellationToken ct = default);
    Task<(bool Success, BookDto? Book, string? Error)> CreateBookAsync(CreateBookDto dto, CancellationToken ct = default);
    Task<(bool Success, BookDto? Book, string? Error)> UpdateBookAsync(int id, UpdateBookDto dto, CancellationToken ct = default);
    Task<(bool Success, string? Error)> DeleteBookAsync(int id, CancellationToken ct = default);
}
