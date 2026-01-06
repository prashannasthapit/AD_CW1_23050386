using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class JournalDbAccess(JournalDbContext context) : IJournalDbAccess
{
    #region User / Auth

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await context.Users.FindAsync(id);
    }

    public async Task<User> CreateUserAsync(string username, string? hashedPin, string theme = "Light")
    {
        var user = new User
        {
            Username = username,
            Pin = hashedPin,
            Theme = theme
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await context.Users.FindAsync(id);
        if (user != null)
        {
            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }
    }

    public async Task<User?> GetDefaultUserAsync()
    {
        return await context.Users.FirstOrDefaultAsync();
    }

    public async Task<IList<User>> GetAllUsersAsync()
    {
        return await context.Users.OrderBy(u => u.Username).ToListAsync();
    }

    #endregion

    #region Journal Entry CRUD

    public async Task<JournalEntry?> GetEntryByIdAsync(Guid id)
    {
        return await context.JournalEntries
            .Include(e => e.Category)
            .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
    {
        var dateOnly = date.Date;
        return await context.JournalEntries
            .Include(e => e.Category)
            .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
            .FirstOrDefaultAsync(e => e.EntryDate.Date == dateOnly);
    }

    public async Task<JournalEntry> AddEntryAsync(JournalEntry entry)
    {
        context.JournalEntries.Add(entry);
        await context.SaveChangesAsync();
        return entry;
    }

    public async Task<JournalEntry> UpdateEntryAsync(JournalEntry entry)
    {
        entry.UpdatedAt = DateTime.UtcNow;
        context.JournalEntries.Update(entry);
        await context.SaveChangesAsync();
        return entry;
    }

    public async Task DeleteEntryAsync(Guid entryId)
    {
        var entry = await context.JournalEntries.FindAsync(entryId);
        if (entry != null)
        {
            context.JournalEntries.Remove(entry);
            await context.SaveChangesAsync();
        }
    }

    #endregion

    #region Entry Queries

    public async Task<IList<JournalEntry>> GetEntriesAsync(
        string? query,
        DateTime? from,
        DateTime? to,
        IEnumerable<Mood>? moods,
        IEnumerable<Guid>? tagIds,
        Guid? categoryId,
        int skip,
        int take)
    {
        var q = BuildEntriesQuery(query, from, to, moods, tagIds, categoryId);
        return await q
            .OrderByDescending(e => e.EntryDate)
            .Skip(skip)
            .Take(take)
            .Include(e => e.Category)
            .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
            .ToListAsync();
    }

    public async Task<int> GetEntriesCountAsync(
        string? query,
        DateTime? from,
        DateTime? to,
        IEnumerable<Mood>? moods,
        IEnumerable<Guid>? tagIds,
        Guid? categoryId)
    {
        var q = BuildEntriesQuery(query, from, to, moods, tagIds, categoryId);
        return await q.CountAsync();
    }

    private IQueryable<JournalEntry> BuildEntriesQuery(
        string? query,
        DateTime? from,
        DateTime? to,
        IEnumerable<Mood>? moods,
        IEnumerable<Guid>? tagIds,
        Guid? categoryId)
    {
        IQueryable<JournalEntry> q = context.JournalEntries;

        if (!string.IsNullOrWhiteSpace(query))
        {
            q = q.Where(e => e.Title.Contains(query) || e.Content.Contains(query));
        }

        if (from.HasValue)
        {
            q = q.Where(e => e.EntryDate >= from.Value.Date);
        }

        if (to.HasValue)
        {
            q = q.Where(e => e.EntryDate <= to.Value.Date);
        }

        if (moods != null)
        {
            var moodList = moods.ToList();
            if (moodList.Count > 0)
            {
                q = q.Where(e => moodList.Contains(e.PrimaryMood));
            }
        }

        if (tagIds != null)
        {
            var tagIdList = tagIds.ToList();
            if (tagIdList.Count > 0)
            {
                q = q.Where(e => e.EntryTags.Any(et => tagIdList.Contains(et.TagId)));
            }
        }

        if (categoryId.HasValue)
        {
            q = q.Where(e => e.CategoryId == categoryId.Value);
        }

        return q;
    }

    #endregion

    #region Date Helpers

    public async Task<IList<DateTime>> GetDatesWithEntriesAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        return await context.JournalEntries
            .Where(e => e.EntryDate >= startDate && e.EntryDate <= endDate)
            .Select(e => e.EntryDate.Date)
            .Distinct()
            .ToListAsync();
    }

    public async Task<bool> HasEntryForDateAsync(DateTime date)
    {
        var dateOnly = date.Date;
        return await context.JournalEntries.AnyAsync(e => e.EntryDate.Date == dateOnly);
    }

    public async Task<int> GetTotalEntriesCountAsync()
    {
        return await context.JournalEntries.CountAsync();
    }

    #endregion

    #region Streak Helpers

    public async Task<IList<DateTime>> GetAllEntryDatesOrderedAsync()
    {
        return await context.JournalEntries
            .Select(e => e.EntryDate.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToListAsync();
    }

    #endregion

    #region Mood Queries

    public async Task<IList<(Mood Mood, int Count)>> GetMoodCountsAsync(DateTime? from, DateTime? to)
    {
        IQueryable<JournalEntry> q = context.JournalEntries;

        if (from.HasValue)
            q = q.Where(e => e.EntryDate >= from.Value.Date);
        if (to.HasValue)
            q = q.Where(e => e.EntryDate <= to.Value.Date);

        var result = await q
            .GroupBy(e => e.PrimaryMood)
            .Select(g => new { Mood = g.Key, Count = g.Count() })
            .ToListAsync();

        return result.Select(r => (r.Mood, r.Count)).ToList();
    }

    #endregion

    #region Tag Queries

    public async Task<IList<(string TagName, int Count)>> GetTagUsageCountsAsync(DateTime? from, DateTime? to, int topN)
    {
        IQueryable<EntryTag> q = context.EntryTags;

        if (from.HasValue || to.HasValue)
        {
            q = q.Where(et =>
                (!from.HasValue || et.JournalEntry.EntryDate >= from.Value.Date) &&
                (!to.HasValue || et.JournalEntry.EntryDate <= to.Value.Date));
        }

        var result = await q
            .GroupBy(et => et.Tag.Name)
            .Select(g => new { TagName = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(topN)
            .ToListAsync();

        return result.Select(r => (r.TagName, r.Count)).ToList();
    }

    #endregion

    #region Word Count Queries

    public async Task<IList<(DateTime Date, int WordCount)>> GetWordCountsByDateAsync(DateTime from, DateTime to)
    {
        // Since WordCount is a NotMapped property, we need to compute it in memory
        var entries = await context.JournalEntries
            .Where(e => e.EntryDate >= from.Date && e.EntryDate <= to.Date)
            .Select(e => new { e.EntryDate, e.Content })
            .ToListAsync();

        return entries
            .Select(e => (
                e.EntryDate.Date,
                WordCount: string.IsNullOrWhiteSpace(e.Content)
                    ? 0
                    : e.Content.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length
            ))
            .ToList();
    }

    #endregion

    #region Category CRUD

    public async Task<IList<Category>> GetAllCategoriesAsync()
    {
        return await context.Categories.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid id)
    {
        return await context.Categories.FindAsync(id);
    }

    public async Task<Category> AddCategoryAsync(Category category)
    {
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        return category;
    }

    public async Task<Category> UpdateCategoryAsync(Category category)
    {
        context.Categories.Update(category);
        await context.SaveChangesAsync();
        return category;
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        var category = await context.Categories.FindAsync(id);
        if (category != null)
        {
            context.Categories.Remove(category);
            await context.SaveChangesAsync();
        }
    }

    #endregion

    #region Tag CRUD

    public async Task<IList<Tag>> GetAllTagsAsync()
    {
        return await context.Tags.OrderBy(t => t.Name).ToListAsync();
    }

    public async Task<IList<Tag>> GetPrebuiltTagsAsync()
    {
        return await context.Tags
            .Where(t => t.IsPrebuilt)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Tag?> GetTagByNameAsync(string name)
    {
        return await context.Tags.FirstOrDefaultAsync(t => t.Name == name);
    }

    public async Task<Tag> AddTagAsync(Tag tag)
    {
        context.Tags.Add(tag);
        await context.SaveChangesAsync();
        return tag;
    }

    public async Task DeleteTagAsync(Guid id)
    {
        var tag = await context.Tags.FindAsync(id);
        if (tag != null)
        {
            context.Tags.Remove(tag);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> TagExistsAsync(string name)
    {
        return await context.Tags.AnyAsync(t => t.Name == name);
    }

    #endregion
}