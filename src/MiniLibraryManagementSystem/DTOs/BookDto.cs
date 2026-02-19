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
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static BookDto FromEntity(Book b) => new(
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
