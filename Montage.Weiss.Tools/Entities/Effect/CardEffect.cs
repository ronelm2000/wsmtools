namespace Montage.Weiss.Tools.Entities.Effect;

public abstract record CardEffect
{
    public abstract string Type { get; }
    public required string[] Labels { get; init; }
    public required string EffectText { get; set; }
    public required string AbilityText { get; set; }
    public required List<CardEffectAbility> Abilities { get; init; }
    public string ReminderText { get; set; } = string.Empty;
    public List<string> TokenLog { get; init; } = [];
}
