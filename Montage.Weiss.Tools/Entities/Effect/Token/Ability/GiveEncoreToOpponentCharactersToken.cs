namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class GiveEncoreToOpponentCharactersToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手のすべてのキャラに、？『【自】 アンコール ［(.+?)］』を与える(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var costText = match.Groups[1].Value;
        string costEnglish;
        try
        {
            var costAbilities = registry.EffectListRegistry.GetMatch(costText.AsMemory())(registry);
            costEnglish = string.Join(", ", costAbilities.Select(a => a.AbilityText));
        }
        catch (NotImplementedException)
        {
            costEnglish = costText;
        }
        return
        [
            new CardEffectAbility
            {
                AbilityText = "All of your opponent's characters get \"[AUTO] Encore [" + costEnglish + "]\""
            }
        ];
    }
}
