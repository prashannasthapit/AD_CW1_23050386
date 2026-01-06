using Domain.Entities;

namespace Application.Models;

/// <summary>
/// Model for streak information
/// </summary>
public class StreakDisplayModel
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public int TotalEntries { get; set; }
}

/// <summary>
/// Model for mood distribution analytics
/// </summary>
public class MoodDistributionModel
{
    public Dictionary<Mood, int> MoodCounts { get; set; } = new();
    public Dictionary<MoodCategory, int> CategoryCounts { get; set; } = new();
    public Mood? MostFrequentMood { get; set; }
}

/// <summary>
/// Model for tag usage analytics
/// </summary>
public class TagUsageModel
{
    public Dictionary<string, int> TagCounts { get; set; } = new();
}

/// <summary>
/// Model for word count trends
/// </summary>
public class WordCountTrendModel
{
    public Dictionary<DateTime, int> DailyWordCounts { get; set; } = new();
    public int TotalWords { get; set; }
    public double AverageWordsPerDay { get; set; }
}

/// <summary>
/// Model for calendar data
/// </summary>
public class CalendarDataModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public List<DateTime> DatesWithEntries { get; set; } = new();
    public List<DateTime> MissedDays { get; set; } = new();
}

