using System.Text.Json.Serialization;

namespace Montage.Weiss.Tools.Entities.Effect;

[JsonDerivedType(typeof(AutoCardEffect), "Auto")]
[JsonDerivedType(typeof(ActCardEffect), "Act")]
[JsonDerivedType(typeof(ContCardEffect), "Cont")]
[JsonDerivedType(typeof(EventCardEffect), "Event")]
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
