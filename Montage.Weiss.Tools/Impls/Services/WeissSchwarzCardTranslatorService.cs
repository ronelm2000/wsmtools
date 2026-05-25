using Montage.Weiss.Tools.Entities.Effect;
using Montage.Weiss.Tools.Entities.Effect.Token;
using Montage.Weiss.Tools.Entities.Effect.Token.Ability;
using Montage.Weiss.Tools.Entities.Effect.Token.Condition;
using Montage.Weiss.Tools.Entities.Effect.Token.ReminderText;
using Montage.Weiss.Tools.Exceptions;

namespace Montage.Weiss.Tools.Impls.Services;

/// <summary>
/// Provides a service for converting effect text into <see cref="CardEffect"/>.
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
        _conditionListRegistry.Register(new DamageYouReceivedNotCanceledConditionToken());
        _conditionListRegistry.Register(new DamageCanceledConditionToken());
        _conditionListRegistry.Register(new DamageNotCanceledConditionToken());
        _conditionListRegistry.Register(new CxWithTriggerIconInCxAreaConditionToken());
        _conditionListRegistry.Register(new YourCxWithTriggerIconTriggeredConditionToken());
        _conditionListRegistry.Register(new CxPlacedConditionToken());
        _conditionListRegistry.Register(new CardPlacedFromHandOrAttackConditionToken());
        _conditionListRegistry.Register(new CardPlacedFromHandConditionToken());
        _conditionListRegistry.Register(new CardPlacedFromHandOrMemoryConditionToken());
        _conditionListRegistry.Register(new CardPlacedFromHandToCxAreaConditionToken());
        _conditionListRegistry.Register(new ReverseConditionToken());
        _conditionListRegistry.Register(new BattleOpponentReverseConditionToken());
        _conditionListRegistry.Register(new NoFacingCharacterOrReversedConditionToken());
        _conditionListRegistry.Register(new FacingCharacterColorConditionToken());
        _conditionListRegistry.Register(new FacingCharacterLevelConditionToken());
        _conditionListRegistry.Register(new DuringTurnFacingCharacterColorConditionToken());
        _conditionListRegistry.Register(new OpponentLevelConditionToken());
        _conditionListRegistry.Register(new LevelConditionToken());
        _conditionListRegistry.Register(new InClockConditionToken());
        _conditionListRegistry.Register(new OtherCharacterCountConditionToken());
        _conditionListRegistry.Register(new TraitCharacterCountConditionToken());
        _conditionListRegistry.Register(new CompoundCardInLevelConditionToken());
        _conditionListRegistry.Register(new ExperienceConditionToken());
        _conditionListRegistry.Register(new TraitCountConditionToken());
        _conditionListRegistry.Register(new AnotherTraitNotExistsConditionToken());
        _conditionListRegistry.Register(new AnotherSpecificCardExistsConditionToken());
        _conditionListRegistry.Register(new WhenYouPlayNamedCardConditionToken());
        _conditionListRegistry.Register(new AnotherSpecificCardNotExistsConditionToken());
        _conditionListRegistry.Register(new TurnOnceConditionToken());
        _conditionListRegistry.Register(new HandSizeConditionToken());
        _conditionListRegistry.Register(new StockCountConditionToken());
        _conditionListRegistry.Register(new OtherBackStageNamedCharacterCountConditionToken());
        _conditionListRegistry.Register(new CenterStageConditionToken());
        _conditionListRegistry.Register(new OtherNamedCharacterFrontalAttackedConditionToken());
        _conditionListRegistry.Register(new FrontalAttackedConditionToken());
        _conditionListRegistry.Register(new OtherCharacterAttackConditionToken());
        _conditionListRegistry.Register(new AttackConditionToken());
        _conditionListRegistry.Register(new CxPhaseStartConditionToken());
        _conditionListRegistry.Register(new OpponentAttackPhaseStartConditionToken());
        _conditionListRegistry.Register(new WhenBackupUsedConditionToken());
        _conditionListRegistry.Register(new CxNamedInCxAreaConditionToken());
        _conditionListRegistry.Register(new CxNamedPlacedConditionToken());
        _conditionListRegistry.Register(new TriggerCheckRevealsCxWithIconConditionToken());
        _conditionListRegistry.Register(new CardWithMarkerPlacedToWaitingRoomFromStageConditionToken());
        _conditionListRegistry.Register(new CardPlacedToWaitingRoomFromStageConditionToken());
        _conditionListRegistry.Register(new NoTraitExistsConditionToken());
        _conditionListRegistry.Register(new EncoreStepStartConditionToken());
        _conditionListRegistry.Register(new ThisCardMarkerCountConditionToken());
        _conditionListRegistry.Register(new ThisCardMarkerUnderneathConditionToken());
        _conditionListRegistry.Register(new MarkerNamedCharacterExistsConditionToken());
        _conditionListRegistry.Register(new MarkerUnderneathConditionToken());
        _conditionListRegistry.Register(new FacingCharacterConditionToken());
        _conditionListRegistry.Register(new FacingOpponentCharacterConditionToken());
        _conditionListRegistry.Register(new NoCardInMemoryConditionToken());
        _conditionListRegistry.Register(new MemoryCountConditionToken());
        _conditionListRegistry.Register(new CardInMemoryNamedConditionToken());
        _conditionListRegistry.Register(new CardExistsInMemoryConditionToken());
        _conditionListRegistry.Register(new BattleOpponentLevelConditionToken());
        _conditionListRegistry.Register(new DrawPhaseStartConditionToken());
        _conditionListRegistry.Register(new NoColorCardsInLevelConditionToken());
        _conditionListRegistry.Register(new IfThatCardIsLevelOrLowerCharacterOrToken());
        _conditionListRegistry.Register(new XEqualsConditionToken());
        _conditionListRegistry.Register(new XThresholdConditionToken());
        _conditionListRegistry.Register(new IfAddedToHandConditionToken());
        _conditionListRegistry.Register(new IfReturnedToHandConditionToken());
        _conditionListRegistry.Register(new IfLowerLevelCharacterAmongThoseCardsConditionToken());
        _conditionListRegistry.Register(new IfCxAmongThoseCardsConditionToken());
        _conditionListRegistry.Register(new OtherCenterStageSingleNamedCharacterConditionToken());
        _conditionListRegistry.Register(new OtherCenterStageNamedCharactersConditionToken());
        _conditionListRegistry.Register(new RevealedCardTypeConditionToken());
        _conditionListRegistry.Register(new RevealedCardLevelConditionToken());
        _conditionListRegistry.Register(new RevealedCardIsEventOrTraitConditionToken());
        _conditionListRegistry.Register(new DeckCountConditionToken());
        _conditionListRegistry.Register(new CardLevelConditionToken());
        _conditionListRegistry.Register(new LevelThresholdConditionToken());
        _conditionListRegistry.Register(new LevelIsConditionToken());
        _conditionListRegistry.Register(new CxAreaHasCxConditionToken());
        _conditionListRegistry.Register(new CxAreaCxWithIconConditionToken());
        _conditionListRegistry.Register(new CxAreaNamedConditionToken());
        _conditionListRegistry.Register(new MultiConditionAndConnectiveToken());
        _conditionListRegistry.Register(new YourCharacterReverseConditionToken());
        _conditionListRegistry.Register(new YourReverseCharactersCountConditionToken());
        _conditionListRegistry.Register(new YourLevelOrLowerConditionToken());
        _conditionListRegistry.Register(new OpponentCenterStageCountConditionToken());
        _conditionListRegistry.Register(new WhenYouUseActConditionToken());
        _conditionListRegistry.Register(new YourAttackPhaseStartConditionToken());
        _conditionListRegistry.Register(new YourDrawPhaseStartConditionToken());
        _conditionListRegistry.Register(new NoRestCharacterInCenterStageConditionToken());
        _conditionListRegistry.Register(new OtherCharacterPlacedFromHandConditionToken());
        _conditionListRegistry.Register(new MarkerUnderCharacterNotExistsConditionToken());
        _conditionListRegistry.Register(new CatchAllConditionToken());

        // Register ability tokens (most to least specific)
        _effectListRegistry.Register(new StockCostWithRestThisCardToken());
        _effectListRegistry.Register(new StockCostWithPutCardFromHandToWaitingRoomToken());
        _effectListRegistry.Register(new StockCostWithChooseCardAndPutToWaitingRoomToken());
        _effectListRegistry.Register(new StockCostToken());
        _effectListRegistry.Register(new StockCostWithCxDiscardToken());
        _effectListRegistry.Register(new PowerBoostWithFollowingAbilitiesToken());
        _effectListRegistry.Register(new PowerBoostWithFollowingAbilityToken());
        _effectListRegistry.Register(new PowerBoostGainEncoreToken());
        _effectListRegistry.Register(new ThatCharacterPowerAndSoulBoostToken());
        _effectListRegistry.Register(new PowerAndSoulBoostToken());
        _effectListRegistry.Register(new PowerBoostWithDurationToken());
        _effectListRegistry.Register(new GiveMultipleAbilitiesToken());
        _effectListRegistry.Register(new DuringBattleCannotPlayEventsOrBackupToken());
        _effectListRegistry.Register(new AllCharactersBoostToken());
        _effectListRegistry.Register(new AllCharactersSoulBoostToken());
        _effectListRegistry.Register(new AssistPowerBoostToken());
        _effectListRegistry.Register(new PowerBoostPerOtherNikkeToken());
        _effectListRegistry.Register(new OpponentFrontRowCharacterMinusPowerToken());
        _effectListRegistry.Register(new PowerBoostPerOpponentRestToken());
        _effectListRegistry.Register(new PowerBoostPerTraitCharacterToken());
        _effectListRegistry.Register(new ShotDamageBoostToken());
        _effectListRegistry.Register(new ChooseTraitCharacterRestAndMoveToOpenBackRowToken());
        _effectListRegistry.Register(new ChooseTraitCharacterAndPowerBoostToken());
        _effectListRegistry.Register(new ChooseMarkerAndThisCardAndPlaceAsMarkerToken());
        _effectListRegistry.Register(new BrainstormToken());
        _effectListRegistry.Register(new RevealUpToNFromDeckChooseTraitOrEventAddToHandToken());
        _effectListRegistry.Register(new RevealTopCardsToken());
        _effectListRegistry.Register(new RevealTopCardAndIfEventOrTraitAddToHandToken());
        _effectListRegistry.Register(new RevealTopCardAndIfTraitAddToHandAndDiscardToken());
        _effectListRegistry.Register(new RevealTopCardAndIfTraitAddToHandToken());
        _effectListRegistry.Register(new RevealTopCardWithPrefixToken());
        _effectListRegistry.Register(new RevealTopCardToken());
        _effectListRegistry.Register(new RevealTopCardIfTraitAddToHandToken());
        _effectListRegistry.Register(new ClockToWaitingRoomSimpleToken());
        _effectListRegistry.Register(new ClockToWaitingRoomToken());
        _effectListRegistry.Register(new PutCardFromHandAndThisToBottomToken());
        _effectListRegistry.Register(new PutCardFromHandToWaitingRoomToken());
        _effectListRegistry.Register(new CostPutTriggerCxFromHandToWaitingRoomToken());
        _effectListRegistry.Register(new CostPutCxFromCxAreaToWaitingRoomToken());
        _effectListRegistry.Register(new CostPutCxFromHandToWaitingRoomToken());
        _effectListRegistry.Register(new CostPutCardFromHandToMemoryToken());
        _effectListRegistry.Register(new CostPutCardFromHandToClockToken());
        _effectListRegistry.Register(new CostPutTraitCharacterFromHandToWaitingRoomToken());
        _effectListRegistry.Register(new CostPutTraitCharactersFromStageToWrToken());
        _effectListRegistry.Register(new CostPutTraitCharacterFromStageToWaitingRoomToken());
        _effectListRegistry.Register(new CostRestTraitCharactersToken());
        _effectListRegistry.Register(new CostRestStandNikkeCharacterToken());
        _effectListRegistry.Register(new CostRestTwoNikkeCharactersToken());
        _effectListRegistry.Register(new CostPutToStockAndSwapBottomToken());
        _effectListRegistry.Register(new StockSalvageThenDiscardToStockBottomToken());
        _effectListRegistry.Register(new CostPutTopOfStockToClockAndRestToken());
        _effectListRegistry.Register(new CostPutTopOfStockToClockToken());
        _effectListRegistry.Register(new CostPutTopOfStockToClockStockVariantToken());
        _effectListRegistry.Register(new CostPutBlueCharacterFromWaitingRoomToClockBottomToken());
        _effectListRegistry.Register(new ChooseCharacterFromWaitingRoomToken());
        _effectListRegistry.Register(new LookAtDeckChooseCardRevealAddToHandRestToWrToken());
        _effectListRegistry.Register(new LookAtTopCardsToken());
        _effectListRegistry.Register(new PutOnTopThenTopToStockToken());
        _effectListRegistry.Register(new ChooseFromLookedAndPutOnTopToken());
        _effectListRegistry.Register(new ChooseOneFromManyEffectsToken());
        _effectListRegistry.Register(new ChooseCardsToken());
        _effectListRegistry.Register(new ChooseTraitCharacterToken());
        _effectListRegistry.Register(new ChooseTraitCharacterFromWrAndPutToStockToken());
        _effectListRegistry.Register(new SearchDeckWithTopLookToken());
        _effectListRegistry.Register(new SearchDeckForCxAndExchangeToken());
        _effectListRegistry.Register(new SearchDeckFromLookToken());
        _effectListRegistry.Register(new SearchDeckSimpleToken());
        _effectListRegistry.Register(new SearchDeckForNamedCardToken());
        _effectListRegistry.Register(new SearchDeckToken());
        _effectListRegistry.Register(new SearchDeckLevelAndCostToken());
        _effectListRegistry.Register(new SearchDeckLevelCostAndPlaceToken());
        _effectListRegistry.Register(new SearchLevelXOrLowerTraitToken());
        _effectListRegistry.Register(new SearchLevel0OrLowerToken());
        _effectListRegistry.Register(new ChooseFromWaitingRoomAndReturnToDeckToken());
        _effectListRegistry.Register(new ChooseOpponentCardsFromWrAndReturnToDeckToken());
        _effectListRegistry.Register(new ChooseFromWaitingRoomAndReturnToken());
        _effectListRegistry.Register(new ChooseYourOtherCenterStageLevel0OrLowerCharToWrToken());
        _effectListRegistry.Register(new ChooseOpponentCharToMemoryThenFromMemoryToStageToken());
        _effectListRegistry.Register(new ChooseOtherCharacterAndGiveAbilityToken());
        _effectListRegistry.Register(new AllCenterStageExceptThisCardGiveAbilityToken());
        _effectListRegistry.Register(new ChooseCharacterBoostAndGiveAbilityToken());
        _effectListRegistry.Register(new ChooseCharacterAndBoostToken());
        _effectListRegistry.Register(new ChooseCharacterAndLevelBoostToken());
        _effectListRegistry.Register(new ChooseCharacterAndGiveAbilityToken());
        _effectListRegistry.Register(new OpponentChooseCxAndShuffleToken());
        _effectListRegistry.Register(new RevealToOpponentToken());
        _effectListRegistry.Register(new ConditionalPutInHandToken());
        _effectListRegistry.Register(new ConditionalAbilityToken());
        _effectListRegistry.Register(new ChooseCardAndPutInWaitingRoomToken());
        _effectListRegistry.Register(new PutInHandToken());
        _effectListRegistry.Register(new ReturnMultipleToHandToken());
        _effectListRegistry.Register(new PutCharacterToBottomOfDeckToken());
        _effectListRegistry.Register(new PutCharacterToBottomOfOpponentDeckToken());
        _effectListRegistry.Register(new PutCharacterToTopOfDeckToken());
        _effectListRegistry.Register(new PutCharacterToClockToken());
        _effectListRegistry.Register(new PutThisCardToMemoryToken());
        _effectListRegistry.Register(new PutCharacterToMemoryToken());
        _effectListRegistry.Register(new PutThisCardToStockToken());
        _effectListRegistry.Register(new PlaceOnStageToken());
        _effectListRegistry.Register(new ReturnThisCardToStageAsRestToken());
        _effectListRegistry.Register(new MayPayCostThenAbilityToken());
        _effectListRegistry.Register(new MayPayCostToken());
        _effectListRegistry.Register(new DuringTurnPowerBoostFromCIPToken());
        _effectListRegistry.Register(new DuringTurnPowerBoostToken());
        _effectListRegistry.Register(new PlacedFromHandPowerBoostToken());
        _effectListRegistry.Register(new SimplePowerBoostToken());
        _effectListRegistry.Register(new PowerBoostToken());
        _effectListRegistry.Register(new GetsSoulToken());
        _effectListRegistry.Register(new SoulBoostToken());
        _effectListRegistry.Register(new SoulBoostOneToken());
        _effectListRegistry.Register(new DrawUpToAndDiscardSameNumberToken());
        _effectListRegistry.Register(new DrawAndDiscardToken());
        _effectListRegistry.Register(new DrawUpToNToken());
        _effectListRegistry.Register(new DrawCardToken());
        _effectListRegistry.Register(new CannotBeChosenAbilityToken());
        _effectListRegistry.Register(new StrikerAbilityToken());
        _effectListRegistry.Register(new DealFixedDamageToken());
        _effectListRegistry.Register(new DealDamageToken());
        _effectListRegistry.Register(new DealDamageXTimesToken());
        _effectListRegistry.Register(new DealXDamageToken());
        _effectListRegistry.Register(new GiveAbilitiesToken());
        _effectListRegistry.Register(new GainQuotedAbilityToken());
        _effectListRegistry.Register(new GainFollowingAbilityToken());
        _effectListRegistry.Register(new GainFollowingAbilityTokenWithParticleWa());
        _effectListRegistry.Register(new GainStandaloneFollowingAbilitiesToken());
        _effectListRegistry.Register(new GainStandaloneFollowingAbilityToken());
        _effectListRegistry.Register(new AfterThatAllCharactersGetAbilityToken());
        _effectListRegistry.Register(new GainFollowingAbilityWithDurationToken());
        _effectListRegistry.Register(new GainEncoreAbilityToken());
        // XEqualsToken moved to ConditionListRegistry as PostCondition
        // _effectListRegistry.Register(new XEqualsToken());
        _effectListRegistry.Register(new ThoseCardsTriggerIconConditionToken());
        _effectListRegistry.Register(new ForEachCxToken());
        _effectListRegistry.Register(new EncoreToken());
        _effectListRegistry.Register(new CannotBecomeReverseToken());
        _effectListRegistry.Register(new FacingCharacterCannotDealDamageToPlayerToken());
        _effectListRegistry.Register(new CannotDealDamageToPlayerToken());
        _effectListRegistry.Register(new CannotReduceSoulBySideAttackToken());
        _effectListRegistry.Register(new CannotSideAttackToken());
        _effectListRegistry.Register(new CannotPlayBackupDuringBattleToken());
        _effectListRegistry.Register(new CannotMoveToAnotherPositionToken());
        _effectListRegistry.Register(new StageLevelMinusToken());
        _effectListRegistry.Register(new HandLevelMinusToken());
        _effectListRegistry.Register(new AllZonesTriggerIconGainToken());
        _effectListRegistry.Register(new PerformTriggerIconEffectAbilityToken());
        _effectListRegistry.Register(new PerformFollowingActionToken());
        _effectListRegistry.Register(new CannotPlayEventsOrBackupFromHandToken());
        _effectListRegistry.Register(new AllTraitCharactersBoostToken());
        _effectListRegistry.Register(new AllOtherTraitCharactersBoostToken());
        _effectListRegistry.Register(new OtherNamedCharactersBoostToken());
        _effectListRegistry.Register(new AllOtherCharactersBoostToken());
        _effectListRegistry.Register(new TraitGainToken());
        _effectListRegistry.Register(new LevelAndPowerBoostToken());
        _effectListRegistry.Register(new LevelBoostToken());
        _effectListRegistry.Register(new CannotPlayFromHandToken());
        _effectListRegistry.Register(new OpponentCannotPlayEventsToken());
        _effectListRegistry.Register(new ColorConditionIgnorePlayToken());
        _effectListRegistry.Register(new PlayWithoutColorConditionToken());
        _effectListRegistry.Register(new ClockTopToHandToken());
        _effectListRegistry.Register(new CostReductionToken());
        _effectListRegistry.Register(new AllOtherTriggerIconGrantToken());
        _effectListRegistry.Register(new StandThisCardToken());
        _effectListRegistry.Register(new AllZonesCxTriggerIconGainToken());
        _effectListRegistry.Register(new FrontCharactersAllBoostToken());
        _effectListRegistry.Register(new GiveEncoreToOpponentCharactersToken());
        _effectListRegistry.Register(new PutCxFromHandToWaitingRoomRestThisCardToken());
        _effectListRegistry.Register(new RestThisCardToken());
        _effectListRegistry.Register(new PutThisCardToWaitingRoomToken());
        _effectListRegistry.Register(new ReturnThatCharacterToHandToken());
        _effectListRegistry.Register(new ReturnThisCardToHandToken());
        _effectListRegistry.Register(new RestAnyCharactersToken());
        _effectListRegistry.Register(new TurnOnceAbilityToken());
        _effectListRegistry.Register(new RestTraitCharactersToken());
        _effectListRegistry.Register(new RestStandCharacterToken());
        _effectListRegistry.Register(new PutCardToWaitingRoomAndThisToWaitingRoomToken());
        _effectListRegistry.Register(new RestAndPutToWaitingRoomToken());
        _effectListRegistry.Register(new StockCostWithPutCardToWaitingRoomToken());
        _effectListRegistry.Register(new PutThisCardToStockAndSwapBottomToken());
        _effectListRegistry.Register(new PutRemainingMarkersToWaitingRoomToken());
        _effectListRegistry.Register(new PutTopCardToWaitingRoomToken());
        _effectListRegistry.Register(new PutToStockToken());
        _effectListRegistry.Register(new IfYouDoToken());
        _effectListRegistry.Register(new PutBottomOfStockToWaitingRoomToken());
        _effectListRegistry.Register(new PutThatCharacterToStockToken());
        _effectListRegistry.Register(new ChooseOpponentCharWithLevelHigherAndPutToMemoryToken());
        _effectListRegistry.Register(new ChooseOpponentCenterStageLevelXOrLowerCharToWrToken());
        _effectListRegistry.Register(new MoveToOpenPositionAndPutOpponentCharactersToWrToken());
        _effectListRegistry.Register(new MoveToOpenPositionToken());
        _effectListRegistry.Register(new ShuffleDeckToken());
        _effectListRegistry.Register(new BackupPrefixToken());
        _effectListRegistry.Register(new OpponentPutToClockToken());
        _effectListRegistry.Register(new OpponentChooseReturnToHandToken());
        _effectListRegistry.Register(new OpponentDeckToWrAndWrToDeckToken());
        _effectListRegistry.Register(new ChooseMemoryCardsAndPutOthersToWrToken());
        _effectListRegistry.Register(new AllPlayersPerformActionToken());
        _effectListRegistry.Register(new AllCharactersSoulBoostTurnToken());
        _effectListRegistry.Register(new DealVariableDamageToken());
        _effectListRegistry.Register(new CannotBeChosenAbilityToken());
        _effectListRegistry.Register(new TriggerCheckTwoTimesToken());
        _effectListRegistry.Register(new PutClockToWrOrStockToken());
        _effectListRegistry.Register(new ExchangeLevelWithWrToken());
        _effectListRegistry.Register(new ChooseAndExchangeToken());
        _effectListRegistry.Register(new PutOpponentClockToWrToken());
        _effectListRegistry.Register(new RestIfCxExistsToken());
        _effectListRegistry.Register(new CannotUseActUntilEndOfTurnToken());
        _effectListRegistry.Register(new PutTopXCardsToWrToken());
        _effectListRegistry.Register(new ReverseThatCharacterToken());
        _effectListRegistry.Register(new OpponentCenterStageCost0OrLowerToBottomOfDeckToken());
        _effectListRegistry.Register(new ChooseClockCharToBottomOfDeckToken());
        _effectListRegistry.Register(new ReturnAllWrToDeckAndShuffleToken());
        _effectListRegistry.Register(new ChooseOpponentWrCardToTopOfDeckToken());
        _effectListRegistry.Register(new TopDeckToStockToken());
        _effectListRegistry.Register(new ChooseCharacterAndSoulBoostToken());
        _effectListRegistry.Register(new ChooseBattleCharacterAndSoulBoostToken());
        _effectListRegistry.Register(new ChooseWrLevelBelowAndPlaceOnStageToken());
        _effectListRegistry.Register(new MoveOpponentCharacterToken());
        _effectListRegistry.Register(new PutToStockInsteadOfWrToken());
        _effectListRegistry.Register(new PlaceOnOpenPositionAndDealDamageToken());
        _effectListRegistry.Register(new SearchDeckForCxToken());
        _effectListRegistry.Register(new ChooseCardFromHandAndPutToStockToken());
        
        // Register effect type tokens (most to least specific)
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
        _reminderTextRegistry.Register(new NotPutToStockReturnToOriginalToken());
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

    public string MatchNameFragment(string name) => name;

    public string[] MatchLabels(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return [];

        var labels = value.Split('】', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.TrimStart('【'))
            .ToArray();

        // Convert Japanese labels to English
        // Output format matches CSV label column: bracketed labels use [LABEL] format,
        // while keyword-style labels (like Brainstorm) remain bare.
        return labels.Select(label => label switch
        {
            "ターン1" => "[1/TURN]",
            "応援" => "[Assist]",
            "集中" => "Brainstorm",
            "CXコンボ" => "[CXCOMBO]",
            "カウンター" => "[COUNTER]",
            _ => label
        }).ToArray();
    }

   public CardEffect TranslateEffect(string japaneseEffectText)
    {
        // Extract reminder text (full-width parentheses at end)
        var reminderMatch = Regex.Match(japaneseEffectText, @"（(?<reminder>[^）]+)）\s*$");
        string reminderTextJapanese = string.Empty;
        string reminderTextEnglish = string.Empty;

        if (reminderMatch.Success)
        {
            reminderTextJapanese = reminderMatch.Groups["reminder"].Value;
            japaneseEffectText = japaneseEffectText.Replace(reminderMatch.Value, "").Trim();

            // Check if entire reminder is a trigger icon format (may span multiple sentences)
            var iconFullMatch = Regex.Match(reminderTextJapanese, @"^\[\[(?<icon>[^\]]+?)\]\]：");
            if (iconFullMatch.Success)
            {
                var icon = iconFullMatch.Groups["icon"].Value;
                var iconName = TriggerIconHelper.GetIconName(icon);
                var iconText = reminderTextJapanese.Substring(iconFullMatch.Length);
                var english = TranslateTriggerIconReminderText(icon, iconName, iconText);
                if (english != null)
                    reminderTextEnglish = english;
                else
                    reminderTextEnglish = reminderTextJapanese;
            }
            else
            {
                // Split by 。and translate each sentence
                var sentences = reminderTextJapanese.Split('。', StringSplitOptions.RemoveEmptyEntries);
                var translated = new List<string>();
                foreach (var sentence in sentences)
                {
                    var matchResult = _reminderTextRegistry.Match(sentence.AsMemory());
                    if (matchResult != null)
                    {
                        var translatedSentence = matchResult.Translate(this);
                        translated.Add(translatedSentence);
                    }
                    else
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
: _effectRegistry.Match(japaneseEffectText.AsMemory())?.Translate(this)
                 ?? new EventEffectToken().Translate(this, japaneseEffectText.AsMemory());

        effect.ReminderText = reminderTextEnglish;
        if (!string.IsNullOrEmpty(reminderTextEnglish))
        {
            var effectText = effect.EffectText.TrimEnd('.');
            if (!string.IsNullOrEmpty(effectText))
            {
                var separator = effectText.EndsWith(']') || effectText.EndsWith('"') ? " " : ". ";
                effect.EffectText = effectText + separator + "(" + reminderTextEnglish + ")";
            }
            else
            {
                effect.EffectText = reminderTextEnglish;
            }
        }

        var unmatchedConditions = effect is IConditionalCardEffect cond
            ? cond.Condition.Where(c => c.IsUnmatched).ToList()
            : [];
        var unmatchedAbilities = effect.Abilities.Where(a => a.IsUnmatched).ToList();
        var unmatchedCosts = effect is ICostedCardEffect costed
            ? costed.Cost.Where(a => a.IsUnmatched).ToList()
            : [];

        if (unmatchedConditions.Count > 0 || unmatchedAbilities.Count > 0 || unmatchedCosts.Count > 0)
        {
            throw new TranslationNotImplementedException(
                $"Unrecognized [condition(s): {string.Join(" / ", unmatchedConditions.Select(c => c.ConditionText))}]" +
                $"[ability(ies): {string.Join(" / ", unmatchedAbilities.Select(a => a.AbilityText))}]" +
                $"[cost(s): {string.Join(" / ", unmatchedCosts.Select(a => a.AbilityText))}]",
                effect);
        }

       return effect;
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
