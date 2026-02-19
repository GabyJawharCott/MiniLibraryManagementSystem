using Microsoft.EntityFrameworkCore;
using MiniLibraryManagementSystem.Entities;

namespace MiniLibraryManagementSystem.Data.Seed;

public static class GenreSeed
{
    public static async Task SeedAsync(ApplicationDbContext db, CancellationToken ct = default)
    {
        if (await db.Genres.AnyAsync(ct)) return;

        var genres = new[] { "Fiction", "Non-Fiction", "Science", "History", "Biography", "Children", "Finance", "Self Help", "Other" };
        foreach (var name in genres)
            db.Genres.Add(new Genre { Name = name });
        await db.SaveChangesAsync(ct);
    }
}
