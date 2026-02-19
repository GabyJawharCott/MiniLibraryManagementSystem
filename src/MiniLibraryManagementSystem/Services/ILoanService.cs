using MiniLibraryManagementSystem.DTOs;

namespace MiniLibraryManagementSystem.Services;

public interface ILoanService
{
    Task<List<LoanDto>> GetLoansAsync(bool activeOnly, CancellationToken ct = default);
    Task<(bool Success, string? Error)> CheckInAsync(int loanId, CancellationToken ct = default);
}
