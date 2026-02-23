using MiniLibraryManagementSystem.Entities;

namespace MiniLibraryManagementSystem.DTOs;

public record BookDto(
    int Id,
    string Title,
    string Author,
    int PageCount,
    int GenreId,
    string GenreName,
    string? ISBN,
    string? Description,
    string? CoverUrl,
    int? PublishYear,
    int? EstimatedReadingMinutes,
    string? EaseOfReading,
    string Status,
    /// <summary>When status is Borrowed, the due date of the current loan; null otherwise.</summary>
    DateTime? BorrowedDueDate,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    /// <summary>Display text for status: when Borrowed and BorrowedDueDate is set, use e.g. "Borrowed (due YYYY-MM-DD)".</summary>
    public string StatusDisplay => Status == "Borrowed" && BorrowedDueDate.HasValue
        ? $"Borrowed (due {BorrowedDueDate.Value:yyyy-MM-dd})"
        : Status;

    public static BookDto FromEntity(Book b, DateTime? borrowedDueDate = null) => new(
        b.Id,
        b.Title,
        b.Author,
        b.PageCount,
        b.GenreId,
        b.Genre?.Name ?? "",
        b.ISBN,
        b.Description,
        b.CoverUrl,
        b.PublishYear,
        b.EstimatedReadingMinutes,
        b.EaseOfReading,
        b.Status.ToString(),
        borrowedDueDate,
        b.CreatedAt,
        b.UpdatedAt);
}

public record CreateBookDto(
    string Title,
    string Author,
    int PageCount,
    int GenreId,
    string? ISBN,
    string? Description,
    string? CoverUrl,
    int? PublishYear);

public record UpdateBookDto(
    string Title,
    string Author,
    int PageCount,
    int GenreId,
    string? ISBN,
    string? Description,
    string? CoverUrl,
    int? PublishYear);
