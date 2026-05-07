using Montage.Weiss.Tools.Entities.Effect;
using Montage.Weiss.Tools.Entities.Effect.Token;
using Montage.Weiss.Tools.Entities.Effect.Token.Ability;
using Montage.Weiss.Tools.Entities.Effect.Token.Condition;
using Montage.Weiss.Tools.Entities.Effect.Token.ReminderText;

namespace Montage.Weiss.Tools.Impls.Services;

/// <summary>
/// Provides a service for converting effect text into <see cref="CardEffectTree"/>.
/// </summary>
public class WeissSchwarzCardTranslatorService : ITokenRegistry
{
    private readonly ComponentRegistry<List<CardEffectAbility>> _effectListRegistry = new();
    private readonly ComponentRegistry<List<CardEffectCondition>> _conditionListRegistry = new();
    private readonly ComponentRegistry<CardEffect> _effectRegistry = new();
    private readonly ComponentRegistry<string> _reminderTextRegistry = new();

    public WeissSchwarzCardTranslatorService()
    {
        RegisterDefaultTokens();
    }

    private void RegisterDefaultTokens()
    {
        // Register condition tokens
        _conditionListRegistry.Register(new HandSizeConditionToken());
        _conditionListRegistry.Register(new TurnAndTraitCharacterCountConditionToken());
        _conditionListRegistry.Register(new CxPlacedConditionToken());
        _conditionListRegistry.Register(new TurnOnceConditionToken());
        _conditionListRegistry.Register(new ReverseConditionToken());
        _conditionListRegistry.Register(new OpponentLevelConditionToken());
        _conditionListRegistry.Register(new DamageCancelledConditionToken());
        _conditionListRegistry.Register(new TraitCharacterCountConditionToken());
        _conditionListRegistry.Register(new CardPlacedFromHandConditionToken());
        _conditionListRegistry.Register(new DuringTurnPlacedFromHandConditionToken());

        // Register ability tokens
        _effectListRegistry.Register(new PowerBoostWithFollowingAbilityToken());
        _effectListRegistry.Register(new DuringBattleCannotPlayEventsOrBackupToken());
        _effectListRegistry.Register(new PowerBoostToken());
        _effectListRegistry.Register(new SoulBoostToken());
        _effectListRegistry.Register(new AllCharactersBoostToken());
        _effectListRegistry.Register(new AllCharactersSoulBoostToken());
        _effectListRegistry.Register(new AssistPowerBoostToken());
        _effectListRegistry.Register(new BrainstormToken());
        _effectListRegistry.Register(new PutCardFromHandToWaitingRoomToken());
        _effectListRegistry.Register(new MayPayCostToken());
        _effectListRegistry.Register(new RevealTopCardToken());
        _effectListRegistry.Register(new RevealTopCardsToken());
        _effectListRegistry.Register(new ChooseCharacterFromWaitingRoomToken());
        _effectListRegistry.Register(new PlaceOnStageToken());
        _effectListRegistry.Register(new ClockToWaitingRoomToken());
        _effectListRegistry.Register(new LookAtTopCardsToken());
        _effectListRegistry.Register(new DealDamageToken());
        _effectListRegistry.Register(new ReturnMultipleToHandToken());
        _effectListRegistry.Register(new PutCharacterToBottomOfDeckToken());
        _effectListRegistry.Register(new DuringTurnPowerBoostToken());
        _effectListRegistry.Register(new GiveAbilitiesToken());
        _effectListRegistry.Register(new GainFollowingAbilityToken());
        _effectListRegistry.Register(new GainFollowingAbilityTokenV2());
        _effectListRegistry.Register(new GainFollowingAbilityTokenV3());
        _effectListRegistry.Register(new MayPayCostThenAbilityToken());
        _effectListRegistry.Register(new DuringTurnPowerBoostFromCIPToken());
        _effectListRegistry.Register(new ChooseCardsToken());
        _effectListRegistry.Register(new PutInHandToken());
        _effectListRegistry.Register(new PutTopCardToWaitingRoomToken());
        _effectListRegistry.Register(new XEqualsToken());

        // Register effect type tokens
        _effectRegistry.Register(new ContEffectToken());
        _effectRegistry.Register(new AssistContEffectToken());
        _effectRegistry.Register(new AutoEffectToken());
        _effectRegistry.Register(new AutoCIPToken());
        _effectRegistry.Register(new BrainstormEffectToken());

        // Register reminder text tokens
        _reminderTextRegistry.Register(new CxLevelZeroToken());
        _reminderTextRegistry.Register(new ReturnToOriginalPositionToken());
        _reminderTextRegistry.Register(new ReturnToOriginalPositionOtherwiseToken());
        _reminderTextRegistry.Register(new DamageMayBeCancelledToken());
    }

    IComponentRegistry<List<CardEffectAbility>> ITokenRegistry.EffectListRegistry => _effectListRegistry;

    IComponentRegistry<List<CardEffectCondition>> ITokenRegistry.ConditionListRegistry => _conditionListRegistry;

    IComponentRegistry<CardEffect> ITokenRegistry.EffectRegistry => _effectRegistry;

    IComponentRegistry<string> ITokenRegistry.ReminderTextRegistry => _reminderTextRegistry;

    public string[] MatchLabels(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return [];

        var labels = value.Split('】', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.TrimStart('【'))
            .ToArray();

        // Convert Japanese labels to English
        return labels.Select(label => label switch
        {
            "ターン1" => "1/TURN",
            "応援" => "Assist",
            "集中" => "Brainstorm",
            _ => label
        }).ToArray();
    }

    public CardEffectTree TranslateEffect(string japaneseEffectText)
    {
        // Extract reminder text (full-width parentheses at end)
        var reminderMatch = Regex.Match(japaneseEffectText, @"（(?<reminder>[^）]+)）\s*$");
        string reminderTextJapanese = string.Empty;
        string reminderTextEnglish = string.Empty;

        if (reminderMatch.Success)
        {
            reminderTextJapanese = reminderMatch.Groups["reminder"].Value;
            japaneseEffectText = japaneseEffectText.Replace(reminderMatch.Value, "").Trim();

            // Split by 。and translate each sentence
            var sentences = reminderTextJapanese.Split('。', StringSplitOptions.RemoveEmptyEntries);
            var translated = new List<string>();
            foreach (var sentence in sentences)
            {
                try
                {
                    var translatedSentence = _reminderTextRegistry.GetMatch(sentence)(this);
                    translated.Add(translatedSentence);
                }
                catch (NotImplementedException)
                {
                    translated.Add(sentence);
                }
            }
            reminderTextEnglish = string.Join(". ", translated);
        }

        // Debug output
        System.Diagnostics.Debug.WriteLine($"TranslateEffect input: {japaneseEffectText}");

        var effect = _effectRegistry.GetMatch(japaneseEffectText)(this);
        effect.ReminderText = reminderTextEnglish;

        // Auto-include in EffectText
        if (!string.IsNullOrEmpty(reminderTextEnglish))
        {
            effect.EffectText = effect.EffectText.TrimEnd('.') + ". (" + reminderTextEnglish + ")";
        }

        return new CardEffectTree
        {
            Effects = [effect]
        };
    }
}
