namespace Montage.Weiss.Tools.Entities.Effect;

/// <summary>
/// Represents a tree of card effects parsed from Japanese effect text.
/// For usage examples, see <see cref="Montage.Weiss.Tools.Test.Internal.CardEffectTreeTests"/>.
/// </summary>
public record CardEffectTree
{
    public required List<CardEffect> Effects { get; set; }
}
