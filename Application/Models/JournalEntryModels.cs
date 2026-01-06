using Domain.Entities;

namespace Application.Models;

/// <summary>
/// Model for creating/updating a journal entry
/// </summary>
public class JournalEntryInputModel
{
    public Guid? Id { get; set; }
    public DateTime EntryDate { get; set; } = DateTime.Today;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsMarkdown { get; set; } = true;
    public Mood PrimaryMood { get; set; }
    public List<Mood>? SecondaryMoods { get; set; }
    public Guid? CategoryId { get; set; }
    public List<Guid>? TagIds { get; set; }
}

/// <summary>
/// Model for displaying a journal entry
/// </summary>
public class JournalEntryDisplayModel
{
    public Guid Id { get; set; }
    public DateTime EntryDate { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsMarkdown { get; set; }
    public Mood PrimaryMood { get; set; }
    public MoodCategory PrimaryMoodCategory { get; set; }
    public List<Mood> SecondaryMoods { get; set; } = new();
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public List<TagDisplayModel> Tags { get; set; } = new();
    public int WordCount { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}

/// <summary>
/// Model for search/filter parameters
/// </summary>
public class JournalSearchModel
{
    public string? Query { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public List<Mood>? Moods { get; set; }
    public List<Guid>? TagIds { get; set; }
    public Guid? CategoryId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Model for paginated search results
/// </summary>
public class JournalSearchResultModel
{
    public List<JournalEntryDisplayModel> Entries { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

