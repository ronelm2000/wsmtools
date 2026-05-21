namespace Montage.Weiss.Tools.Entities.Effect;

/// <summary>
/// A <see cref="CardEffectAbility"/> that was not recognized by any registered token.
/// Carries brief <see cref="Suggestions"/> about what kind of token class may need to be created.
/// Designed to be JSON-serialized with only sparse diagnostic info.
/// </summary>
public record UnmatchedAbility : CardEffectAbility
{
    public string[] Suggestions { get; init; } = [];
}
