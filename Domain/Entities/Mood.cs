namespace Domain.Entities;

public enum Mood
{
    // Positive (0-4)
    Happy = 0,
    Excited = 1,
    Relaxed = 2,
    Grateful = 3,
    Confident = 4,
    // Neutral (5-9)
    Calm = 5,
    Thoughtful = 6,
    Curious = 7,
    Nostalgic = 8,
    Bored = 9,
    // Negative (10-14)
    Sad = 10,
    Angry = 11,
    Stressed = 12,
    Lonely = 13,
    Anxious = 14
}

public static class MoodExtensions
{
    public static MoodCategory GetCategory(this Mood mood)
    {
        return (int)mood switch
        {
            <= 4 => MoodCategory.Positive,
            <= 9 => MoodCategory.Neutral,
            _ => MoodCategory.Negative
        };
    }
    
    public static string GetEmoji(this Mood mood)
    {
        return mood switch
        {
            Mood.Happy => "😊",
            Mood.Excited => "🤩",
            Mood.Relaxed => "😌",
            Mood.Grateful => "🙏",
            Mood.Confident => "💪",
            Mood.Calm => "😐",
            Mood.Thoughtful => "🤔",
            Mood.Curious => "🧐",
            Mood.Nostalgic => "🥹",
            Mood.Bored => "😑",
            Mood.Sad => "😢",
            Mood.Angry => "😠",
            Mood.Stressed => "😰",
            Mood.Lonely => "😔",
            Mood.Anxious => "😟",
            _ => "😶"
        };
    }
    
    public static string GetCategoryColor(this MoodCategory category)
    {
        return category switch
        {
            MoodCategory.Positive => "#4caf50",
            MoodCategory.Neutral => "#2196f3",
            MoodCategory.Negative => "#f44336",
            _ => "#9e9e9e"
        };
    }

    public static IEnumerable<Mood> GetMoodsByCategory(MoodCategory category)
    {
        return Enum.GetValues<Mood>().Where(m => m.GetCategory() == category);
    }
}