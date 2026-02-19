using MiniLibraryManagementSystem.DTOs;

namespace MiniLibraryManagementSystem.Services;

public interface ISearchService
{
    Task<List<BookDto>> SearchAsync(string? q, string? author, int? minPages, int? maxPages, int? genreId, string? level = null, CancellationToken ct = default);
    Task<List<GenreOption>> GetGenresAsync(CancellationToken ct = default);
}

public record GenreOption(int Id, string Name);
