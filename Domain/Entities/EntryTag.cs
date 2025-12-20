namespace Domain.Entities;

public class EntryTag
{
    public Guid JournalEntryId { get; init; }
    public JournalEntry JournalEntry { get; init; } = null!;

    public Guid TagId { get; init; }
    public Tag Tag { get; init; } = null!;
}