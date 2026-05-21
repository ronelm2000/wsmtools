namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ShotDamageBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたの)?\[\[(?<icon>.+?\.gif)\]\]の効果で与えるダメージを[＋\+](?<amount>\d+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var iconFile = match.Groups["icon"].Value;
        var amount = match.Groups["amount"].Value;
        var iconName = TriggerIconHelper.GetIconName(iconFile);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"The damage from the effect of your [{iconName}] gets ＋{amount}."
            }
        ];
    }
}
