using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Card.API.Helpers;
using Montage.Weiss.Tools.Entities.Effect;
using Montage.Weiss.Tools.Impls.Services;

namespace Montage.Weiss.Tools.Test;

[TestClass]
public class TranslatorServiceTests
{
    private readonly WeissSchwarzCardTranslatorService _service = new();

    [TestMethod]
    public void Translate_ContEffect_HandSizePowerBoost()
    {
        var japanese = "【永】 あなたの手札が5枚以上なら、このカードのパワーを＋2000。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as ContCardEffect;
        Assert.IsNotNull(effect);
        Assert.AreEqual("If you have 5 or more cards in your hand", effect.ConditionText);
        Assert.AreEqual("this card gets +2000 power", effect.AbilityText);
        Assert.IsTrue(effect.EffectText.Contains("[CONT]"));
        Assert.IsTrue(effect.EffectText.Contains("+2000 power"));
    }

    [TestMethod]
    public void MatchLabels_WithLabel_ReturnsLabels()
    {
        var result = _service.MatchLabels("【R】");
        Assert.AreEqual(1, result.Length);
        Assert.AreEqual("R", result[0]);
    }

    [TestMethod]
    public void MatchLabels_Empty_ReturnsEmpty()
    {
        var result = _service.MatchLabels("");
        Assert.AreEqual(0, result.Length);
    }

    [TestMethod]
    public void Translate_ContEffect_TurnAndTraitCharacterCountPowerBoost()
    {
        var japanese = "【永】 あなたのターン中、他のあなたの《風》のキャラが2枚以上なら、このカードのパワーを＋4000。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as ContCardEffect;
        Assert.IsNotNull(effect);
        Assert.AreEqual("During your turn, if you have 2 or more other <<風>> characters", effect.ConditionText);
        Assert.AreEqual("this card gets +4000 power", effect.AbilityText);
        Assert.IsTrue(effect.EffectText.Contains("[CONT]"));
        Assert.IsTrue(effect.EffectText.Contains("+4000 power"));
    }

    [TestMethod]
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
    public void Translate_WithReminderText_MultipleSentences()
    {
        var japanese = "【自】［手札を1枚控え室に置く］ あなたのCXがCX置場に置かれた時、あなたはコストを払ってよい。そうしたら、あなたは自分の山札の上から1枚を公開し、自分の控え室のレベルＸ以下のキャラを1枚選び、手札に戻す。Ｘは公開されたカードのレベルに等しい。（CXのレベルは0として扱う。公開したカードは元に戻す）";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0];
        Assert.IsFalse(string.IsNullOrEmpty(effect.ReminderText));
        Assert.IsTrue(effect.ReminderText.Contains("CX are regarded as level 0"));
        Assert.IsTrue(effect.ReminderText.Contains("Put it on its original position"));
        Assert.IsTrue(effect.EffectText.Contains("CX are regarded as level 0"));
        Assert.IsTrue(effect.EffectText.Contains("Put it on its original position"));
    }

    [TestMethod]
    public void Translate_WithoutReminderText_ReminderTextEmpty()
    {
        var japanese = "【永】 あなたの手札が5枚以上なら、このカードのパワーを＋2000。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0];
        Assert.AreEqual(string.Empty, effect.ReminderText);
    }

    [TestMethod]
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
    public void Translate_Brainstorm_Token()
    {
        var japanese = "集中 あなたは自分の山札の上から3枚をめくり、控え室に置く。あなたは自分の控え室のレベルＸ以下の《風》のキャラを1枚選び、手札に戻す。Ｘはそれらのカードの《風》のキャラの枚数に等しい。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0];
        Assert.IsTrue(effect.EffectText.Contains("Brainstorm"));
    }

    [TestMethod]
    public void Translate_ReverseCondition_Token()
    {
        var japanese = "【自】 このカードが【リバース】した時、このカードのバトル相手のレベルが0以下なら、あなたはそのキャラを山札の下に置いてよい。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as AutoCardEffect;
        Assert.IsNotNull(effect);
        Assert.IsTrue(effect.ConditionText.Contains("[REVERSED]"));
    }

    [TestMethod]
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
    public void Translate_DamageCancelledCondition_Token()
    {
        var japanese = "【自】【ターン1】 このカードが手札から舞台に置かれたターン中、このカードの与えたダメージがキャンセルされた時、あなたは自分の山札の上から1枚を、控え室に置き、相手にＸダメージを与える。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as AutoCardEffect;
        Assert.IsNotNull(effect);
        Assert.IsTrue(effect.ConditionText.Contains("damage is cancelled"));
    }

    [TestMethod]
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
    public void Translate_ClockToWaitingRoom_Token()
    {
        var japanese = "【自】 このカードが手札から舞台に置かれた時、あなたは自分のクロックの上から1枚までを、控え室に置き、そのターン中、このカードのパワーを＋3000。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as AutoCardEffect;
        Assert.IsNotNull(effect);
        Assert.IsTrue(effect.AbilityText.Contains("clock into your waiting room"));
    }

    [TestMethod]
    public void Translate_LookAtTopCards_Token()
    {
        var japanese = "【自】 このカードが手札から舞台に置かれた時、あなたは自分の山札を上からＸ枚まで見て、カードを1枚まで選び、手札に加え、残りのカードを控え室に置く。Ｘはあなたの《風》のキャラの枚数に等しい。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as AutoCardEffect;
        Assert.IsNotNull(effect);
        Assert.AreEqual("[AUTO] When this card is placed on stage from your hand, look at up to X cards from the top of your deck, choose up to 1 card, put it into your hand, and put the rest into your waiting room. X is equal to the number of your  <<風>> characters.", effect.EffectText);
        Assert.AreEqual("Look at up to X cards from the top of your deck, choose up to 1 card, put it into your hand, and put the rest into your waiting room. X is equal to the number of your  <<風>> characters", effect.AbilityText);
        Assert.IsTrue(effect.AbilityText.Contains("Look at up to X cards"));
    }

    [TestMethod]
    public void Translate_DealDamage_Token()
    {
        var japanese = "【自】【ターン1】 このカードが手札から舞台に置かれたターン中、このカードの与えたダメージがキャンセルされた時、あなたは自分の山札の上から1枚を、控え室に置き、相手にＸダメージを与える。";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as AutoCardEffect;
        Assert.IsNotNull(effect);
        Assert.AreEqual("[AUTO][1/TURN] During the turn this card was placed on stage from the hand, when this card's damage is cancelled, put the top card of your deck into your waiting room, and deal X damage to your opponent. X is equal to that sent card's level +1.", effect.EffectText);
        Assert.AreEqual("Put the top card of your deck into your waiting room, and deal X damage to your opponent. X is equal to that sent card's level +1.", effect.AbilityText);
        Assert.IsTrue(effect.AbilityText.Contains("deal X damage to your opponent"));
    }

    [TestMethod]
    public void Translate_DealDamage_Token_With_Reminder()
    {
        var japanese = "【自】【ターン1】 このカードが手札から舞台に置かれたターン中、このカードの与えたダメージがキャンセルされた時、あなたは自分の山札の上から1枚を、控え室に置き、相手にＸダメージを与える。Ｘはそのカードのレベル＋1に等しい。（CXのレベルは0として扱う。ダメージキャンセルは発生する）";
        var tree = _service.TranslateEffect(japanese);

        Assert.AreEqual(1, tree.Effects.Count);
        var effect = tree.Effects[0] as AutoCardEffect;

        const string expectedReminder = "CX are regarded as level 0. Damage may be cancelled";
        const string expectedEffectText = $"[AUTO][1/TURN] During the turn this card was placed on stage from the hand, " +
            $"when this card's damage is cancelled, put the top card of your deck into your waiting room, " +
            $"and deal X damage to your opponent. X is equal to that sent card's level +1. ({expectedReminder})";
        const string expectedAbilityText = "Put the top card of your deck into your waiting room, " +
            "and deal X damage to your opponent. X is equal to that sent card's level +1.";
        Assert.IsNotNull(effect);
        Assert.AreEqual(expectedEffectText, effect.EffectText);
        Assert.AreEqual(expectedAbilityText, effect.AbilityText);
        Assert.AreEqual(expectedReminder, effect.ReminderText);
        Assert.IsTrue(effect.AbilityText.Contains("deal X damage to your opponent"));
    }

    [TestMethod]
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
        ]);
    }
}
