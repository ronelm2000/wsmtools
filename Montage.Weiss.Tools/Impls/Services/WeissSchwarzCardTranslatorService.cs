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

        // Register ability tokens
        _effectListRegistry.Register(new PowerBoostToken());
        _effectListRegistry.Register(new AssistPowerBoostToken());
        _effectListRegistry.Register(new PutCardFromHandToWaitingRoomToken());
        _effectListRegistry.Register(new MayPayCostToken());
        _effectListRegistry.Register(new RevealTopCardToken());
        _effectListRegistry.Register(new ChooseCharacterFromWaitingRoomToken());

        // Register effect type tokens
        _effectRegistry.Register(new ContEffectToken());
        _effectRegistry.Register(new AssistContEffectToken());
        _effectRegistry.Register(new AutoEffectToken());

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

        return value.Split('】', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.TrimStart('【'))
            .ToArray();
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
            reminderTextEnglish = string.Join(". ", translated) + ".";
        }

        var effect = _effectRegistry.GetMatch(japaneseEffectText)(this);
        effect.ReminderText = reminderTextEnglish;

        // Auto-include in EffectText
        if (!string.IsNullOrEmpty(reminderTextEnglish))
        {
            effect.EffectText = effect.EffectText.TrimEnd('.') + " " + reminderTextEnglish;
        }

        return new CardEffectTree
        {
            Effects = [effect]
        };
    }
}
