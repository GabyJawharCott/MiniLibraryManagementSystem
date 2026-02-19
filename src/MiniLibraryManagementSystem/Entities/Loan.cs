using Microsoft.AspNetCore.Identity;

namespace MiniLibraryManagementSystem.Entities;

public class Loan
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public IdentityUser User { get; set; } = null!;

    public DateTime BorrowedAt { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    /// <summary>When the book was returned; null if still borrowed.</summary>
    public DateTime? ReturnedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
