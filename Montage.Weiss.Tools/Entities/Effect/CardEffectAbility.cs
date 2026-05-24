using System.Text.Json.Serialization;

namespace Montage.Weiss.Tools.Entities.Effect;

public enum AbilityPrefix
{
    /// <summary>Default. Actions joined by comma: "do X, do Y, do Z."</summary>
    And,
    /// <summary>`し、` / `て、` — continuative "and also". Same output as And but semantically distinct.</summary>
    Continuation,
    /// <summary>`そうしたら` / `そうすれば` — "if you do" / "then". Output: "If you do, ..."</summary>
    IfYouDo,
    /// <summary>`そうでないなら` / `そうでなければ` / `そうしなければ` — "otherwise" / "if not". Output: "Otherwise, ..."</summary>
    Otherwise,
    /// <summary>`その後` — "after that" / "then". Output: "After that, ..."</summary>
    AfterThat,
    /// <summary>`あなたは` / `自分の` — subject prefix with no special conjunction. Output: "you ..."</summary>
    Subject,
}

[JsonDerivedType(typeof(UnmatchedAbility), "Unmatched")]
[JsonDerivedType(typeof(ConditionalCardEffectAbility), "Conditional")]
[JsonDerivedType(typeof(NestedCardEffectAbility), "NestedAbility")]
[JsonDerivedType(typeof(CardEffectAbility), "Plain")]
public record CardEffectAbility : ICardAbility
{
    public virtual required string AbilityText { get; init; }
    public AbilityPrefix Prefix { get; init; } = AbilityPrefix.And;
    public bool IsUnmatched { get; init; } = false;

    public static CardEffectAbility operator +(CardEffectAbility a, CardEffectAbility b)
    {
        var connector = b.Prefix switch
        {
            AbilityPrefix.IfYouDo => ". If you do, ",
            AbilityPrefix.Otherwise => ". Otherwise, ",
            AbilityPrefix.AfterThat => ". After that, ",
            AbilityPrefix.Continuation => ", and ",
            AbilityPrefix.Subject => ", ",
            _ => ", ",
        };
        return new CardEffectAbility
        {
            AbilityText = $"{a.AbilityText}{connector}{char.ToLower(b.AbilityText[0]) + b.AbilityText[1..]}"
        };
    }
}