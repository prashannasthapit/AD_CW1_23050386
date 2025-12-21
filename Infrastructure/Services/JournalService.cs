using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class JournalService : IJournalService
{
    private readonly JournalDbContext _db;

    public JournalService(JournalDbContext db) => _db = db;

    #region Entry CRUD

    public async Task<JournalEntry?> GetEntryByIdAsync(Guid id)
    {
        return await _db.JournalEntries
            .Include(e => e.EntryTags).ThenInclude(et => et.Tag)
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
    {
        var day = date.Date;
        return await _db.JournalEntries
            .Include(e => e.EntryTags).ThenInclude(et => et.Tag)
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.EntryDate.Date == day);
    }

    public async Task<JournalEntry> AddOrUpdateEntryAsync(JournalEntry entry)
    {
        var existing = await GetEntryByDateAsync(entry.EntryDate);
        if (existing is null)
        {
            entry.Id = Guid.NewGuid();
            entry.CreatedAt = DateTime.UtcNow;
            entry.UpdatedAt = DateTime.UtcNow;

            // Handle tags - ensure they exist in database
            var newEntryTags = new List<EntryTag>();
            foreach (var et in entry.EntryTags)
            {
                var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Id == et.TagId)
                          ?? await _db.Tags.FirstOrDefaultAsync(t => t.Name == et.Tag.Name);
                if (tag != null)
                {
                    newEntryTags.Add(new EntryTag { JournalEntryId = entry.Id, TagId = tag.Id });
                }
            }

            entry.EntryTags = newEntryTags;
            _db.JournalEntries.Add(entry);
        }
        else
        {
            existing.Title = entry.Title;
            existing.Content = entry.Content;
            existing.IsMarkdown = entry.IsMarkdown;
            existing.PrimaryMood = entry.PrimaryMood;
            existing.SecondaryMoods = entry.SecondaryMoods;
            existing.CategoryId = entry.CategoryId;
            existing.UpdatedAt = DateTime.UtcNow;

            // Sync tags
            _db.EntryTags.RemoveRange(existing.EntryTags);

            foreach (var et in entry.EntryTags)
            {
                var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Id == et.TagId)
                          ?? await _db.Tags.FirstOrDefaultAsync(t => t.Name == et.Tag.Name);
                if (tag != null)
                {
                    _db.EntryTags.Add(new EntryTag { JournalEntryId = existing.Id, TagId = tag.Id });
                }
            }

            entry = existing;
        }

        await _db.SaveChangesAsync();
        return entry;
    }

    public async Task DeleteEntryAsync(Guid entryId)
    {
        var entry = await _db.JournalEntries.FindAsync(entryId);
        if (entry != null)
        {
            _db.JournalEntries.Remove(entry);
            await _db.SaveChangesAsync();
        }
    }

    #endregion

    #region Search & Filter

    public async Task<(IList<JournalEntry> Entries, int TotalCount)> SearchAsync(
        string? query,
        DateTime? from,
        DateTime? to,
        IEnumerable<Mood>? moods,
        IEnumerable<Guid>? tagIds,
        Guid? categoryId,
        int page,
        int pageSize)
    {
        var q = _db.JournalEntries
            .Include(e => e.EntryTags).ThenInclude(et => et.Tag)
            .Include(e => e.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var searchTerm = query.ToLower();
            q = q.Where(e => e.Title.ToLower().Contains(searchTerm) ||
                             e.Content.ToLower().Contains(searchTerm));
        }

        if (from.HasValue)
            q = q.Where(e => e.EntryDate >= from.Value.Date);

        if (to.HasValue)
            q = q.Where(e => e.EntryDate <= to.Value.Date);

        if (moods != null && moods.Any())
        {
            var moodList = moods.ToList();
            q = q.Where(e => moodList.Contains(e.PrimaryMood));
        }

        if (tagIds != null && tagIds.Any())
        {
            var tagIdList = tagIds.ToList();
            q = q.Where(e => e.EntryTags.Any(et => tagIdList.Contains(et.TagId)));
        }

        if (categoryId.HasValue)
            q = q.Where(e => e.CategoryId == categoryId.Value);

        var totalCount = await q.CountAsync();

        var entries = await q.OrderByDescending(e => e.EntryDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (entries, totalCount);
    }

    #endregion

    #region Calendar & Date helpers

    public async Task<IList<DateTime>> GetDatesWithEntriesAsync(DateTime month)
    {
        var startOfMonth = new DateTime(month.Year, month.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        return await _db.JournalEntries
            .Where(e => e.EntryDate >= startOfMonth && e.EntryDate <= endOfMonth)
            .Select(e => e.EntryDate.Date)
            .ToListAsync();
    }

    public async Task<bool> HasEntryForDateAsync(DateTime date)
    {
        return await _db.JournalEntries.AnyAsync(e => e.EntryDate.Date == date.Date);
    }

    #endregion

    #region Streaks & Analytics

    public async Task<int> GetCurrentStreakAsync(DateTime asOf)
    {
        var dates = await _db.JournalEntries
            .Select(e => e.EntryDate.Date)
            .OrderByDescending(d => d)
            .ToListAsync();

        var today = asOf.Date;
        int streak = 0;
        var expected = today;

        // If no entry today, start from yesterday
        if (!dates.Contains(today))
            expected = today.AddDays(-1);

        foreach (var d in dates.Distinct().OrderByDescending(x => x))
        {
            if (d == expected)
            {
                streak++;
                expected = expected.AddDays(-1);
            }
            else if (d < expected)
            {
                break;
            }
        }

        return streak;
    }

    public async Task<int> GetLongestStreakAsync()
    {
        var dates = await _db.JournalEntries
            .Select(e => e.EntryDate.Date)
            .OrderBy(d => d)
            .ToListAsync();

        var distinctDates = dates.Distinct().ToList();
        int best = 0, current = 0;
        DateTime? prev = null;

        foreach (var d in distinctDates)
        {
            if (prev == null || d == prev.Value.AddDays(1))
                current++;
            else
                current = 1;

            prev = d;
            if (current > best)
                best = current;
        }

        return best;
    }

    public async Task<IList<DateTime>> GetMissedDaysAsync(DateTime from, DateTime to)
    {
        var present = await _db.JournalEntries
            .Where(e => e.EntryDate.Date >= from.Date && e.EntryDate.Date <= to.Date)
            .Select(e => e.EntryDate.Date)
            .ToListAsync();

        var presentSet = present.ToHashSet();
        var missed = new List<DateTime>();

        for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
        {
            if (!presentSet.Contains(d))
                missed.Add(d);
        }

        return missed;
    }

    public async Task<int> GetTotalEntriesCountAsync()
    {
        return await _db.JournalEntries.CountAsync();
    }

    #endregion

    #region Mood Analytics

    public async Task<Dictionary<MoodCategory, int>> GetMoodCategoryDistributionAsync(DateTime? from, DateTime? to)
    {
        var query = _db.JournalEntries.AsQueryable();

        if (from.HasValue)
            query = query.Where(e => e.EntryDate >= from.Value.Date);
        if (to.HasValue)
            query = query.Where(e => e.EntryDate <= to.Value.Date);

        var moods = await query.Select(e => e.PrimaryMood).ToListAsync();

        return moods
            .GroupBy(m => m.GetCategory())
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<Mood, int>> GetMoodDistributionAsync(DateTime? from, DateTime? to)
    {
        var query = _db.JournalEntries.AsQueryable();

        if (from.HasValue)
            query = query.Where(e => e.EntryDate >= from.Value.Date);
        if (to.HasValue)
            query = query.Where(e => e.EntryDate <= to.Value.Date);

        var moods = await query.Select(e => e.PrimaryMood).ToListAsync();

        return moods
            .GroupBy(m => m)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Mood?> GetMostFrequentMoodAsync(DateTime? from, DateTime? to)
    {
        var distribution = await GetMoodDistributionAsync(from, to);
        return distribution.Count == 0 ? null : distribution.MaxBy(kv => kv.Value).Key;
    }

    #endregion

    #region Tag Analytics

    public async Task<Dictionary<string, int>> GetTagUsageAsync(DateTime? from, DateTime? to, int topN = 10)
    {
        var query = _db.EntryTags
            .Include(et => et.Tag)
            .Include(et => et.JournalEntry)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(et => et.JournalEntry.EntryDate >= from.Value.Date);
        if (to.HasValue)
            query = query.Where(et => et.JournalEntry.EntryDate <= to.Value.Date);

        var tagCounts = await query
            .GroupBy(et => et.Tag.Name)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(topN)
            .ToListAsync();

        return tagCounts.ToDictionary(x => x.Name, x => x.Count);
    }

    #endregion

    #region Word Count Trends

    public async Task<Dictionary<DateTime, int>> GetWordCountTrendsAsync(DateTime from, DateTime to)
    {
        var entries = await _db.JournalEntries
            .Where(e => e.EntryDate >= from.Date && e.EntryDate <= to.Date)
            .Select(e => new { e.EntryDate, e.Content })
            .ToListAsync();

        return entries.ToDictionary(
            e => e.EntryDate.Date,
            e => string.IsNullOrWhiteSpace(e.Content)
                ? 0
                : e.Content.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length
        );
    }

    #endregion

    #region Category Management

    public async Task<IList<Category>> GetAllCategoriesAsync()
    {
        return await _db.Categories.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<Category> AddCategoryAsync(string name)
    {
        var existing = await _db.Categories.FirstOrDefaultAsync(c => c.Name == name);
        if (existing != null)
            return existing;

        var category = new Category { Name = name };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return category;
    }

    public async Task<Category?> UpdateCategoryAsync(Guid id, string name)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null)
            return null;

        category.Name = name;
        await _db.SaveChangesAsync();
        return category;
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category != null)
        {
            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
        }
    }

    #endregion

    #region Tag Management

    public async Task<IList<Tag>> GetAllTagsAsync()
    {
        return await _db.Tags.OrderBy(t => t.Name).ToListAsync();
    }

    public async Task<IList<Tag>> GetPrebuiltTagsAsync()
    {
        return await _db.Tags.Where(t => t.IsPrebuilt).OrderBy(t => t.Name).ToListAsync();
    }

    public async Task<Tag> AddTagAsync(string name, bool isPrebuilt = false)
    {
        var existing = await _db.Tags.FirstOrDefaultAsync(t => t.Name == name);
        if (existing != null)
            return existing;

        var tag = new Tag { Name = name, IsPrebuilt = isPrebuilt };
        _db.Tags.Add(tag);
        await _db.SaveChangesAsync();
        return tag;
    }

    public async Task DeleteTagAsync(Guid id)
    {
        var tag = await _db.Tags.FindAsync(id);
        if (tag != null && !tag.IsPrebuilt)
        {
            _db.Tags.Remove(tag);
            await _db.SaveChangesAsync();
        }
    }

    public async Task EnsurePrebuiltTagsAsync()
    {
        var existingNames = await _db.Tags
            .Where(t => t.IsPrebuilt)
            .Select(t => t.Name)
            .ToListAsync();

        var existingSet = existingNames.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var tagName in Tag.PrebuiltTags)
        {
            if (!existingSet.Contains(tagName))
            {
                _db.Tags.Add(new Tag { Name = tagName, IsPrebuilt = true });
            }
        }

        await _db.SaveChangesAsync();
    }

    #endregion
}