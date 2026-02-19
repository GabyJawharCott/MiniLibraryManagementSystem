namespace MiniLibraryManagementSystem.Services;

/// <summary>Notifies the user by email when a borrowed book is returned.</summary>
public interface IEmailNotificationService
{
    /// <summary>Sends an email to the borrower when their book is marked as returned.</summary>
    /// <param name="userEmail">Borrower's email address.</param>
    /// <param name="userName">Borrower's display name (optional).</param>
    /// <param name="bookTitle">Title of the returned book.</param>
    /// <param name="returnedAt">When the book was returned.</param>
    Task NotifyBookReturnedAsync(string userEmail, string? userName, string bookTitle, DateTime returnedAt);
}
