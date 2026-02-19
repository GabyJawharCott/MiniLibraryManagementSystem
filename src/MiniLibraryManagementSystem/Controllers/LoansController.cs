using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniLibraryManagementSystem.Data;
using MiniLibraryManagementSystem.Entities;
using MiniLibraryManagementSystem.DTOs;
using MiniLibraryManagementSystem.Services;

namespace MiniLibraryManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoansController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailNotificationService _email;

    public LoansController(ApplicationDbContext db, IEmailNotificationService email)
    {
        _db = db;
        _email = email;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LoanDto>>> GetLoans(bool? activeOnly, CancellationToken ct)
    {
        var query = _db.Loans
            .Include(l => l.Book).ThenInclude(b => b!.Genre)
            .Include(l => l.User)
            .AsQueryable();
        if (activeOnly == true)
            query = query.Where(l => l.ReturnedAt == null);
        var list = await query.OrderByDescending(l => l.BorrowedAt).Select(l => LoanDto.FromEntity(l)).ToListAsync(ct);
        return Ok(list);
    }

    [HttpPost("check-out")]
    public async Task<ActionResult<LoanDto>> CheckOut([FromBody] CheckOutDto dto, CancellationToken ct)
    {
        var book = await _db.Books.FindAsync([dto.BookId], ct);
        if (book is null) return NotFound("Book not found");
        if (book.Status == BookStatus.Borrowed) return BadRequest("Book is already borrowed");

        var loan = new Loan
        {
            BookId = dto.BookId,
            UserId = dto.UserId,
            BorrowedAt = DateTime.UtcNow,
            DueDate = dto.DueDate,
        };
        _db.Loans.Add(loan);
        book.Status = BookStatus.Borrowed;
        await _db.SaveChangesAsync(ct);

        await _db.Entry(loan).Reference(l => l.Book).LoadAsync(ct);
        await _db.Entry(loan.Book!).Reference(b => b.Genre).LoadAsync(ct);
        await _db.Entry(loan).Reference(l => l.User).LoadAsync(ct);
        return Ok(LoanDto.FromEntity(loan));
    }

    [HttpPost("{id:int}/check-in")]
    public async Task<ActionResult<LoanDto>> CheckIn(int id, CancellationToken ct)
    {
        var loan = await _db.Loans.Include(l => l.Book).Include(l => l.User).FirstOrDefaultAsync(l => l.Id == id, ct);
        if (loan is null) return NotFound();
        if (loan.ReturnedAt.HasValue) return BadRequest("Loan already returned");

        loan.ReturnedAt = DateTime.UtcNow;
        if (loan.Book is not null) loan.Book.Status = BookStatus.Available;
        await _db.SaveChangesAsync(ct);

        await _email.NotifyBookReturnedAsync(
            loan.User.Email ?? "",
            loan.User.UserName,
            loan.Book?.Title ?? "Book",
            loan.ReturnedAt.Value);

        await _db.Entry(loan).Reference(l => l.Book).LoadAsync(ct);
        if (loan.Book is not null) await _db.Entry(loan.Book).Reference(b => b.Genre).LoadAsync(ct);
        return Ok(LoanDto.FromEntity(loan));
    }
}
