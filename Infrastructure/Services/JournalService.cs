using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

// temporarily placed in Infrastructure
// TODO: implement data access layer and move service to Application
public class JournalService : IJournalService
{
    private readonly JournalDbContext _db;
    public JournalService(JournalDbContext db) => _db = db;
    
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
            entry.CreatedAt = DateTime.UtcNow;
            entry.UpdatedAt = DateTime.UtcNow;
            _db.JournalEntries.Add(entry);
        }
        else
        {
            // Update fields (simple approach)
            existing.Title = entry.Title;
            existing.Content = entry.Content;
            existing.IsMarkdown = entry.IsMarkdown;
            existing.PrimaryMood = entry.PrimaryMood;
            existing.SecondaryMoods = entry.SecondaryMoods;
            existing.CategoryId = entry.CategoryId;
            existing.UpdatedAt = DateTime.UtcNow;
            // synchronize tags: naive approach - remove all and re-add
            existing.EntryTags.Clear();
            foreach (var et in entry.EntryTags)
            {
                existing.EntryTags.Add(new EntryTag { JournalEntryId = existing.Id, TagId = et.TagId });
            }
        }
        await _db.SaveChangesAsync();
        return entry;
    }
    
    public async Task DeleteEntryAsync(Guid entryId)
    {
        var e = await _db.JournalEntries.FindAsync(entryId);
        if (e != null)
        {
            _db.JournalEntries.Remove(e);
            await _db.SaveChangesAsync();
        }
    }
    
    public async Task<IList<JournalEntry>> SearchAsync(
        string? q,
        DateTime? from,
        DateTime? to,
        Mood? mood,
        IEnumerable<string>? tags,
        int page,
        int pageSize)
    {
        var ql = _db.JournalEntries
            .Include(e => e.EntryTags).ThenInclude(et => et.Tag)
            .Include(e => e.Category)
            .AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(q))
        {
            ql = ql.Where(e => e.Title.Contains(q) || e.Content.Contains(q));
        }
        if (from.HasValue) ql = ql.Where(e => e.EntryDate >= from.Value.Date);
        if (to.HasValue) ql = ql.Where(e => e.EntryDate <= to.Value.Date);
        if (mood.HasValue) ql = ql.Where(e => e.PrimaryMood == mood.Value);
        if (tags != null && tags.Any())
        {
            ql = ql.Where(e => e.EntryTags.Any(et => tags.Contains(et.Tag.Name)));
        }
        
        return await ql.OrderByDescending(e => e.EntryDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<int> GetCurrentStreakAsync(DateTime asOf)
    {
        // naive implementation: fetch all entry dates and compute streak
        var dates = await _db.JournalEntries.Select(e => e.EntryDate.Date).OrderByDescending(d => d).ToListAsync();
        var today = asOf.Date;
        int streak = 0;
        var expected = today;
        foreach (var d in dates)
        {
            if (d == expected) { streak++; expected = expected.AddDays(-1); }
            else if (d < expected) break; // gap
        }
        return streak;
    }
    
    public async Task<int> GetLongestStreakAsync()
    {
        var dates = (await _db.JournalEntries.Select(e => e.EntryDate.Date).OrderBy(d => d).ToListAsync()).Distinct().ToList();
        int best = 0; int current = 0; DateTime? prev = null;
        foreach (var d in dates)
        {
            if (prev == null || d == prev.Value.AddDays(1)) current++; else current = 1;
            prev = d;
            if (current > best) best = current;
        }
        return best;
    }
    
    public async Task<IList<DateTime>> GetMissedDaysAsync(DateTime from,
        DateTime to)
    {
        var present = await _db.JournalEntries
            .Where(e => e.EntryDate.Date >= from.Date && e.EntryDate.Date <= to.Date)
            .Select(e => e.EntryDate.Date)
            .ToListAsync();
        
        var missed = new List<DateTime>();
        for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
        {
            if (!present.Contains(d)) missed.Add(d);
        }
        return missed;
    }
    
    public async Task<IList<Tag>> GetAllTagsAsync() => await _db.Tags.OrderBy(t => t.Name).ToListAsync();
    
    public async Task<Tag> AddTagAsync(string name)
    {
        var existing = await _db.Tags.FirstOrDefaultAsync(t => t.Name == name);
        if (existing != null) return existing;
        var tag = new Tag { Name = name };
        _db.Tags.Add(tag);
        await _db.SaveChangesAsync();
        return tag;
    }
}