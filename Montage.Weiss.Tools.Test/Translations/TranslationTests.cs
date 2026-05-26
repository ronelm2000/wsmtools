using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Card.API.Helpers;
using Montage.Weiss.Tools.Entities.Effect;
using Montage.Weiss.Tools.Impls.Services;
using Montage.Weiss.Tools.Exceptions;
using Serilog;

namespace Montage.Weiss.Tools.Test.Translations;

[TestClass]
public partial class TranslationTests
{
    private static readonly ILogger Log = Serilog.Log.ForContext<TranslationTests>();
    private static readonly WeissSchwarzCardTranslatorService _service = Global.Container.GetInstance<WeissSchwarzCardTranslatorService>();

    public TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_ContEffect_HandSizePowerBoost()
    {
        var japanese = "【永】 あなたの手札が5枚以上なら、このカードのパワーを＋2000。";
        var tree = _service.TranslateEffect(japanese);

        var effect = tree as ContCardEffect;
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

        var effect = tree as ContCardEffect;
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
        var japanese = "【永】 応援 このカードの前のあなたのキャラすべてに、パワーを＋X。X はそのキャラのレベル×500 に等しい。";
        var tree = _service.TranslateEffect(japanese);

        var effect = tree as ContCardEffect;
        Assert.IsNotNull(effect);
        Assert.AreEqual("Assist", effect.Labels[0]);
        Assert.AreEqual("All of your characters in front of this card get +X power. X is equal to that character's level ×500.", effect.AbilityText);
        Assert.IsTrue(effect.EffectText.Contains("[CONT]"));
        Assert.IsTrue(effect.EffectText.Contains("+X power"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_ContEffect_Assist_1500()
    {
        var japanese = "【永】 応援 このカードの前のあなたのキャラすべてに、パワーを＋X。X はそのキャラのレベル×1500 に等しい。";
        var tree = _service.TranslateEffect(japanese);

        var effect = tree as ContCardEffect;
        Assert.IsNotNull(effect);
        Assert.AreEqual("Assist", effect.Labels[0]);
        Assert.AreEqual("All of your characters in front of this card get +X power. X is equal to that character's level ×1500.", effect.AbilityText);
        Assert.IsTrue(effect.EffectText.Contains("[CONT]"));
        Assert.IsTrue(effect.EffectText.Contains("+X power"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_WithReminderText_SingleSentence()
    {
        var japanese = "【自】［手札を1枚控え室に置く］ あなたのCXがCX置場に置かれた時、あなたはコストを払ってよい。そうしたら、あなたは自分の山札の上から1枚を公開し、自分の控え室のレベルＸ以下のキャラを1枚選び、手札に戻す。Ｘは公開されたカードのレベルに等しい。（CXのレベルは0として扱う）";
        var tree = _service.TranslateEffect(japanese);

        var effect = tree;
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

        var effect = tree;
        Assert.IsFalse(string.IsNullOrEmpty(effect.ReminderText));
        Assert.IsTrue(effect.ReminderText.Contains("CX are regarded as level 0"));
        Assert.IsTrue(effect.ReminderText.Contains("Return the revealed card to its original place"));
        Assert.IsTrue(effect.EffectText.Contains("Return the revealed card to its original place"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_WithoutReminderText_ReminderTextEmpty()
    {
        var japanese = "【永】 あなたの手札が5枚以上なら、このカードのパワーを＋2000。";
        var tree = _service.TranslateEffect(japanese);

        var effect = tree;
        Assert.AreEqual(string.Empty, effect.ReminderText);
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_SoulBoost_Token()
    {
        var japanese = "【永】 あなたのキャラすべてに、ソウルを＋2。";
        var tree = _service.TranslateEffect(japanese);

        var effect = tree as ContCardEffect;
        Assert.IsNotNull(effect);
        Assert.IsTrue(effect.AbilityText.Contains("+2 soul"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_AllCharactersBoost_Token()
    {
        var japanese = "【永】 あなたのキャラすべてに、パワーを＋1000し、ソウルを＋1。";
        var tree = _service.TranslateEffect(japanese);

        var effect = tree as ContCardEffect;
        Assert.IsNotNull(effect);
        Assert.IsTrue(effect.AbilityText.Contains("+1000 power and +1 soul"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_Brainstorm_Token()
    {
        var japanese = "集中 あなたは自分の山札の上から3枚をめくり、控え室に置く。あなたは自分の控え室のレベルＸ以下の《風》のキャラを1枚選び、手札に戻す。Ｘはそれらのカードの《風》のキャラの枚数に等しい。";
        var tree = _service.TranslateEffect(japanese);

        var effect = tree;
        Assert.IsTrue(effect.EffectText.Contains("Brainstorm"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_ReverseCondition_Token()
    {
        var japanese = "【自】 このカードが【リバース】した時、このカードのバトル相手のレベルが 0 以下なら、あなたはそのキャラを山札の下に置いてよい。";
        var tree = _service.TranslateEffect(japanese);

        var effect = tree as AutoCardEffect;
        Assert.IsNotNull(effect);
        Assert.IsTrue(effect.ConditionText.Contains("[REVERSE]"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_TurnOnce_Label()
    {
        var japanese = "【自】【ターン1】 このカードが手札から舞台に置かれたターン中、このカードの与えたダメージがキャンセルされた時、あなたは自分の山札の上から1枚を、控え室に置き、相手にＸダメージを与える。";
        var tree = _service.TranslateEffect(japanese);

        var effect = tree as AutoCardEffect;
        Assert.IsNotNull(effect);
        Assert.IsTrue(effect.EffectText.Contains("[1/TURN]"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_OpponentLevelCondition_Token()
    {
        var japanese = "【自】 このカードが【リバース】した時、このカードのバトル相手のレベルが0以下なら、あなたはそのキャラを山札の下に置いてよい。";
        var tree = _service.TranslateEffect(japanese);

        var effect = tree as AutoCardEffect;
        Assert.IsNotNull(effect);
        Assert.IsTrue(effect.ConditionText.Contains("level 0 or lower"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_DamageCancelledCondition_Token()
    {
        var japanese = "【自】【ターン1】 このカードが手札から舞台に置かれたターン中、このカードの与えたダメージがキャンセルされた時、あなたは自分の山札の上から1枚を、控え室に置き、相手にＸダメージを与える。";
        var tree = _service.TranslateEffect(japanese);

        var effect = tree as AutoCardEffect;
        Assert.IsNotNull(effect);
        Assert.IsTrue(effect.ConditionText.Contains("dealt by this card is canceled"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_TraitCharacterCountCondition_Token()
    {
        var japanese = "【永】 他のあなたの《風》のキャラが2枚以上なら、このカードのパワーを＋2000。";
        var tree = _service.TranslateEffect(japanese);

        var effect = tree as ContCardEffect;
        Assert.IsNotNull(effect);
        Assert.IsTrue(effect.ConditionText.Contains("<<風>> characters"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_ClockToWaitingRoom_Token()
    {
        var japanese = "【自】 このカードが手札から舞台に置かれた時、あなたは自分のクロックの上から1枚までを、控え室に置き、そのターン中、このカードのパワーを＋3000。";
        var tree = _service.TranslateEffect(japanese);

        var effect = tree as AutoCardEffect;
        Assert.IsNotNull(effect);
        Assert.IsTrue(effect.AbilityText.Contains("clock to your waiting room"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_AutoEffect_TokenLogMustNotBeEmpty()
    {
        var japanese = "【自】 このカードが手札から舞台に置かれた時、あなたは自分のクロックの上から1枚までを、控え室に置き、そのターン中、このカードのパワーを＋3000。";
        var tree = _service.TranslateEffect(japanese);

        var effect = tree as AutoCardEffect;
        Assert.IsNotNull(effect);
        Assert.IsNotEmpty(effect!.TokenLog);
    }


    [TestMethod]
    [TestCategory("CI")]
    public void Translate_ActEffect_TokenLogMustNotBeEmpty()
    {
        var japanese = "【起】［手札を1枚控え室に置き、このカードを控え室に置く］ あなたは自分の控え室の《NIKKE》のキャラを1枚選び、手札に戻す。";
        var tree = _service.TranslateEffect(japanese);

        var effect = tree as ActCardEffect;
        Assert.IsNotNull(effect);
        Assert.IsNotEmpty(effect!.TokenLog);
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_LookAtTopCards_Token()
    {
        var japanese = "【自】 このカードが手札から舞台に置かれた時、あなたは自分の山札を上からＸ枚まで見て、カードを1枚まで選び、手札に加え、残りのカードを控え室に置く。Ｘはあなたの《風》のキャラの枚数に等しい。";
        var tree = _service.TranslateEffect(japanese);

        var effect = tree as AutoCardEffect;
        var expectedENText = "[AUTO] When this card is placed on stage from your hand, look at up to X cards from the top of your deck, choose up to 1 card from among them, put it to your hand, and put the rest to your waiting room. X is equal to the number of your <<風>> characters.";
        Assert.IsNotNull(effect);
        Assert.AreEqual(expectedENText, effect.EffectText);
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_DealDamage_Token()
    {
        var japanese = "【自】【ターン1】 このカードが手札から舞台に置かれたターン中、このカードの与えたダメージがキャンセルされた時、あなたは自分の山札の上から1枚を、控え室に置き、相手にＸダメージを与える。";
        var tree = _service.TranslateEffect(japanese);

        var effect = tree as AutoCardEffect;
        Assert.IsNotNull(effect);
        Assert.AreEqual("[AUTO][1/TURN] During the turn that this card is placed on the stage in your hand, when damage dealt by this card is canceled, put the top card of your deck to your waiting room, and deal X damage to your opponent.", effect.EffectText);
        Assert.AreEqual("Put the top card of your deck to your waiting room, and deal X damage to your opponent.", effect.AbilityText);
        Assert.IsTrue(effect.AbilityText.Contains("deal X damage to your opponent"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_DealDamage_Token_With_Reminder()
    {
        var japanese = "【自】【ターン1】 このカードが手札から舞台に置かれたターン中、このカードの与えたダメージがキャンセルされた時、あなたは自分の山札の上から1枚を、控え室に置き、相手にＸダメージを与える。Ｘはそのカードのレベル＋1に等しい。（CXのレベルは0として扱う。ダメージキャンセルは発生する）";
        var tree = _service.TranslateEffect(japanese);

        var effect = tree as AutoCardEffect;

        const string expectedReminder = "CX are regarded as level 0. Damage may be canceled";
        const string expectedPostConditionText = "X is equal to that sent card's level +1.";
        const string expectedEffectText = $"[AUTO][1/TURN] During the turn that this card is placed on the stage in your hand, " +
            $"when damage dealt by this card is canceled, put the top card of your deck to your waiting room, " +
            $"and deal X damage to your opponent. {expectedPostConditionText} ({expectedReminder})";
        const string expectedAbilityText = "Put the top card of your deck to your waiting room, " +
            "and deal X damage to your opponent. X is equal to that sent card's level +1.";
        Assert.IsNotNull(effect);
        Assert.AreEqual(expectedEffectText, effect.EffectText);
        Assert.AreEqual(expectedAbilityText, effect.AbilityText);
        Assert.AreEqual(expectedReminder, effect.ReminderText);
        Assert.AreEqual(expectedPostConditionText, effect.PostConditionText);
        Assert.IsTrue(effect.AbilityText.Contains("deal X damage to your opponent"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_ContEffect_TurnAndTraitCharacterCountPowerBoost_4()
    {
        var japanese = "【永】 あなたのターン中、他のあなたの《風》のキャラが4枚以上なら、このカードのパワーを＋5000。";
        var tree = _service.TranslateEffect(japanese);

        var effect = tree as ContCardEffect;
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

        var effect = tree as ContCardEffect;

        Assert.IsNotEmpty(effect!.TokenLog);
    }


    [TestMethod]
    [TestCategory("CI")]
    public void Translate_ContEffect_NonExistentConditionMustCrash()
    {
        var japanese = "【永】 あなたのターン中、他のあなたadded_textの《不存在》のキャラが4枚以上なら、このカードのパワーを＋5000。";
        var ex = Assert.ThrowsExactly<TranslationNotImplementedException>(() => _service.TranslateEffect(japanese));
        Assert.IsNotNull(ex.Effect);
        Assert.IsInstanceOfType<ContCardEffect>(ex.Effect);
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_ContEffect_NonExistentAbilityMustCrash()
    {
        var japanese = "【永】 あなたのターン中、他のあなたadded_textの《不存在》のキャラが4枚以上なら、このカードのadded_textパワーを＋5000。";
        var ex = Assert.ThrowsExactly<TranslationNotImplementedException>(() => _service.TranslateEffect(japanese));
        Assert.IsNotNull(ex.Effect);
        Assert.IsInstanceOfType<ContCardEffect>(ex.Effect);
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_AutoEffect_NonExistentConditionMustCrash()
    {
        var japanese = "【自】 あなたがこのカードの『non_existent_label』を使った時、あなたは自分の山札の上から1枚を公開する。そのカードが《風》のキャラなら手札に加え、あなたは自分の手札を1枚選び、控え室に置く。（そうでないなら元に戻す）";
        var ex = Assert.ThrowsExactly<TranslationNotImplementedException>(() => _service.TranslateEffect(japanese));
        Assert.IsNotNull(ex.Effect);
        Assert.IsInstanceOfType<AutoCardEffect>(ex.Effect);
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_AutoEffect_NonExistentAbilityMustCrash()
    {
        var japanese = "【自】 あなたがこのカードの『助太刀』を使った時、あなたは自分の山札の上から1枚枚枚枚枚を公開する。そのカードが《風》のキャラなら手札に加え、あなたは自分の手札を1枚選び、控え室に置く。（そうでないなら元に戻す）";
        var ex = Assert.ThrowsExactly<TranslationNotImplementedException>(() => _service.TranslateEffect(japanese));
        Assert.IsNotNull(ex.Effect);
        Assert.IsInstanceOfType<AutoCardEffect>(ex.Effect);
    }

    /// <summary>
    /// SMP/W137-075: 【自】 that grants an inner 【永】 ability.
    /// Verifies that TranslateNested catches the inner ContEffectToken exception
    /// and returns a fallback EventCardEffect, so the outer AutoEffectToken
    /// completes and its IsUnmatched scan fires with the correct AutoCardEffect
    /// partial tree preserving the full outer context.
    /// </summary>
    [TestMethod]
    [TestCategory("Manual")]
    public void Translate_AutoEffect_GrantContAbility_Misclassified()
    {
        var japanese = "【自】 このカードが手札から舞台に置かれた時、あなたは自分のクロック置場のキャラを1枚まで選び、控え室に置き、他のあなたの《サマポケ》のキャラが4枚以上なら、次の相手のターンの終わりまで、このカードは次の能力を得る。『【永】 このカードの正面のキャラのソウルを－1。』";
        var ex = Assert.ThrowsExactly<TranslationNotImplementedException>(() => _service.TranslateEffect(japanese));

        Assert.IsNotNull(ex.Effect);
        Assert.IsInstanceOfType<AutoCardEffect>(ex.Effect,
            "Outer tree should be AutoCardEffect; the nested Cont failure is recorded as an unmatched ability inside it.");

        var auto = (AutoCardEffect)ex.Effect;
        Assert.IsTrue(auto.ConditionText.Contains("When this card is placed on stage from your hand"),
            "Condition should preserve the auto trigger.");
        Assert.IsTrue(auto.Abilities.Any(a => a.IsUnmatched),
            "There should be at least one unmatched ability (the untranslated nested Cont effect).");
        Assert.IsTrue(auto.EffectText.StartsWith("[AUTO]"),
            "EffectText should start with [AUTO], not [CONT].");
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_ContEffect_TurnAndTraitCharacterCountPowerBoostAndGainSkill()
    {
        var japanese = "【永】 あなたのターン中、他のあなたの《風》のキャラが4枚以上なら、このカードのパワーを＋5000し、このカードは次の能力を得る。『【永】 このカードのバトル中、相手はイベントと『助太刀』を手札からプレイできない。』";
        var tree = _service.TranslateEffect(japanese);

        ContCardEffect? effect = null;

        MultiAssert.AllAreTrue([
            () => {
                effect = tree as ContCardEffect;
                Assert.IsNotNull(effect);
            },
            () => Assert.AreEqual("During your turn, if you have 4 or more other <<風>> characters", effect!.ConditionText),
            () => Assert.IsTrue(effect!.AbilityText.Contains("+5000 power")),
            () => Assert.IsTrue(effect!.EffectText.Contains("[CONT]")),
            () => Assert.IsTrue(effect!.EffectText.Contains("+5000 power")),
            () => Assert.AreEqual("[CONT] During your turn, if you have 4 or more other <<風>> characters, this card gets +5000 power, and the following ability. \"[CONT] During this card's battle, your opponent cannot play events or \"Backup\" from their hand.\"", effect!.EffectText),
        ], Assert.Fail);
    }

    [TestMethod]
    [TestCategory("CI")]
    public void Translate_EventEffect_HasCosts()
    {
        var japanese = "［手札のCXを1枚控え室に置く］ あなたはコストを払ってよい。そうしたら、あなたはバトル中のキャラを1枚選び、そのターン中、次の能力を与える。『【永】 このカードはプレイヤーにダメージを与えることができない。』";
        var tree = _service.TranslateEffect(japanese);
        var effect = tree as EventCardEffect;
        MultiAssert.AllAreTrue([
            () => Assert.IsNotNull(effect),
            () => Assert.IsTrue(effect!.CostText == "Put a CX from your hand to your waiting room"),
            () => Assert.AreEqual("[Put a CX from your hand to your waiting room] You may pay the cost. If you do, choose 1 character in battle, and that character gets the following ability until end of turn. \"[CONT] This card cannot deal damage to players.\"", effect!.EffectText),
        ], Assert.Fail);
    }

    [TestMethod]
    [TestCategory("CI")]
    [DynamicData(nameof(TranslateCsvCrossCheckAllData))]
    public void Translate_CSV_CrossCheckAll(string serial, string jpEffect, string enEffect, string labels)
    {
        var tree = _service.TranslateEffect(jpEffect);

        var expected = enEffect.Trim();
        var actual = tree.EffectText.Trim();
        var expectedLabels = string.IsNullOrEmpty(labels)
            ? Array.Empty<string>()
            : labels.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (jpEffect.StartsWith("【自】") && tree is not AutoCardEffect)
        {
            Log.Warning("[{serial}] resolves to {effectType} and needs to unify its function to AutoCardEffect.", serial, tree.GetType().Name);
        }
        else if (jpEffect.StartsWith("【永】") && tree is not ContCardEffect)
        {
            Log.Warning("[{serial}] resolves to {effectType} and needs to unify its function to ContCardEffect.", serial, tree.GetType().Name);
        }
        else if (jpEffect.StartsWith("【起】") && tree is not ActCardEffect)
        {
            Log.Warning("[{serial}] resolves to {effectType} and needs to unify its function to ActCardEffect.", serial, tree.GetType().Name);
        }

        Log.Debug("Full Effect: {@effect}", tree);

        MultiAssert.AllAreTrue([
            () => Assert.AreEqual(expected, actual, $"[{serial}] EffectText mismatch{Environment.NewLine}Expected: {expected}{Environment.NewLine}Actual: {actual}"),
            () => CollectionAssert.AreEqual(expectedLabels, tree.Labels, $"[{serial}]{Environment.NewLine}Expected: {string.Join(", ", expectedLabels)}{Environment.NewLine}Actual: {string.Join(", ", tree.Labels)}{Environment.NewLine}Labels mismatched")
        ], Assert.Fail);
    }
}