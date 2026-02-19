using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using MiniLibraryManagementSystem.Data;
using MiniLibraryManagementSystem.DTOs;
using MiniLibraryManagementSystem.Entities;

namespace MiniLibraryManagementSystem.Services;

public class LoanService : ILoanService
{
    private readonly ApplicationDbContext _db;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IEmailNotificationService _email;

    public LoanService(ApplicationDbContext db, AuthenticationStateProvider authStateProvider, IEmailNotificationService email)
    {
        _db = db;
        _authStateProvider = authStateProvider;
        _email = email;
    }

    private async Task<(string? UserId, bool IsStaff)> GetCurrentUserAsync()
    {
        var state = await _authStateProvider.GetAuthenticationStateAsync();
        var user = state.User;
        var userId = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        var isStaff = user?.IsInRole("Admin") == true || user?.IsInRole("Librarian") == true;
        return (userId, isStaff);
    }

    public async Task<List<LoanDto>> GetLoansAsync(bool activeOnly, CancellationToken ct = default)
    {
        string? userId = null;
        bool isStaff = false;
        try
        {
            var current = await GetCurrentUserAsync();
            userId = current.UserId;
            isStaff = current.IsStaff;
        }
        catch
        {
            // Auth not ready yet; return empty list
            return new List<LoanDto>();
        }
        var query = _db.Loans
            .Include(l => l.Book!).ThenInclude(b => b.Genre)
            .Include(l => l.User)
            .AsQueryable();
        if (!isStaff && !string.IsNullOrEmpty(userId))
            query = query.Where(l => l.UserId == userId);
        if (activeOnly)
            query = query.Where(l => l.ReturnedAt == null);
        var list = await query.OrderByDescending(l => l.BorrowedAt).ToListAsync(ct);
        return list.Select(LoanDto.FromEntity).ToList();
    }

    public async Task<(bool Success, string? Error)> CheckInAsync(int loanId, CancellationToken ct = default)
    {
        var (_, isStaff) = await GetCurrentUserAsync();
        if (!isStaff)
            return (false, "Only Admin or Librarian can check in books.");

        var loan = await _db.Loans.Include(l => l.Book).Include(l => l.User).FirstOrDefaultAsync(l => l.Id == loanId, ct);
        if (loan is null)
            return (false, "Loan not found.");
        if (loan.ReturnedAt.HasValue)
            return (false, "Loan already returned.");

        loan.ReturnedAt = DateTime.UtcNow;
        if (loan.Book is not null) loan.Book.Status = BookStatus.Available;
        await _db.SaveChangesAsync(ct);

        await _email.NotifyBookReturnedAsync(
            loan.User.Email ?? "",
            loan.User.UserName ?? "",
            loan.Book?.Title ?? "Book",
            loan.ReturnedAt.Value);

        return (true, null);
    }
}
