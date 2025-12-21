using Domain.Entities;

namespace Application.Interfaces;

public interface IJournalService
{
    // Entry CRUD
    Task<JournalEntry?> GetEntryByIdAsync(Guid id);
    Task<JournalEntry?> GetEntryByDateAsync(DateTime date);
    Task<JournalEntry> AddOrUpdateEntryAsync(JournalEntry entry);
    Task DeleteEntryAsync(Guid entryId);
    
    // Search & Filter
    Task<(IList<JournalEntry> Entries, int TotalCount)> SearchAsync(
        string? query,
        DateTime? from,
        DateTime? to,
        IEnumerable<Mood>? moods,
        IEnumerable<Guid>? tagIds,
        Guid? categoryId,
        int page,
        int pageSize);
    
    // Calendar & Date helpers
    Task<IList<DateTime>> GetDatesWithEntriesAsync(DateTime month);
    Task<bool> HasEntryForDateAsync(DateTime date);
    
    // Streaks & Analytics
    Task<int> GetCurrentStreakAsync(DateTime asOf);
    Task<int> GetLongestStreakAsync();
    Task<IList<DateTime>> GetMissedDaysAsync(DateTime from, DateTime to);
    Task<int> GetTotalEntriesCountAsync();
    
    // Mood Analytics
    Task<Dictionary<MoodCategory, int>> GetMoodCategoryDistributionAsync(DateTime? from, DateTime? to);
    Task<Dictionary<Mood, int>> GetMoodDistributionAsync(DateTime? from, DateTime? to);
    Task<Mood?> GetMostFrequentMoodAsync(DateTime? from, DateTime? to);
    
    // Tag Analytics
    Task<Dictionary<string, int>> GetTagUsageAsync(DateTime? from, DateTime? to, int topN = 10);
    
    // Word Count Trends
    Task<Dictionary<DateTime, int>> GetWordCountTrendsAsync(DateTime from, DateTime to);
    
    // Category Management
    Task<IList<Category>> GetAllCategoriesAsync();
    Task<Category> AddCategoryAsync(string name);
    Task<Category?> UpdateCategoryAsync(Guid id, string name);
    Task DeleteCategoryAsync(Guid id);
    
    // Tag Management
    Task<IList<Tag>> GetAllTagsAsync();
    Task<IList<Tag>> GetPrebuiltTagsAsync();
    Task<Tag> AddTagAsync(string name, bool isPrebuilt = false);
    Task DeleteTagAsync(Guid id);
    Task EnsurePrebuiltTagsAsync();
}