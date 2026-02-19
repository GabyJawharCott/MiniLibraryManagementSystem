using MiniLibraryManagementSystem.Entities;

namespace MiniLibraryManagementSystem.Services;

/// <summary>Placeholder for ease-of-reading (Easy/Medium/Hard). Can be extended with AI later.</summary>
public static class EaseOfReadingService
{
    /// <summary>Simple heuristic by page count; can be replaced with AI (e.g. OpenAI) later.</summary>
    public static string Estimate(Book book)
    {
        if (book.PageCount <= 150) return "Easy";
        if (book.PageCount <= 350) return "Medium";
        return "Hard";
    }
}
