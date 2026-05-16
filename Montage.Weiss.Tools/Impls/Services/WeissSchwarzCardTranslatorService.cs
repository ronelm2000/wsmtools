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
        // Register condition tokens (most to least specific)
        _conditionListRegistry.Register(new ReverseAndOpponentLevelConditionToken());
        _conditionListRegistry.Register(new TurnAndTraitCharacterCountConditionToken());
        _conditionListRegistry.Register(new AllCharactersAreTraitConditionToken());
        _conditionListRegistry.Register(new AnotherCharacterWithTraitExistsConditionToken());
        _conditionListRegistry.Register(new DuringOpponentTurnAllCharactersAreTraitConditionToken());
        _conditionListRegistry.Register(new DuringTurnConditionToken());
        _conditionListRegistry.Register(new DuringOpponentTurnConditionToken());
        _conditionListRegistry.Register(new DuringTurnAllCharactersAreTraitConditionToken());
        _conditionListRegistry.Register(new DuringTurnPlacedFromHandConditionToken());
        _conditionListRegistry.Register(new DamageCanceledConditionToken());
        _conditionListRegistry.Register(new CxWithTriggerIconInCxAreaConditionToken());
        _conditionListRegistry.Register(new YourCxWithTriggerIconTriggeredConditionToken());
        _conditionListRegistry.Register(new CxPlacedConditionToken());
        _conditionListRegistry.Register(new CardPlacedFromHandConditionToken());
        _conditionListRegistry.Register(new ReverseConditionToken());
        _conditionListRegistry.Register(new BattleOpponentReverseConditionToken());
        _conditionListRegistry.Register(new FacingCharacterColorConditionToken());
        _conditionListRegistry.Register(new FacingCharacterLevelConditionToken());
        _conditionListRegistry.Register(new DuringTurnFacingCharacterColorConditionToken());
        _conditionListRegistry.Register(new OpponentLevelConditionToken());
        _conditionListRegistry.Register(new LevelConditionToken());
        _conditionListRegistry.Register(new InClockConditionToken());
        _conditionListRegistry.Register(new TraitCharacterCountConditionToken());
        _conditionListRegistry.Register(new ExperienceConditionToken());
        _conditionListRegistry.Register(new TraitCountConditionToken());
        _conditionListRegistry.Register(new AnotherTraitNotExistsConditionToken());
        _conditionListRegistry.Register(new AnotherSpecificCardExistsConditionToken());
        _conditionListRegistry.Register(new AnotherSpecificCardNotExistsConditionToken());
        _conditionListRegistry.Register(new TurnOnceConditionToken());
        _conditionListRegistry.Register(new HandSizeConditionToken());
        _conditionListRegistry.Register(new StockCountConditionToken());
        _conditionListRegistry.Register(new CenterStageConditionToken());
        _conditionListRegistry.Register(new FrontalAttackedConditionToken());
        _conditionListRegistry.Register(new AttackConditionToken());
        _conditionListRegistry.Register(new CxPhaseStartConditionToken());
        _conditionListRegistry.Register(new OpponentAttackPhaseStartConditionToken());
        _conditionListRegistry.Register(new CannotBeChosenConditionToken());
        _conditionListRegistry.Register(new WhenBackupUsedConditionToken());
        _conditionListRegistry.Register(new CxNamedInCxAreaConditionToken());
        _conditionListRegistry.Register(new CxNamedPlacedConditionToken());
        _conditionListRegistry.Register(new TriggerCheckRevealsCxWithIconConditionToken());
        _conditionListRegistry.Register(new CardPlacedToWaitingRoomFromStageConditionToken());
        _conditionListRegistry.Register(new NoTraitExistsConditionToken());
        _conditionListRegistry.Register(new EncoreStepStartConditionToken());
        _conditionListRegistry.Register(new ThisCardMarkerCountConditionToken());
        _conditionListRegistry.Register(new ThisCardMarkerUnderneathConditionToken());
        _conditionListRegistry.Register(new MarkerUnderneathConditionToken());
        _conditionListRegistry.Register(new FacingCharacterConditionToken());
      _conditionListRegistry.Register(new ThisCardMarkerUnderneathConditionToken());
      _conditionListRegistry.Register(new ThisCardMarkerCountConditionToken());

        // Register ability tokens (most to least specific)
        _effectListRegistry.Register(new StockCostWithPutCardFromHandToWaitingRoomToken());
        _effectListRegistry.Register(new StockCostWithChooseCardAndPutToWaitingRoomToken());
        _effectListRegistry.Register(new StockCostWithCxDiscardToken());
        _effectListRegistry.Register(new StockCostToken());
        _effectListRegistry.Register(new PowerBoostWithFollowingAbilityToken());
        _effectListRegistry.Register(new GiveMultipleAbilitiesToken());
        _effectListRegistry.Register(new DuringBattleCannotPlayEventsOrBackupToken());
        _effectListRegistry.Register(new AllCharactersBoostToken());
        _effectListRegistry.Register(new AllCharactersSoulBoostToken());
        _effectListRegistry.Register(new AssistPowerBoostToken());
        _effectListRegistry.Register(new ChooseTraitCharacterAndPowerBoostToken());
        _effectListRegistry.Register(new BrainstormToken());
        _effectListRegistry.Register(new RevealTopCardsToken());
        _effectListRegistry.Register(new RevealTopCardWithPrefixToken());
        _effectListRegistry.Register(new RevealTopCardToken());
        _effectListRegistry.Register(new ClockToWaitingRoomToken());
        _effectListRegistry.Register(new PutCardFromHandToWaitingRoomToken());
        _effectListRegistry.Register(new CostPutTriggerCxFromHandToWaitingRoomToken());
        _effectListRegistry.Register(new CostPutCxFromCxAreaToWaitingRoomToken());
        _effectListRegistry.Register(new CostPutCxFromHandToWaitingRoomToken());
        _effectListRegistry.Register(new CostPutCardFromHandToMemoryToken());
        _effectListRegistry.Register(new CostPutCardFromHandToClockToken());
        _effectListRegistry.Register(new CostPutTraitCharacterFromHandToWaitingRoomToken());
        _effectListRegistry.Register(new CostPutTraitCharacterFromStageToWaitingRoomToken());
        _effectListRegistry.Register(new CostRestTraitCharactersToken());
        _effectListRegistry.Register(new CostPutTopOfStockToClockAndRestToken());
        _effectListRegistry.Register(new CostPutTopOfStockToClockToken());
        _effectListRegistry.Register(new CostPutTopOfStockToClockStockVariantToken());
        _effectListRegistry.Register(new CostPutBlueCharacterFromWaitingRoomToClockBottomToken());
        _effectListRegistry.Register(new ChooseCharacterFromWaitingRoomToken());
        _effectListRegistry.Register(new LookAtTopCardsToken());
        _effectListRegistry.Register(new ChooseCardsToken());
        _effectListRegistry.Register(new ChooseTraitCharacterToken());
        _effectListRegistry.Register(new SearchDeckWithTopLookToken());
        _effectListRegistry.Register(new SearchDeckToken());
        _effectListRegistry.Register(new ChooseFromWaitingRoomAndReturnToken());
        _effectListRegistry.Register(new ChooseOtherCharacterAndGiveAbilityToken());
        _effectListRegistry.Register(new ChooseCharacterAndBoostToken());
        _effectListRegistry.Register(new OpponentChooseCxAndShuffleToken());
        _effectListRegistry.Register(new RevealToOpponentToken());
        _effectListRegistry.Register(new ConditionalPutInHandToken());
        _effectListRegistry.Register(new ChooseCardAndPutInWaitingRoomToken());
        _effectListRegistry.Register(new PutInHandToken());
        _effectListRegistry.Register(new ReturnMultipleToHandToken());
        _effectListRegistry.Register(new PutCharacterToBottomOfDeckToken());
        _effectListRegistry.Register(new PlaceOnStageToken());
        _effectListRegistry.Register(new MayPayCostThenAbilityToken());
        _effectListRegistry.Register(new MayPayCostToken());
        _effectListRegistry.Register(new DuringTurnPowerBoostFromCIPToken());
        _effectListRegistry.Register(new DuringTurnPowerBoostToken());
        _effectListRegistry.Register(new SimplePowerBoostToken());
        _effectListRegistry.Register(new PowerBoostToken());
        _effectListRegistry.Register(new SoulBoostToken());
        _effectListRegistry.Register(new DealDamageToken());
        _effectListRegistry.Register(new GiveAbilitiesToken());
        _effectListRegistry.Register(new GainFollowingAbilityToken());
        _effectListRegistry.Register(new GainFollowingAbilityTokenWithParticleWa());
        _effectListRegistry.Register(new XEqualsToken());
        _effectListRegistry.Register(new ThoseCardsTriggerIconConditionToken());
        _effectListRegistry.Register(new EncoreToken());
        _effectListRegistry.Register(new CannotSideAttackToken());
        _effectListRegistry.Register(new CannotPlayBackupDuringBattleToken());
        _effectListRegistry.Register(new HandLevelMinusToken());
        _effectListRegistry.Register(new AllZonesTriggerIconGainToken());
        _effectListRegistry.Register(new PerformTriggerIconEffectAbilityToken());
        _effectListRegistry.Register(new CannotPlayEventsOrBackupFromHandToken());
        _effectListRegistry.Register(new AllTraitCharactersBoostToken());
        _effectListRegistry.Register(new AllOtherTraitCharactersBoostToken());
        _effectListRegistry.Register(new AllOtherCharactersBoostToken());
        _effectListRegistry.Register(new TraitGainToken());
        _effectListRegistry.Register(new LevelAndPowerBoostToken());
        _effectListRegistry.Register(new CannotPlayFromHandToken());
        _effectListRegistry.Register(new ColorConditionIgnorePlayToken());
        _effectListRegistry.Register(new CostReductionToken());
        _effectListRegistry.Register(new AllOtherTriggerIconGrantToken());
        _effectListRegistry.Register(new AllZonesCxTriggerIconGainToken());
        _effectListRegistry.Register(new FrontCharactersAllBoostToken());
        _effectListRegistry.Register(new GiveEncoreToOpponentCharactersToken());
        _effectListRegistry.Register(new PutCxFromHandToWaitingRoomRestThisCardToken());
        _effectListRegistry.Register(new RestThisCardToken());
        _effectListRegistry.Register(new PutThisCardToWaitingRoomToken());
        _effectListRegistry.Register(new ReturnThisCardToHandToken());
        _effectListRegistry.Register(new RestAnyCharactersToken());
        _effectListRegistry.Register(new RestTraitCharactersToken());
        _effectListRegistry.Register(new PutCardToWaitingRoomAndThisToWaitingRoomToken());
        _effectListRegistry.Register(new PutCardFromHandAndThisToBottomToken());
        _effectListRegistry.Register(new RestAndPutToWaitingRoomToken());
        _effectListRegistry.Register(new StockCostWithPutCardToWaitingRoomToken());
        _effectListRegistry.Register(new PutThisCardToStockAndSwapBottomToken());
        _effectListRegistry.Register(new PutTopCardToWaitingRoomToken());
        _effectListRegistry.Register(new PutToStockToken());
        _effectListRegistry.Register(new IfYouDoToken());
        _effectListRegistry.Register(new PutBottomOfStockToWaitingRoomToken());
        _effectListRegistry.Register(new MoveToOpenPositionToken());
        _effectListRegistry.Register(new ShuffleDeckToken());
        _effectListRegistry.Register(new BackupPrefixToken());
        _effectListRegistry.Register(new OpponentPutToClockToken());
        _effectListRegistry.Register(new OpponentChooseReturnToHandToken());
        _effectListRegistry.Register(new AllCharactersSoulBoostTurnToken());
        _effectListRegistry.Register(new DealVariableDamageToken());
        _effectListRegistry.Register(new CannotBeChosenAbilityToken());
        
        // Register effect type tokens (most to least specific)
        _effectRegistry.Register(new AssistContEffectToken());
        _effectRegistry.Register(new ContEffectToken());
        _effectRegistry.Register(new AutoEffectToken());
        _effectRegistry.Register(new ActEffectToken());
        _effectRegistry.Register(new CounterEffectToken());
        _effectRegistry.Register(new BrainstormEffectToken());
        _effectRegistry.Register(new EventEffectToken());

        // Register reminder text tokens
        _reminderTextRegistry.Register(new CxLevelZeroToken());
        _reminderTextRegistry.Register(new ReturnToOriginalPositionToken());
        _reminderTextRegistry.Register(new ReturnToOriginalPositionOtherwiseToken());
        _reminderTextRegistry.Register(new DamageMayBeCanceledToken());
        _reminderTextRegistry.Register(new BackupCounterReminderToken());
        _reminderTextRegistry.Register(new EncoreReminderPart1Token());
        _reminderTextRegistry.Register(new EncoreReminderPart2Token());
        _reminderTextRegistry.Register(new EncoreReminderToken());
    }

    public IComponentRegistry<List<CardEffectAbility>> EffectListRegistry => _effectListRegistry;
    public IComponentRegistry<List<CardEffectCondition>> ConditionListRegistry => _conditionListRegistry;
    public IComponentRegistry<CardEffect> EffectRegistry => _effectRegistry;
    public IComponentRegistry<string> ReminderTextRegistry => _reminderTextRegistry;

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
            "CXコンボ" => "CXCOMBO",
            "カウンター" => "COUNTER",
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
                    var translatedSentence = _reminderTextRegistry.GetMatch(sentence.AsMemory())(this);
                    translated.Add(translatedSentence);
                }
                catch (NotImplementedException)
                {
                    // Try to translate trigger icon format: [[icon.gif]]：text to English
                    var iconMatch = Regex.Match(sentence, @"^\[\[(?<icon>[^\]]+?)\]\]：");
                    if (iconMatch.Success)
                    {
                        var icon = iconMatch.Groups["icon"].Value;
                        var iconName = TriggerIconHelper.GetIconName(icon);
                        var iconText = sentence.Substring(iconMatch.Length);
                        var english = TranslateTriggerIconReminderText(icon, iconName, iconText);
                        if (english != null)
                            translated.Add(english);
                        else
                            translated.Add(sentence);
                    }
                    else
                    {
                        translated.Add(sentence);
                    }
                }
            }
            reminderTextEnglish = string.Join(". ", translated);
        }

     // Debug output
        System.Diagnostics.Debug.WriteLine($"TranslateEffect input: {japaneseEffectText}");

        var effect = string.IsNullOrEmpty(japaneseEffectText)
            ? new EventCardEffect
            {
                Labels = Array.Empty<string>(),
                EffectText = "",
                AbilityText = "",
                Abilities = [],
                ReminderText = reminderTextEnglish
            }
: _effectRegistry.GetMatch(japaneseEffectText.AsMemory())(this)
                 ?? new EventEffectToken().Translate(this, japaneseEffectText.AsMemory());

        effect.ReminderText = reminderTextEnglish;
        if (!string.IsNullOrEmpty(reminderTextEnglish))
        {
            var effectText = effect.EffectText.TrimEnd('.');
            if (!string.IsNullOrEmpty(effectText))
            {
                effect.EffectText = effectText + ". (" + reminderTextEnglish + ")";
            }
            else
            {
                effect.EffectText = reminderTextEnglish;
            }
        }

       return new CardEffectTree
        {
            Effects = [effect]
        };
    }

 private static string? TranslateTriggerIconReminderText(string icon, string iconName, string japaneseText)
    {
        return icon.ToLowerInvariant() switch
        {
            "gate.gif" => $"([{iconName}]: When this card triggers, you may choose 1 CX in your waiting room, and return it to your hand)",
            "choice.gif" => $"([{iconName}]: When this card triggers, you may choose a character with [SOUL] in its trigger icon in your waiting room, and return it to your hand or put it to your stock)",
            "shot.gif" => $"([{iconName}]: During this turn, when the next damage dealt by the attacking character that triggered this card is canceled, deal 1 damage to your opponent)",
            "treasure.gif" => $"([{iconName}]: When this card triggers, return this card to your hand. You may put the top card of your deck to your stock)",
            "standby.gif" => $"([{iconName}]: When this card triggers, you may choose 1 character with a level equal to or less than your level +1 in your waiting room, and put it on any position of your stage as[REST])",
            "salvage.gif" => $"([{iconName}]: When this card triggers, you may choose 1 character in your waiting room, and return it to your hand)",
            "stock.gif" => $"([{iconName}]: When this card triggers, you may put the top card of your deck to your stock)",
            _ => null
        };
    }
}
