using Application.Common;
using Application.Interfaces;
using Application.Models;
using Domain.Entities;

namespace Application.Services;

public class JournalService : IJournalService
{
    private readonly IJournalDbAccess dbAccess;
    private readonly IUserService _userService;

    public JournalService(IJournalDbAccess dbAccess, IUserService userService)
    {
        this.dbAccess = dbAccess;
        _userService = userService;
    }

    #region Entry CRUD

    public async Task<ServiceResult<JournalEntryDisplayModel>> GetEntryByIdAsync(Guid id)
    {
        try
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (!currentUser.Success || currentUser.Data == null)
                return ServiceResult<JournalEntryDisplayModel>.Fail("No user logged in.");
            var userId = currentUser.Data.Id;

            var entry = await dbAccess.GetEntryByIdAsync(id);
            if (entry == null || entry.UserId != userId)
                return ServiceResult<JournalEntryDisplayModel>.Fail("Entry not found.");

            return ServiceResult<JournalEntryDisplayModel>.Ok(MapToDisplay(entry));
        }
        catch (Exception ex)
        {
            return ServiceResult<JournalEntryDisplayModel>.Fail(ex.Message);
        }
    }

    public async Task<ServiceResult<JournalEntryDisplayModel>> GetEntryByDateAsync(DateTime date)
    {
        try
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (!currentUser.Success || currentUser.Data == null)
                return ServiceResult<JournalEntryDisplayModel>.Fail("No user logged in.");
            var userId = currentUser.Data.Id;

            var entry = await dbAccess.GetEntryByDateAsync(date, userId);
            if (entry == null || entry.UserId != userId)
                return ServiceResult<JournalEntryDisplayModel>.Fail("No entry found for this date.");

            return ServiceResult<JournalEntryDisplayModel>.Ok(MapToDisplay(entry));
        }
        catch (Exception ex)
        {
            return ServiceResult<JournalEntryDisplayModel>.Fail(ex.Message);
        }
    }

    public async Task<ServiceResult<JournalEntryDisplayModel>> AddOrUpdateEntryAsync(JournalEntryInputModel model)
    {
        try
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (!currentUser.Success || currentUser.Data == null)
                return ServiceResult<JournalEntryDisplayModel>.Fail("No user logged in.");
            var userId = currentUser.Data.Id;

            JournalEntry entry;
            
            if (model.Id.HasValue)
            {
                var existingEntry = await dbAccess.GetEntryByIdAsync(model.Id.Value);
                if (existingEntry != null && existingEntry.UserId == userId)
                {
                    // Update existing entry
                    UpdateEntryFromModel(existingEntry, model);
                    await UpdateEntryTags(existingEntry, model.TagIds);
                    entry = await dbAccess.UpdateEntryAsync(existingEntry);
                    return ServiceResult<JournalEntryDisplayModel>.Ok(MapToDisplay(entry));
                }
            }
            
            // Check if entry for date already exists
            var entryForDate = await dbAccess.GetEntryByDateAsync(model.EntryDate, userId);
            if (entryForDate != null && entryForDate.UserId == userId)
            {
                // Update existing entry for this date
                UpdateEntryFromModel(entryForDate, model);
                await UpdateEntryTags(entryForDate, model.TagIds);
                entry = await dbAccess.UpdateEntryAsync(entryForDate);
                return ServiceResult<JournalEntryDisplayModel>.Ok(MapToDisplay(entry));
            }

            // Create new entry
            entry = MapToEntity(model);
            entry.UserId = userId;
            entry = await dbAccess.AddEntryAsync(entry);
            
            if (model.TagIds != null && model.TagIds.Count > 0)
            {
                await UpdateEntryTags(entry, model.TagIds);
                entry = (await dbAccess.GetEntryByIdAsync(entry.Id))!;
            }
            
            return ServiceResult<JournalEntryDisplayModel>.Ok(MapToDisplay(entry));
        }
        catch (Exception ex)
        {
            return ServiceResult<JournalEntryDisplayModel>.Fail(ex.Message);
        }
    }

    public async Task<ServiceResult<bool>> DeleteEntryAsync(Guid entryId)
    {
        try
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (!currentUser.Success || currentUser.Data == null)
                return ServiceResult<bool>.Fail("No user logged in.");
            var userId = currentUser.Data.Id;

            var entry = await dbAccess.GetEntryByIdAsync(entryId);
            if (entry == null || entry.UserId != userId)
                return ServiceResult<bool>.Fail("Entry not found.");

            await dbAccess.DeleteEntryAsync(entryId);
            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Fail(ex.Message);
        }
    }

    #endregion

    #region Search & Filter

    public async Task<ServiceResult<JournalSearchResultModel>> SearchAsync(JournalSearchModel model)
    {
        try
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (!currentUser.Success || currentUser.Data == null)
                return ServiceResult<JournalSearchResultModel>.Fail("No user logged in.");
            var userId = currentUser.Data.Id;

            var skip = (model.Page - 1) * model.PageSize;
            var entries = await dbAccess.GetEntriesAsync(
                model.Query, model.From, model.To, 
                model.Moods, model.TagIds, model.CategoryId, 
                skip, model.PageSize, userId);
            var totalCount = await dbAccess.GetEntriesCountAsync(
                model.Query, model.From, model.To,
                model.Moods, model.TagIds, model.CategoryId, userId);

            var result = new JournalSearchResultModel
            {
                Entries = entries.Select(MapToDisplay).ToList(),
                TotalCount = totalCount,
                Page = model.Page,
                PageSize = model.PageSize
            };

            return ServiceResult<JournalSearchResultModel>.Ok(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<JournalSearchResultModel>.Fail(ex.Message);
        }
    }

    #endregion

    #region Calendar & Date Helpers

    public async Task<ServiceResult<CalendarDataModel>> GetCalendarDataAsync(int year, int month)
    {
        try
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (!currentUser.Success || currentUser.Data == null)
                return ServiceResult<CalendarDataModel>.Fail("No user logged in.");
            var userId = currentUser.Data.Id;

            var datesWithEntries = await dbAccess.GetDatesWithEntriesAsync(year, month, userId);
            
            // Calculate missed days for the month
            var startDate = new DateTime(year, month, 1);
            var endDate = DateTime.Today < startDate.AddMonths(1) 
                ? DateTime.Today 
                : startDate.AddMonths(1).AddDays(-1);
            
            var allDates = await dbAccess.GetAllEntryDatesOrderedAsync(userId);
            var dateSet = new HashSet<DateTime>(allDates);
            var missedDays = new List<DateTime>();
            
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (!dateSet.Contains(date))
                    missedDays.Add(date);
            }

            var result = new CalendarDataModel
            {
                Year = year,
                Month = month,
                DatesWithEntries = datesWithEntries.Select(d => d.Date).ToList(),
                MissedDays = missedDays
            };

            return ServiceResult<CalendarDataModel>.Ok(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<CalendarDataModel>.Fail(ex.Message);
        }
    }

    public async Task<ServiceResult<bool>> HasEntryForDateAsync(DateTime date)
    {
        try
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (!currentUser.Success || currentUser.Data == null)
                return ServiceResult<bool>.Fail("No user logged in.");
            var userId = currentUser.Data.Id;

            var hasEntry = await dbAccess.HasEntryForDateAsync(date, userId);
            return ServiceResult<bool>.Ok(hasEntry);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Fail(ex.Message);
        }
    }

    #endregion

    #region Streaks & Analytics

    public async Task<ServiceResult<StreakDisplayModel>> GetStreakInfoAsync(DateTime asOf)
    {
        try
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (!currentUser.Success || currentUser.Data == null)
                return ServiceResult<StreakDisplayModel>.Fail("No user logged in.");
            var userId = currentUser.Data.Id;

            var dates = await dbAccess.GetAllEntryDatesOrderedAsync(userId);
            var totalEntries = await dbAccess.GetTotalEntriesCountAsync(userId);
            var currentStreak = CalculateCurrentStreak(dates, asOf);
            var longestStreak = CalculateLongestStreak(dates);

            var result = new StreakDisplayModel
            {
                CurrentStreak = currentStreak,
                LongestStreak = longestStreak,
                TotalEntries = totalEntries
            };

            return ServiceResult<StreakDisplayModel>.Ok(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<StreakDisplayModel>.Fail(ex.Message);
        }
    }

    public async Task<ServiceResult<List<DateTime>>> GetMissedDaysAsync(DateTime from, DateTime to)
    {
        try
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (!currentUser.Success || currentUser.Data == null)
                return ServiceResult<List<DateTime>>.Fail("No user logged in.");
            var userId = currentUser.Data.Id;

            var dates = await dbAccess.GetAllEntryDatesOrderedAsync(userId);
            var dateSet = new HashSet<DateTime>(dates);
            var missedDays = new List<DateTime>();

            for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
            {
                if (!dateSet.Contains(date))
                    missedDays.Add(date);
            }

            return ServiceResult<List<DateTime>>.Ok(missedDays);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<DateTime>>.Fail(ex.Message);
        }
    }

    private static int CalculateCurrentStreak(IList<DateTime> dates, DateTime asOf)
    {
        if (dates.Count == 0)
            return 0;

        var currentDate = asOf.Date;
        int streak;
        
        if (dates.Contains(currentDate))
        {
            streak = 1;
            currentDate = currentDate.AddDays(-1);
        }
        else if (dates.Contains(currentDate.AddDays(-1)))
        {
            currentDate = currentDate.AddDays(-1);
            streak = 1;
            currentDate = currentDate.AddDays(-1);
        }
        else
        {
            return 0;
        }

        while (dates.Contains(currentDate))
        {
            streak++;
            currentDate = currentDate.AddDays(-1);
        }

        return streak;
    }

    private static int CalculateLongestStreak(IList<DateTime> dates)
    {
        if (dates.Count == 0)
            return 0;

        var sortedDates = dates.OrderBy(d => d).ToList();
        var longestStreak = 1;
        var currentStreak = 1;

        for (int i = 1; i < sortedDates.Count; i++)
        {
            if ((sortedDates[i] - sortedDates[i - 1]).Days == 1)
            {
                currentStreak++;
                longestStreak = Math.Max(longestStreak, currentStreak);
            }
            else
            {
                currentStreak = 1;
            }
        }

        return longestStreak;
    }

    #endregion

    #region Mood Analytics

    public async Task<ServiceResult<MoodDistributionModel>> GetMoodDistributionAsync(DateTime? from, DateTime? to)
    {
        try
        {
            var moodCounts = await dbAccess.GetMoodCountsAsync(from, to);
            
            var categoryDistribution = new Dictionary<MoodCategory, int>
            {
                { MoodCategory.Positive, 0 },
                { MoodCategory.Neutral, 0 },
                { MoodCategory.Negative, 0 }
            };

            foreach (var (mood, count) in moodCounts)
            {
                var category = mood.GetCategory();
                categoryDistribution[category] += count;
            }

            var result = new MoodDistributionModel
            {
                MoodCounts = moodCounts.ToDictionary(m => m.Mood, m => m.Count),
                CategoryCounts = categoryDistribution,
                MostFrequentMood = moodCounts.Count > 0 
                    ? moodCounts.OrderByDescending(m => m.Count).First().Mood 
                    : null
            };

            return ServiceResult<MoodDistributionModel>.Ok(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<MoodDistributionModel>.Fail(ex.Message);
        }
    }

    #endregion

    #region Tag Analytics

    public async Task<ServiceResult<TagUsageModel>> GetTagUsageAsync(DateTime? from, DateTime? to, int topN = 10)
    {
        try
        {
            var tagCounts = await dbAccess.GetTagUsageCountsAsync(from, to, topN);
            var result = new TagUsageModel
            {
                TagCounts = tagCounts.ToDictionary(t => t.TagName, t => t.Count)
            };

            return ServiceResult<TagUsageModel>.Ok(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<TagUsageModel>.Fail(ex.Message);
        }
    }

    #endregion

    #region Word Count Trends

    public async Task<ServiceResult<WordCountTrendModel>> GetWordCountTrendsAsync(DateTime from, DateTime to)
    {
        try
        {
            var wordCounts = await dbAccess.GetWordCountsByDateAsync(from, to);
            var dailyCounts = wordCounts.ToDictionary(w => w.Date, w => w.WordCount);
            var totalWords = dailyCounts.Values.Sum();
            var dayCount = dailyCounts.Count > 0 ? dailyCounts.Count : 1;

            var result = new WordCountTrendModel
            {
                DailyWordCounts = dailyCounts,
                TotalWords = totalWords,
                AverageWordsPerDay = (double)totalWords / dayCount
            };

            return ServiceResult<WordCountTrendModel>.Ok(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<WordCountTrendModel>.Fail(ex.Message);
        }
    }

    #endregion

    #region Category Management

    public async Task<ServiceResult<List<CategoryDisplayModel>>> GetAllCategoriesAsync()
    {
        try
        {
            var categories = await dbAccess.GetAllCategoriesAsync();
            var displays = categories.Select(MapCategoryToDisplay).ToList();
            return ServiceResult<List<CategoryDisplayModel>>.Ok(displays);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<CategoryDisplayModel>>.Fail(ex.Message);
        }
    }

    public async Task<ServiceResult<CategoryDisplayModel>> AddCategoryAsync(CategoryInputModel model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return ServiceResult<CategoryDisplayModel>.Fail("Category name is required.");

            var category = new Category { Name = model.Name };
            category = await dbAccess.AddCategoryAsync(category);
            return ServiceResult<CategoryDisplayModel>.Ok(MapCategoryToDisplay(category));
        }
        catch (Exception ex)
        {
            return ServiceResult<CategoryDisplayModel>.Fail(ex.Message);
        }
    }

    public async Task<ServiceResult<CategoryDisplayModel>> UpdateCategoryAsync(Guid id, CategoryInputModel model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return ServiceResult<CategoryDisplayModel>.Fail("Category name is required.");

            var category = await dbAccess.GetCategoryByIdAsync(id);
            if (category == null)
                return ServiceResult<CategoryDisplayModel>.Fail("Category not found.");

            category.Name = model.Name;
            category = await dbAccess.UpdateCategoryAsync(category);
            return ServiceResult<CategoryDisplayModel>.Ok(MapCategoryToDisplay(category));
        }
        catch (Exception ex)
        {
            return ServiceResult<CategoryDisplayModel>.Fail(ex.Message);
        }
    }

    public async Task<ServiceResult<bool>> DeleteCategoryAsync(Guid id)
    {
        try
        {
            var category = await dbAccess.GetCategoryByIdAsync(id);
            if (category == null)
                return ServiceResult<bool>.Fail("Category not found.");

            await dbAccess.DeleteCategoryAsync(id);
            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Fail(ex.Message);
        }
    }

    #endregion

    #region Tag Management

    public async Task<ServiceResult<List<TagDisplayModel>>> GetAllTagsAsync()
    {
        try
        {
            var tags = await dbAccess.GetAllTagsAsync();
            var displays = tags.Select(MapTagToDisplay).ToList();
            return ServiceResult<List<TagDisplayModel>>.Ok(displays);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<TagDisplayModel>>.Fail(ex.Message);
        }
    }

    public async Task<ServiceResult<List<TagDisplayModel>>> GetPrebuiltTagsAsync()
    {
        try
        {
            var tags = await dbAccess.GetPrebuiltTagsAsync();
            var displays = tags.Select(MapTagToDisplay).ToList();
            return ServiceResult<List<TagDisplayModel>>.Ok(displays);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<TagDisplayModel>>.Fail(ex.Message);
        }
    }

    public async Task<ServiceResult<TagDisplayModel>> AddTagAsync(TagInputModel model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return ServiceResult<TagDisplayModel>.Fail("Tag name is required.");

            if (await dbAccess.TagExistsAsync(model.Name))
                return ServiceResult<TagDisplayModel>.Fail("Tag already exists.");

            var tag = new Tag { Name = model.Name, IsPrebuilt = model.IsPrebuilt };
            tag = await dbAccess.AddTagAsync(tag);
            return ServiceResult<TagDisplayModel>.Ok(MapTagToDisplay(tag));
        }
        catch (Exception ex)
        {
            return ServiceResult<TagDisplayModel>.Fail(ex.Message);
        }
    }

    public async Task<ServiceResult<bool>> DeleteTagAsync(Guid id)
    {
        try
        {
            await dbAccess.DeleteTagAsync(id);
            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Fail(ex.Message);
        }
    }

    public async Task<ServiceResult<bool>> EnsurePrebuiltTagsAsync()
    {
        try
        {
            foreach (var tagName in Tag.PrebuiltTags)
            {
                if (!await dbAccess.TagExistsAsync(tagName))
                {
                    var tag = new Tag { Name = tagName, IsPrebuilt = true };
                    await dbAccess.AddTagAsync(tag);
                }
            }
            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Fail(ex.Message);
        }
    }

    #endregion

    #region Mapping Helpers

    private static JournalEntryDisplayModel MapToDisplay(JournalEntry entry) => new()
    {
        Id = entry.Id,
        EntryDate = entry.EntryDate,
        Title = entry.Title,
        Content = entry.Content,
        IsMarkdown = entry.IsMarkdown,
        PrimaryMood = entry.PrimaryMood,
        PrimaryMoodCategory = entry.PrimaryMood.GetCategory(),
        SecondaryMoods = ParseSecondaryMoods(entry.SecondaryMoods),
        CategoryId = entry.CategoryId,
        CategoryName = entry.Category?.Name,
        Tags = entry.EntryTags.Select(et => MapTagToDisplay(et.Tag)).ToList(),
        WordCount = entry.WordCount,
        CreatedAt = entry.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
        UpdatedAt = entry.UpdatedAt.ToString("yyyy-MM-dd HH:mm")
    };

    private static JournalEntry MapToEntity(JournalEntryInputModel model)
    {
        var entry = new JournalEntry
        {
            EntryDate = model.EntryDate.Date,
            Title = model.Title,
            Content = model.Content,
            IsMarkdown = model.IsMarkdown,
            PrimaryMood = model.PrimaryMood,
            SecondaryMoods = model.SecondaryMoods != null 
                ? string.Join(",", model.SecondaryMoods.Select(m => m.ToString())) 
                : null,
            CategoryId = model.CategoryId
        };
        
        if (model.Id.HasValue)
            entry.Id = model.Id.Value;
            
        return entry;
    }

    private static void UpdateEntryFromModel(JournalEntry entry, JournalEntryInputModel model)
    {
        entry.Title = model.Title;
        entry.Content = model.Content;
        entry.IsMarkdown = model.IsMarkdown;
        entry.PrimaryMood = model.PrimaryMood;
        entry.SecondaryMoods = model.SecondaryMoods != null 
            ? string.Join(",", model.SecondaryMoods.Select(m => m.ToString())) 
            : null;
        entry.CategoryId = model.CategoryId;
    }

    private async Task UpdateEntryTags(JournalEntry entry, List<Guid>? tagIds)
    {
        // Clear existing tags
        entry.EntryTags.Clear();
        
        if (tagIds == null || tagIds.Count == 0)
            return;

        // Add new tags
        foreach (var tagId in tagIds)
        {
            entry.EntryTags.Add(new EntryTag
            {
                JournalEntryId = entry.Id,
                TagId = tagId
            });
        }
        
        await dbAccess.UpdateEntryAsync(entry);
    }

    private static List<Mood> ParseSecondaryMoods(string? secondaryMoods)
    {
        if (string.IsNullOrWhiteSpace(secondaryMoods))
            return new List<Mood>();

        return secondaryMoods
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => Enum.TryParse<Mood>(s.Trim(), out var mood) ? mood : (Mood?)null)
            .Where(m => m.HasValue)
            .Select(m => m!.Value)
            .ToList();
    }

    private static CategoryDisplayModel MapCategoryToDisplay(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name
    };

    private static TagDisplayModel MapTagToDisplay(Tag tag) => new()
    {
        Id = tag.Id,
        Name = tag.Name,
        IsPrebuilt = tag.IsPrebuilt
    };

    #endregion
}

