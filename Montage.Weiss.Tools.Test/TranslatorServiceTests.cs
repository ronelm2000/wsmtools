using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        Assert.AreEqual("During your turn, if you control 2 or more other <<風>> characters", effect.ConditionText);
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
}
