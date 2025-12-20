using Domain.Entities;

namespace Application.Interfaces;

public interface IJournalService
{
    Task<JournalEntry?> GetEntryByDateAsync(DateTime date);
    Task<JournalEntry> AddOrUpdateEntryAsync(JournalEntry entry);
    Task DeleteEntryAsync(Guid entryId);
    Task<IList<JournalEntry>> SearchAsync(
        string? q,
        DateTime? from,
        DateTime? to,
        Mood? mood,
        IEnumerable<string>? tags,
        int page,
        int pageSize);
    
    // Analytics
    Task<int> GetCurrentStreakAsync(DateTime asOf);
    Task<int> GetLongestStreakAsync();
    Task<IList<DateTime>> GetMissedDaysAsync(DateTime from, DateTime to);
    
    // Tag helpers
    Task<IList<Tag>> GetAllTagsAsync();
    Task<Tag> AddTagAsync(string name);
}