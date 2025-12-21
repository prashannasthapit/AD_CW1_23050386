namespace Application.Extensions;

public static class GuidExtensions
{
    public static Guid? ToGuid(this string? value)
    {
        if (Guid.TryParse(value, out var guid))
            return guid;
        return null;
    }
}