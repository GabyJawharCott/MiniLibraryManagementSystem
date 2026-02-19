namespace MiniLibraryManagementSystem.Entities;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int PageCount { get; set; }
    public int GenreId { get; set; }
    public Genre Genre { get; set; } = null!;

    public string? ISBN { get; set; }
    public string? Description { get; set; }
    public string? CoverUrl { get; set; }
    public int? PublishYear { get; set; }

    /// <summary>Estimated reading time in minutes (formula or AI).</summary>
    public int? EstimatedReadingMinutes { get; set; }
    /// <summary>Ease of reading: Easy, Medium, Hard (from AI or heuristic).</summary>
    public string? EaseOfReading { get; set; }

    public BookStatus Status { get; set; } = BookStatus.Available;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Soft delete: when true, the book is hidden from lists and cannot be loaned.</summary>
    public bool IsDeleted { get; set; }

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
