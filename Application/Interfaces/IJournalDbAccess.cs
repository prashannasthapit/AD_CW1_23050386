using Domain.Entities;

namespace Application.Interfaces;

public interface IJournalDbAccess
{
    // User / Auth
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByIdAsync(Guid id);
    Task<User> CreateUserAsync(string username, string? hashedPin, string theme = "Light");
    Task<User> UpdateUserAsync(User user);
    Task DeleteUserAsync(Guid id);
    Task<User?> GetDefaultUserAsync();
    Task<IList<User>> GetAllUsersAsync();
    
    // Journal Entry CRUD
    Task<JournalEntry?> GetEntryByIdAsync(Guid id);
    Task<JournalEntry?> GetEntryByDateAsync(DateTime date, Guid userId);
    Task<JournalEntry> AddEntryAsync(JournalEntry entry);
    Task<JournalEntry> UpdateEntryAsync(JournalEntry entry);
    Task DeleteEntryAsync(Guid entryId);
    
    // Entry Queries
    Task<IList<JournalEntry>> GetEntriesAsync(
        string? query,
        DateTime? from,
        DateTime? to,
        IEnumerable<Mood>? moods,
        IEnumerable<Guid>? tagIds,
        Guid? categoryId,
        int skip,
        int take,
        Guid userId);
    Task<int> GetEntriesCountAsync(
        string? query,
        DateTime? from,
        DateTime? to,
        IEnumerable<Mood>? moods,
        IEnumerable<Guid>? tagIds,
        Guid? categoryId,
        Guid userId);
    
    // Date helpers
    Task<IList<DateTime>> GetDatesWithEntriesAsync(int year, int month, Guid userId);
    Task<bool> HasEntryForDateAsync(DateTime date, Guid userId);
    Task<int> GetTotalEntriesCountAsync(Guid userId);
    
    // Streak helpers
    Task<IList<DateTime>> GetAllEntryDatesOrderedAsync(Guid userId);
    
    // Mood queries
    Task<IList<(Mood Mood, int Count)>> GetMoodCountsAsync(DateTime? from, DateTime? to);
    
    // Tag queries
    Task<IList<(string TagName, int Count)>> GetTagUsageCountsAsync(DateTime? from, DateTime? to, int topN);
    
    // Word count queries
    Task<IList<(DateTime Date, int WordCount)>> GetWordCountsByDateAsync(DateTime from, DateTime to);
    
    // Category CRUD
    Task<IList<Category>> GetAllCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(Guid id);
    Task<Category> AddCategoryAsync(Category category);
    Task<Category> UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(Guid id);
    
    // Tag CRUD
    Task<IList<Tag>> GetAllTagsAsync();
    Task<IList<Tag>> GetPrebuiltTagsAsync();
    Task<Tag?> GetTagByNameAsync(string name);
    Task<Tag> AddTagAsync(Tag tag);
    Task DeleteTagAsync(Guid id);
    Task<bool> TagExistsAsync(string name);
}