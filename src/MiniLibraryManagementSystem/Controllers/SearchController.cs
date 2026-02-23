using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniLibraryManagementSystem.Data;
using MiniLibraryManagementSystem.DTOs;

namespace MiniLibraryManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class SearchController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public SearchController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookDto>>> Search(
        [FromQuery] string? q,
        [FromQuery] string? author,
        [FromQuery] int? minPages,
        [FromQuery] int? maxPages,
        [FromQuery] int? genreId,
        CancellationToken ct)
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
        if (genreId.HasValue)
            query = query.Where(b => b.GenreId == genreId.Value);

        var list = await query.OrderBy(b => b.Title).ToListAsync(ct);
        return Ok(list.Select(b => BookDto.FromEntity(b, b.Loans.FirstOrDefault()?.DueDate)).ToList());
    }
}
