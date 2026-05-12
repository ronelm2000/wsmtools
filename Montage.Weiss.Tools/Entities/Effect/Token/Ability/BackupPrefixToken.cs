namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class BackupPrefixToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"助太刀(\d+) レベル(\d+)\s*［(?:\((\d+)\)\s*)?手札のこのカードを控え室に置く］");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var backupPower = match.Groups[1].Value;
        var level = match.Groups[2].Value;
        var stockCost = match.Groups[3].Success ? $"({match.Groups[3].Value}) " : "";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"backup {backupPower}, Level {level} [{stockCost}Put this card in your hand to your waiting room]"
            }
        ];
    }
}
