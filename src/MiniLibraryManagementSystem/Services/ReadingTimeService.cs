using MiniLibraryManagementSystem.Entities;

namespace MiniLibraryManagementSystem.Services;

/// <summary>Estimates reading time (e.g. page count / 200 pages per hour).</summary>
public static class ReadingTimeService
{
    private const int DefaultPagesPerHour = 200;

    /// <summary>Returns estimated reading time in minutes from page count.</summary>
    public static int EstimateMinutes(int pageCount, int pagesPerHour = DefaultPagesPerHour)
    {
        if (pageCount <= 0) return 0;
        return (int)Math.Ceiling(pageCount * 60.0 / pagesPerHour);
    }
}
