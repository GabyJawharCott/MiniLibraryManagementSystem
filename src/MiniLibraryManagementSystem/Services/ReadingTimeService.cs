using MiniLibraryManagementSystem.Entities;

namespace MiniLibraryManagementSystem.Services;

/// <summary>Estimates reading time at ~1 page per minute (average adult pace).</summary>
public static class ReadingTimeService
{
    /// <summary>Average adult reading pace: about 60 pages per hour (~250 words/min).</summary>
    private const int DefaultPagesPerHour = 60;

    /// <summary>Returns estimated reading time in minutes from page count.</summary>
    public static int EstimateMinutes(int pageCount, int pagesPerHour = DefaultPagesPerHour)
    {
        if (pageCount <= 0) return 0;
        return (int)Math.Ceiling(pageCount * 60.0 / pagesPerHour);
    }
}
