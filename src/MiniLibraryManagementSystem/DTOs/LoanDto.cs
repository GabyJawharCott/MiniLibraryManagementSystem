using MiniLibraryManagementSystem.Entities;
using Microsoft.AspNetCore.Identity;

namespace MiniLibraryManagementSystem.DTOs;

public record LoanDto(
    int Id,
    int BookId,
    string BookTitle,
    string? GenreName,
    string UserId,
    string? UserEmail,
    DateTime BorrowedAt,
    DateTime DueDate,
    DateTime? ReturnedAt,
    DateTime CreatedAt)
{
    public static LoanDto FromEntity(Loan l)
    {
        var user = l.User;
        return new LoanDto(
            l.Id,
            l.BookId,
            l.Book?.Title ?? "",
            l.Book?.Genre?.Name,
            l.UserId,
            user?.Email,
            l.BorrowedAt,
            l.DueDate,
            l.ReturnedAt,
            l.CreatedAt);
    }
}

public record CheckOutDto(int BookId, string UserId, DateTime DueDate);
