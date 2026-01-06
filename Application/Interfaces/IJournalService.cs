using Application.Common;
using Application.Models;

namespace Application.Interfaces;

public interface IJournalService
{
    // Entry CRUD
    Task<ServiceResult<JournalEntryDisplayModel>> GetEntryByIdAsync(Guid id);
    Task<ServiceResult<JournalEntryDisplayModel>> GetEntryByDateAsync(DateTime date);
    Task<ServiceResult<JournalEntryDisplayModel>> AddOrUpdateEntryAsync(JournalEntryInputModel model);
    Task<ServiceResult<bool>> DeleteEntryAsync(Guid entryId);
    
    // Search & Filter
    Task<ServiceResult<JournalSearchResultModel>> SearchAsync(JournalSearchModel model);
    
    // Calendar & Date helpers
    Task<ServiceResult<CalendarDataModel>> GetCalendarDataAsync(int year, int month);
    Task<ServiceResult<bool>> HasEntryForDateAsync(DateTime date);
    
    // Streaks & Analytics
    Task<ServiceResult<StreakDisplayModel>> GetStreakInfoAsync(DateTime asOf);
    Task<ServiceResult<List<DateTime>>> GetMissedDaysAsync(DateTime from, DateTime to);
    
    // Mood Analytics
    Task<ServiceResult<MoodDistributionModel>> GetMoodDistributionAsync(DateTime? from, DateTime? to);
    
    // Tag Analytics
    Task<ServiceResult<TagUsageModel>> GetTagUsageAsync(DateTime? from, DateTime? to, int topN = 10);
    
    // Word Count Trends
    Task<ServiceResult<WordCountTrendModel>> GetWordCountTrendsAsync(DateTime from, DateTime to);
    
    // Category Management
    Task<ServiceResult<List<CategoryDisplayModel>>> GetAllCategoriesAsync();
    Task<ServiceResult<CategoryDisplayModel>> AddCategoryAsync(CategoryInputModel model);
    Task<ServiceResult<CategoryDisplayModel>> UpdateCategoryAsync(Guid id, CategoryInputModel model);
    Task<ServiceResult<bool>> DeleteCategoryAsync(Guid id);
    
    // Tag Management
    Task<ServiceResult<List<TagDisplayModel>>> GetAllTagsAsync();
    Task<ServiceResult<List<TagDisplayModel>>> GetPrebuiltTagsAsync();
    Task<ServiceResult<TagDisplayModel>> AddTagAsync(TagInputModel model);
    Task<ServiceResult<bool>> DeleteTagAsync(Guid id);
    Task<ServiceResult<bool>> EnsurePrebuiltTagsAsync();
}