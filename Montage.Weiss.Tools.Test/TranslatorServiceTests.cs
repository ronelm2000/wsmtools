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
}
