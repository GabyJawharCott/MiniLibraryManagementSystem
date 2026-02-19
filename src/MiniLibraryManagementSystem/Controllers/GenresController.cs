using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniLibraryManagementSystem.Data;
using MiniLibraryManagementSystem.Entities;

namespace MiniLibraryManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class GenresController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public GenresController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Genre>>> GetGenres(CancellationToken ct)
    {
        var list = await _db.Genres.OrderBy(g => g.Name).ToListAsync(ct);
        return Ok(list);
    }
}
