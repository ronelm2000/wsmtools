using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Card.API.Helpers;
using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.Entities.Effect;
using Montage.Weiss.Tools.Entities.Effect.Token;
using Montage.Weiss.Tools.Impls.Services;
using Serilog;

namespace Montage.Weiss.Tools.Test;

[TestClass]
public class TranslatorServiceTests
{
    private static readonly ILogger Log = Serilog.Log.ForContext<TranslatorServiceTests>();
    private static readonly WeissSchwarzCardTranslatorService _service = new();

    /// <summary>
    /// List of excluded ability type names for the test "Registry_AbilitiesMustCaptureEndingPunctuations".
    /// These are ability tokens are classified to perform greedy captures that are known to not capture ending punctuations, and are excluded from that specific test.
    /// Do not add without proper justification and consideration of the implications on the translation results.
    /// </summary>
    private static readonly HashSet<string> ExcludedAbilityTypeNames =
    [
        "PowerBoostWithFollowingAbilityToken",
        "GiveMultipleAbilitiesToken",
        "EncoreToken",
        "BackupPrefixToken",
        "IfYouDoToken",
        "ChooseOtherCharacterAndGiveAbilityToken"
    ];

    public TestContext TestContext { get; set; }

    public static IEnumerable<(Type type, String regex)> GetTokenRegexValues()
    {
        var conditionList = _service.ConditionListRegistry.GetAllTokens()
                     .Select(t => (t.GetType(), t.Matcher.ToString()));

        var effectList = _service.EffectListRegistry.GetAllTokens()
            .Select(t => (t.GetType(), t.Matcher.ToString()));

        var effects = _service.EffectRegistry.GetAllTokens()
            .Select(t => (t.GetType(), t.Matcher.ToString()));

        var reminders = _service.ReminderTextRegistry.GetAllTokens()
            .Select(t => (t.GetType(), t.Matcher.ToString()));

        var all = conditionList.Concat(effectList).Concat(effects).Concat(reminders);
        return all;
    }
    public static IEnumerable<(Type type, String regex)> GetAbilityTokenRegexValues()
    {
        var effectList = _service.EffectListRegistry.GetAllTokens()
            .Select(t => (t.GetType(), t.Matcher.ToString()))
            .Where(t => !ExcludedAbilityTypeNames.Contains(t.Item1.Name));

        return effectList;
    }

    public static IEnumerable<(Type type, String regex)> GetAbilityTokenRegexValuesV2()
    {
        var effectList = _service.EffectListRegistry.GetAllTokens()
            .Select(t => (t.GetType(), t.Matcher.ToString()));

        return effectList;
    }

    public static IEnumerable<(string tokenName, CardTextToken<List<CardEffectCondition>> condition)> GetConditionTokenRegexValues()
        => _service.ConditionListRegistry.GetAllTokens()
            .Select(t => (t.GetType().Name, t));

    [TestMethod]
    [TestCategory("CI")]
    [DynamicData(nameof(GetTokenRegexValues))]
    public void Registry_RegexMustStartWithAnchor(Type type, string regex)
    {
        Assert.StartsWith("^", regex, $"Token {type.Name} regex must start with '^' anchor. Current regex: {regex}");
    }

    [TestMethod]
    [TestCategory("CI")]
    [DynamicData(nameof(GetAbilityTokenRegexValues))]
    public void Registry_AbilitiesMustCaptureEndingPunctuations(Type type, string regex)
    {
        Assert.EndsWith(@"(?:\.|,|、|。)?", regex, $"Token {type.Name} regex must end with an optional capture of all possible punctuations. Current regex: {regex}");
    }

    [TestMethod]
    [TestCategory("CI")]
    [DynamicData(nameof(GetAbilityTokenRegexValuesV2))]
    public void Registry_AbilityRegexMustNotContainSpaces(Type type, string regex)
    {
        Assert.DoesNotContain(" ", regex, $"Token {type.Name} regex must not contain spaces. Current regex: {regex}");
    }

    [TestMethod]
    [TestCategory("CI")]
    [DynamicData(nameof(GetConditionTokenRegexValues))]
    public void Registry_ConditionsMustBeAtomic(string tokenName, CardTextToken<List<CardEffectCondition>> condition)
    {
        var result = condition.Translate(_service, "dummy".AsMemory());
        Assert.IsTrue(result is not null);
        Assert.IsFalse(result!.Any(e => e.ConditionText.Contains("during", StringComparison.OrdinalIgnoreCase) && e.ConditionText.Contains("if", StringComparison.OrdinalIgnoreCase)),
            $"Token {tokenName} must be designed to capture atomic conditions without including conjunctions or multiple conditions. Result: {result.Select(e => e.ConditionText).ConcatAsString(";")}");
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_ContEffect_HandSizePowerBoost()
    {
        var japanese = "【永】 あなたの手札が5枚以上なら、このカードのパワーを＋2000。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as ContCardEffect;
        Assert.IsNotNull(effect);
        Assert.AreEqual("If you have 5 or more cards in your hand", effect.ConditionText);
        Assert.AreEqual("This card gets +2000 power.", effect.AbilityText);
        Assert.IsTrue(effect.EffectText.Contains("[CONT]"));
        Assert.IsTrue(effect.EffectText.Contains("+2000 power"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void MatchLabels_WithLabel_ReturnsLabels()
    {
        var result = _service.MatchLabels("【R】");
        Assert.AreEqual(1, result.Length);
        Assert.AreEqual("R", result[0]);
    }

    [TestMethod]
    [TestCategory("CI")]
    public void MatchLabels_Empty_ReturnsEmpty()
    {
        var result = _service.MatchLabels("");
        Assert.AreEqual(0, result.Length);
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_ContEffect_TurnAndTraitCharacterCountPowerBoost()
    {
        var japanese = "【永】 あなたのターン中、他のあなたの《風》のキャラが2枚以上なら、このカードのパワーを＋4000。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as ContCardEffect;
        Assert.IsNotNull(effect);
        Assert.AreEqual("During your turn, if you have 2 or more other <<風>> characters", effect.ConditionText);
        Assert.AreEqual("This card gets +4000 power.", effect.AbilityText);
        Assert.IsTrue(effect.EffectText.Contains("[CONT]"));
        Assert.IsTrue(effect.EffectText.Contains("+4000 power"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_ContEffect_Assist()
    {
        var japanese = "【永】 応援 このカードの前のあなたのキャラすべてに、パワーを＋Ｘ。Ｘはそのキャラのレベル×500に等しい。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as ContCardEffect;
        Assert.IsNotNull(effect);
        Assert.AreEqual("Assist", effect.Labels[0]);
        Assert.AreEqual("All of your characters in front of this card get +X power. X is equal to that character's level x500", effect.AbilityText);
        Assert.IsTrue(effect.EffectText.Contains("[CONT] Assist"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_ContEffect_Assist_1500()
    {
        var japanese = "【永】 応援 このカードの前のあなたのキャラすべてに、パワーを＋Ｘ。Ｘはそのキャラのレベル×1500に等しい。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as ContCardEffect;
        Assert.IsNotNull(effect);
        Assert.AreEqual("Assist", effect.Labels[0]);
        Assert.AreEqual("All of your characters in front of this card get +X power. X is equal to that character's level x1500", effect.AbilityText);
        Assert.IsTrue(effect.EffectText.Contains("[CONT] Assist"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_WithReminderText_SingleSentence()
    {
        var japanese = "【自】［手札を1枚控え室に置く］ あなたのCXがCX置場に置かれた時、あなたはコストを払ってよい。そうしたら、あなたは自分の山札の上から1枚を公開し、自分の控え室のレベルＸ以下のキャラを1枚選び、手札に戻す。Ｘは公開されたカードのレベルに等しい。（CXのレベルは0として扱う）";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0];
        Assert.IsFalse(string.IsNullOrEmpty(effect.ReminderText));
        Assert.IsTrue(effect.ReminderText.Contains("CX are regarded as level 0"));
        Assert.IsTrue(effect.EffectText.Contains("CX are regarded as level 0"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_WithReminderText_MultipleSentences()
    {
        var japanese = "【自】［手札を1枚控え室に置く］ あなたのCXがCX置場に置かれた時、あなたはコストを払ってよい。そうしたら、あなたは自分の山札の上から1枚を公開し、自分の控え室のレベルＸ以下のキャラを1枚選び、手札に戻す。Ｘは公開されたカードのレベルに等しい。（CXのレベルは0として扱う。公開したカードは元に戻す）";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0];
        Assert.IsFalse(string.IsNullOrEmpty(effect.ReminderText));
        Assert.IsTrue(effect.ReminderText.Contains("CX are regarded as level 0"));
        Assert.IsTrue(effect.ReminderText.Contains("Put it on its original place"));
        Assert.IsTrue(effect.EffectText.Contains("Put it on its original place"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_WithoutReminderText_ReminderTextEmpty()
    {
        var japanese = "【永】 あなたの手札が5枚以上なら、このカードのパワーを＋2000。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0];
        Assert.AreEqual(string.Empty, effect.ReminderText);
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_SoulBoost_Token()
    {
        var japanese = "【永】 あなたのキャラすべてに、ソウルを＋2。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as ContCardEffect;
        Assert.IsNotNull(effect);
        Assert.IsTrue(effect.AbilityText.Contains("+2 soul"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_AllCharactersBoost_Token()
    {
        var japanese = "【永】 あなたのキャラすべてに、パワーを＋1000し、ソウルを＋1。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as ContCardEffect;
        Assert.IsNotNull(effect);
        Assert.IsTrue(effect.AbilityText.Contains("+1000 power and +1 soul"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_Brainstorm_Token()
    {
        var japanese = "集中 あなたは自分の山札の上から3枚をめくり、控え室に置く。あなたは自分の控え室のレベルＸ以下の《風》のキャラを1枚選び、手札に戻す。Ｘはそれらのカードの《風》のキャラの枚数に等しい。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0];
        Assert.IsTrue(effect.EffectText.Contains("Brainstorm"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_ReverseCondition_Token()
    {
        var japanese = "【自】 このカードが【リバース】した時、このカードのバトル相手のレベルが0以下なら、あなたはそのキャラを山札の下に置いてよい。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as AutoCardEffect;
        Assert.IsNotNull(effect);
        Assert.IsTrue(effect.ConditionText.Contains("[REVERSE]"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_TurnOnce_Label()
    {
        var japanese = "【自】【ターン1】 このカードが手札から舞台に置かれたターン中、このカードの与えたダメージがキャンセルされた時、あなたは自分の山札の上から1枚を、控え室に置き、相手にＸダメージを与える。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as AutoCardEffect;
        Assert.IsNotNull(effect);
        Assert.IsTrue(effect.EffectText.Contains("[1/TURN]"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_OpponentLevelCondition_Token()
    {
        var japanese = "【自】 このカードが【リバース】した時、このカードのバトル相手のレベルが0以下なら、あなたはそのキャラを山札の下に置いてよい。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as AutoCardEffect;
        Assert.IsNotNull(effect);
        Assert.IsTrue(effect.ConditionText.Contains("level 0 or lower"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_DamageCancelledCondition_Token()
    {
        var japanese = "【自】【ターン1】 このカードが手札から舞台に置かれたターン中、このカードの与えたダメージがキャンセルされた時、あなたは自分の山札の上から1枚を、控え室に置き、相手にＸダメージを与える。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as AutoCardEffect;
        Assert.IsNotNull(effect);
        Assert.IsTrue(effect.ConditionText.Contains("damage is canceled"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_TraitCharacterCountCondition_Token()
    {
        var japanese = "【永】 他のあなたの《風》のキャラが2枚以上なら、このカードのパワーを＋2000。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as ContCardEffect;
        Assert.IsNotNull(effect);
        Assert.IsTrue(effect.ConditionText.Contains("<<風>> characters"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_ClockToWaitingRoom_Token()
    {
        var japanese = "【自】 このカードが手札から舞台に置かれた時、あなたは自分のクロックの上から1枚までを、控え室に置き、そのターン中、このカードのパワーを＋3000。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as AutoCardEffect;
        Assert.IsNotNull(effect);
        Assert.IsTrue(effect.AbilityText.Contains("clock to your waiting room"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_AutoEffect_TokenLogMustNotBeEmpty()
    {
        var japanese = "【自】 このカードが手札から舞台に置かれた時、あなたは自分のクロックの上から1枚までを、控え室に置き、そのターン中、このカードのパワーを＋3000。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as AutoCardEffect;
        Assert.IsNotNull(effect);
        Assert.IsNotEmpty(effect!.TokenLog);
    }


    [TestMethod]
    [TestCategory("CI")]
    public void Translate_ActEffect_TokenLogMustNotBeEmpty()
    {
        var japanese = "【起】［手札を1枚控え室に置き、このカードを控え室に置く］ あなたは自分の控え室の《NIKKE》のキャラを1枚選び、手札に戻す。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as ActCardEffect;
        Assert.IsNotNull(effect);
        Assert.IsNotEmpty(effect!.TokenLog);
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_LookAtTopCards_Token()
    {
        var japanese = "【自】 このカードが手札から舞台に置かれた時、あなたは自分の山札を上からＸ枚まで見て、カードを1枚まで選び、手札に加え、残りのカードを控え室に置く。Ｘはあなたの《風》のキャラの枚数に等しい。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as AutoCardEffect;
        Assert.IsNotNull(effect);
        Assert.AreEqual("[AUTO] When this card is placed on stage from your hand, look at up to X cards from the top of your deck, choose up to 1 card, put it to your hand, and put the rest to your waiting room. X is equal to the number of your <<風>> characters.", effect.EffectText);
        Assert.AreEqual("Look at up to X cards from the top of your deck, choose up to 1 card, put it to your hand, and put the rest to your waiting room. X is equal to the number of your <<風>> characters.", effect.AbilityText);
        Assert.IsTrue(effect.AbilityText.Contains("Look at up to X cards"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_DealDamage_Token()
    {
        var japanese = "【自】【ターン1】 このカードが手札から舞台に置かれたターン中、このカードの与えたダメージがキャンセルされた時、あなたは自分の山札の上から1枚を、控え室に置き、相手にＸダメージを与える。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as AutoCardEffect;
        Assert.IsNotNull(effect);
        Assert.AreEqual("[AUTO][1/TURN] During the turn this card was placed on stage from the hand, when this card's damage is canceled, put the top card of your deck to your waiting room, and deal X damage to your opponent. X is equal to that sent card's level +1.", effect.EffectText);
        Assert.AreEqual("Put the top card of your deck to your waiting room, and deal X damage to your opponent. X is equal to that sent card's level +1.", effect.AbilityText);
        Assert.IsTrue(effect.AbilityText.Contains("deal X damage to your opponent"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_DealDamage_Token_With_Reminder()
    {
        var japanese = "【自】【ターン1】 このカードが手札から舞台に置かれたターン中、このカードの与えたダメージがキャンセルされた時、あなたは自分の山札の上から1枚を、控え室に置き、相手にＸダメージを与える。Ｘはそのカードのレベル＋1に等しい。（CXのレベルは0として扱う。ダメージキャンセルは発生する）";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as AutoCardEffect;

        const string expectedReminder = "CX are regarded as level 0. Damage may be canceled";
        const string expectedEffectText = $"[AUTO][1/TURN] During the turn this card was placed on stage from the hand, " +
            $"when this card's damage is canceled, put the top card of your deck to your waiting room, " +
            $"and deal X damage to your opponent. X is equal to that sent card's level +1. ({expectedReminder})";
        const string expectedAbilityText = "Put the top card of your deck to your waiting room, " +
            "and deal X damage to your opponent. X is equal to that sent card's level +1.";
        Assert.IsNotNull(effect);
        Assert.AreEqual(expectedEffectText, effect.EffectText);
        Assert.AreEqual(expectedAbilityText, effect.AbilityText);
        Assert.AreEqual(expectedReminder, effect.ReminderText);
        Assert.IsTrue(effect.AbilityText.Contains("deal X damage to your opponent"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_ContEffect_TurnAndTraitCharacterCountPowerBoost_4()
    {
        var japanese = "【永】 あなたのターン中、他のあなたの《風》のキャラが4枚以上なら、このカードのパワーを＋5000。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as ContCardEffect;
        Assert.IsNotNull(effect);
        Assert.AreEqual("During your turn, if you have 4 or more other <<風>> characters", effect.ConditionText);
        Assert.IsTrue(effect.AbilityText.Contains("+5000 power"));
        Assert.IsTrue(effect.EffectText.Contains("[CONT]"));
        Assert.IsTrue(effect.EffectText.Contains("+5000 power"));
    }


    [TestMethod]
    [TestCategory("CI")]
    public void Translate_ContEffect_TokenLogMustNotBeEmpty()
    {
        var japanese = "【永】 あなたのターン中、他のあなたの《風》のキャラが4枚以上なら、このカードのパワーを＋5000。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as ContCardEffect;

        Assert.IsNotEmpty(effect!.TokenLog);
    }


    [TestMethod]
    [TestCategory("CI")]
    public void Translate_ContEffect_NonExistentConditionMustCrash()
    {
        var japanese = "【永】 あなたのターン中、他のあなたadded_textの《不存在》のキャラが4枚以上なら、このカードのパワーを＋5000。";
        Assert.ThrowsExactly<NotImplementedException>(() => _service.TranslateEffect(japanese));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_ContEffect_NonExistentAbilityMustCrash()
    {
        var japanese = "【永】 あなたのターン中、他のあなたadded_textの《不存在》のキャラが4枚以上なら、このカードのadded_textパワーを＋5000。";
        Assert.ThrowsExactly<NotImplementedException>(() => _service.TranslateEffect(japanese));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_AutoEffect_NonExistentConditionMustCrash()
    {
        var japanese = "【自】 あなたがこのカードの『non_existent_label』を使った時、あなたは自分の山札の上から1枚を公開する。そのカードが《風》のキャラなら手札に加え、あなたは自分の手札を1枚選び、控え室に置く。（そうでないなら元に戻す）";
        Assert.ThrowsExactly<NotImplementedException>(() => _service.TranslateEffect(japanese));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_AutoEffect_NonExistentAbilityMustCrash()
    {
        var japanese = "【自】 あなたがこのカードの『助太刀』を使った時、あなたは自分の山札の上から1枚枚枚枚枚を公開する。そのカードが《風》のキャラなら手札に加え、あなたは自分の手札を1枚選び、控え室に置く。（そうでないなら元に戻す）";
        Assert.ThrowsExactly<NotImplementedException>(() => _service.TranslateEffect(japanese));
    }

    [TestMethod]
    [TestCategory("CI")]
    [DynamicData(nameof(TranslateCsvCrossCheckAllData))]
    public void Translate_CSV_CrossCheckAll(string serial, string jpEffect, string enEffect, string labels)
    {
        CardEffectTree tree;
        try
        {
            tree = _service.TranslateEffect(jpEffect);
        }
        catch (Exception ex)
        {
            Assert.Fail($"[{serial}] Translation threw {ex.GetType().Name}: {ex.Message}");
            return;
        }

        Assert.IsTrue(tree.Effects.Count > 0, $"[{serial}] No translated effects were returned");

        var expected = enEffect.Trim();
        var actual = tree.Effects[0].EffectText.Trim();
        var expectedLabels = string.IsNullOrEmpty(labels)
            ? Array.Empty<string>()
            : labels.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (jpEffect.StartsWith("【自】") && tree.Effects[0] is not AutoCardEffect)
        {
            Log.Warning("[{serial}] resolves to {effectType} and needs to unify its function to AutoCardEffect.", serial, tree.Effects[0].GetType().Name);
        }
        else if (jpEffect.StartsWith("【永】") && tree.Effects[0] is not ContCardEffect)
        {
            Log.Warning("[{serial}] resolves to {effectType} and needs to unify its function to ContCardEffect.", serial, tree.Effects[0].GetType().Name);
        }
        else if (jpEffect.StartsWith("【起】") && tree.Effects[0] is not ActCardEffect)
        {
            Log.Warning("[{serial}] resolves to {effectType} and needs to unify its function to ActCardEffect.", serial, tree.Effects[0].GetType().Name);
        }

        Log.Debug("Full Effect: {@effect}", tree.Effects[0]);

        MultiAssert.AllAreTrue([
            () => Assert.AreEqual(expected, actual, $"[{serial}] EffectText mismatch{Environment.NewLine}Expected: {expected}{Environment.NewLine}Actual: {actual}"),
            () => CollectionAssert.AreEqual(expectedLabels, tree.Effects[0].Labels, $"[{serial}]{Environment.NewLine}Expected: {string.Join(", ", expectedLabels)}{Environment.NewLine}Actual: {string.Join(", ", tree.Effects[0].Labels)}{Environment.NewLine}Labels mismatched")
        ], Assert.Fail);
    }

    public static IEnumerable<TestDataRow<(string serial, string jpEffect, string enEffect, string labels)>> TranslateCsvCrossCheckAllData()
    {
        var resourcesDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Resources"));
        List<string> csvFilePaths = [
            "effects_550.csv",
            "expansion_494_effects.csv"
            ];
        var csvFiles = csvFilePaths
            .Select(fileName => Path.Combine(resourcesDirectory, fileName))
            .ToList();

        Assert.IsTrue(csvFiles.Count > 0, $"No cross-check CSV files found in: {resourcesDirectory}");

        foreach (var csvPath in csvFiles)
        {
            var csvFile = Path.GetFileName(csvPath);

            using var reader = new StreamReader(csvPath);
            var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null
            };
            using var csv = new CsvReader(reader, config);
            var rows = csv.GetRecords<EffectCsvRow>()
                .Where(r => !string.IsNullOrWhiteSpace(r.Serial)
                         && !string.IsNullOrWhiteSpace(r.JpEffect)
                         && !string.IsNullOrWhiteSpace(r.EnEffect));

            foreach (var row in rows)
            {
                var effectID = row.JpEffect.GetHashCode();
                yield return new ((row.Serial, row.JpEffect, row.EnEffect, row.Labels))
                {
                    TestCategories= new[] { "CI", row.Serial },
                    DisplayName = $"CSV-Cross-Check#{row.Serial}#{effectID}"
                };
            }
        }
    }

    private sealed record EffectCsvRow
    {
        [Name("serial")]
        public string Serial { get; set; } = "";
        [Name("jp_effect")]
        public string JpEffect { get; set; } = "";
        [Name("en_effect")]
        public string EnEffect { get; set; } = "";
        [Name("labels")]
        public string Labels { get; set; } = "";
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_ContEffect_TurnAndTraitCharacterCountPowerBoostAndGainSkill()
    {
        var japanese = "【永】 あなたのターン中、他のあなたの《風》のキャラが4枚以上なら、このカードのパワーを＋5000し、このカードは次の能力を得る。『【永】 このカードのバトル中、相手はイベントと『助太刀』を手札からプレイできない。』";
        var tree = _service.TranslateEffect(japanese);

        ContCardEffect? effect = null;

        MultiAssert.AllAreTrue([
            () => Assert.AreEqual(1, tree.Effects.Count),
            () => {
                effect = tree.Effects[0] as ContCardEffect;
                Assert.IsNotNull(effect);
            },
            () => Assert.AreEqual("During your turn, if you have 4 or more other <<風>> characters", effect!.ConditionText),
            () => Assert.IsTrue(effect!.AbilityText.Contains("+5000 power")),
            () => Assert.IsTrue(effect!.EffectText.Contains("[CONT]")),
            () => Assert.IsTrue(effect!.EffectText.Contains("+5000 power")),
            () => Assert.AreEqual("[CONT] During your turn, if you have 4 or more other <<風>> characters, this card gets +5000 power and the following ability. \"[CONT] During this card's battle, your opponent cannot play events or \"Backup\" from their hand.\"", effect!.EffectText),
        ], Assert.Fail);
    }
}
