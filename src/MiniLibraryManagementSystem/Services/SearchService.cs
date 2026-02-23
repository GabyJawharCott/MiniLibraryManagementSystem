using Microsoft.EntityFrameworkCore;
using MiniLibraryManagementSystem.Data;
using MiniLibraryManagementSystem.DTOs;

namespace MiniLibraryManagementSystem.Services;

public class SearchService : ISearchService
{
    private readonly ApplicationDbContext _db;

    public SearchService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<BookDto>> SearchAsync(string? q, string? author, int? minPages, int? maxPages, int? genreId, string? level = null, CancellationToken ct = default)
    {
        var query = _db.Books.Include(b => b.Genre).Include(b => b.Loans.Where(l => l.ReturnedAt == null)).AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(term) ||
                (b.Author != null && b.Author.ToLower().Contains(term)) ||
                (b.Description != null && b.Description.ToLower().Contains(term)));
        }
        if (!string.IsNullOrWhiteSpace(author))
            query = query.Where(b => b.Author != null && b.Author.ToLower().Contains(author.Trim().ToLower()));
        if (minPages.HasValue)
            query = query.Where(b => b.PageCount >= minPages.Value);
        if (maxPages.HasValue)
            query = query.Where(b => b.PageCount <= maxPages.Value);
        if (genreId.HasValue && genreId.Value > 0)
            query = query.Where(b => b.GenreId == genreId.Value);
        if (!string.IsNullOrWhiteSpace(level))
            query = query.Where(b => b.EaseOfReading != null && b.EaseOfReading == level);

        var list = await query.Include(b => b.Loans.Where(l => l.ReturnedAt == null)).OrderBy(b => b.Title).ToListAsync(ct);
        return list.Select(b => BookDto.FromEntity(b, b.Loans.FirstOrDefault()?.DueDate)).ToList();
    }

    public async Task<List<GenreOption>> GetGenresAsync(CancellationToken ct = default)
    {
        var list = await _db.Genres.OrderBy(g => g.Name).Select(g => new GenreOption(g.Id, g.Name)).ToListAsync(ct);
        return list;
    }
}
