namespace Montage.Weiss.Tools.Entities.Effect;

public record CardReminder
{
    public required string OriginalText { get; init; }
    public required string TranslatedText { get; init; }
    public bool IsUnmatched { get; init; } = false;
}
