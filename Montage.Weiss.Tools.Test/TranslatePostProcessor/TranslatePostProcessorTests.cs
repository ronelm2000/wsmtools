using Fluent.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Entities.Effect;
using Montage.Weiss.Tools.Exceptions;
using Montage.Weiss.Tools.Impls.PostProcessors;
using Montage.Weiss.Tools.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.TranslatePostProcessors;

[TestClass]
public class TranslatePostProcessorTests
{
    public TestContext TestContext { get; set; }

    public IParseInfo CreateParseInfo(params string[] hints)
    {
        return new MockParseInfo { ParserHints = hints };
    }

    private static readonly string KnownGoodJpEffect = "【永】 あなたの手札が5枚以上なら、このカードのパワーを＋2000。";
    private static readonly string KnownFailingJpEffect = "【永】 あなたのターン中、他のあなたadded_textの《不存在》のキャラが4枚以上なら、このカードのadded_textパワーを＋5000。";

    private WeissSchwarzCard CreateJpCard(params string[] effects)
    {
        return new WeissSchwarzCard
        {
            Serial = "NIK/S117-001",
            Name = new MultiLanguageString { JP = "テスト", EN = "" },
            Effect = effects,
            Rarity = "C",
            Triggers = []
        };
    }

    private WeissSchwarzCard CreateEnCard()
    {
        var card = new WeissSchwarzCard
        {
            Serial = "BD/EN-W03-001",
            Name = new MultiLanguageString { JP = "", EN = "Test" },
            Effect = ["Some English effect text."],
            Rarity = "C",
            Triggers = []
        };
        return card;
    }

    [TestMethod]
    [TestCategory("CI")]
    public void IsIncluded_WithTranslationFlag_ReturnsTrue()
    {
        var pp = Global.Container.GetInstance<TranslatePostProcessor>();
        var result = pp.IsIncluded(CreateParseInfo("translation")).Result;
        Assert.IsTrue(result);
    }

    [TestMethod]
    [TestCategory("CI")]
    public void IsIncluded_WithoutTranslationFlag_ReturnsFalse()
    {
        var pp = Global.Container.GetInstance<TranslatePostProcessor>();
        var result = pp.IsIncluded(CreateParseInfo()).Result;
        Assert.IsFalse(result);
    }

    [TestMethod]
    [TestCategory("CI")]
    public void IsIncluded_SkipTranslateFlag_ReturnsFalse()
    {
        var pp = Global.Container.GetInstance<TranslatePostProcessor>();
        var result = pp.IsIncluded(CreateParseInfo("translation", "skip:translate")).Result;
        Assert.IsFalse(result);
    }

    [TestMethod]
    [TestCategory("CI")]
    public void IsIncluded_SkipExternalFlag_ReturnsFalse()
    {
        var pp = Global.Container.GetInstance<TranslatePostProcessor>();
        var result = pp.IsIncluded(CreateParseInfo("translation", "skip:external")).Result;
        Assert.IsFalse(result);
    }

    [TestMethod]
    [TestCategory("CI")]
    public void IsCompatible_WithJapaneseCards_ReturnsTrue()
    {
        var pp = Global.Container.GetInstance<TranslatePostProcessor>();
        var cards = new List<WeissSchwarzCard> { CreateJpCard(KnownGoodJpEffect), CreateEnCard() };
        var result = pp.IsCompatible(cards).Result;
        Assert.IsTrue(result);
    }

    [TestMethod]
    [TestCategory("CI")]
    public void IsCompatible_AllEnglishCards_ReturnsFalse()
    {
        var pp = Global.Container.GetInstance<TranslatePostProcessor>();
        var cards = new List<WeissSchwarzCard> { CreateEnCard(), CreateEnCard() };
        var result = pp.IsCompatible(cards).Result;
        Assert.IsFalse(result);
    }

    [TestMethod]
    [TestCategory("CI")]
    public async Task Process_TranslatesJpCard_ReplacesEffect()
    {
        var pp = Global.Container.GetInstance<TranslatePostProcessor>();
        var card = CreateJpCard(KnownGoodJpEffect);
        var source = new[] { card }.ToAsyncEnumerable();

        var results = await pp.Process(source, Global.MockProgress, TestContext.CancellationToken).ToListAsync();

        Assert.AreEqual(1, results.Count);
        var resultCard = results[0];
        Assert.AreNotEqual(KnownGoodJpEffect, resultCard.Effect[0]);
        Assert.IsTrue(resultCard.Effect[0].Contains("[CONT]"));
        Assert.IsTrue(resultCard.Effect[0].Contains("+2000 power"));
    }

    [TestMethod]
    [TestCategory("CI")]
    public async Task Process_StoresCardEffectTreeInAdditionalInfo()
    {
        var pp = Global.Container.GetInstance<TranslatePostProcessor>();
        var card = CreateJpCard(KnownGoodJpEffect);
        var source = new[] { card }.ToAsyncEnumerable();

        var results = await pp.Process(source, Global.MockProgress, TestContext.CancellationToken).ToListAsync();

        var tree = results[0].FindOptionalInfo<CardEffectTree>("translation.tree");
        Assert.IsNotNull(tree);
        Assert.AreEqual(1, tree.Effects.Count);
        Assert.IsInstanceOfType<ContCardEffect>(tree.Effects[0]);
    }

    [TestMethod]
    [TestCategory("CI")]
    public async Task Process_SkipsEnglishCard_Unchanged()
    {
        var pp = Global.Container.GetInstance<TranslatePostProcessor>();
        var card = CreateEnCard();
        var source = new[] { card }.ToAsyncEnumerable();

        var results = await pp.Process(source, Global.MockProgress, TestContext.CancellationToken).ToListAsync();

        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("Some English effect text.", results[0].Effect[0]);
    }

    [TestMethod]
    [TestCategory("CI")]
    public async Task Process_FailureWritesReportAndThrowsAggregate()
    {
        var pp = Global.Container.GetInstance<TranslatePostProcessor>();
        var card = CreateJpCard(KnownFailingJpEffect);
        var source = new[] { card }.ToAsyncEnumerable();

        var reportPath = Path.Get("./Export/", "failed_translation_report.json");
        if (reportPath.Exists)
            reportPath.Delete();

        var ex = await Assert.ThrowsExactlyAsync<TranslationFailedException>(async () =>
        {
            await pp.Process(source, Global.MockProgress, TestContext.CancellationToken).ToListAsync();
        });

        Assert.IsInstanceOfType<AggregateException>(ex.InnerException);
        var aggregate = (AggregateException)ex.InnerException!;
        Assert.IsTrue(aggregate.InnerExceptions.Count >= 1);
        Assert.IsInstanceOfType<TranslationNotImplementedException>(aggregate.InnerExceptions[0]);

        Assert.IsTrue(reportPath.Exists);
        var reportJson = await reportPath.ReadStringAsync(TestContext.CancellationToken);
        Assert.IsTrue(reportJson.Contains("NIK/S117-001"), "Report should contain the card serial");
        Assert.IsTrue(reportJson.Contains("added_text"), "Report should contain the original JP text");
        Assert.IsTrue(reportJson.Contains("$type"), "Report should contain polymorphic type discriminator");
        Assert.IsTrue(reportJson.Contains("Suggestions"), "Report should contain Suggestions from UnmatchedAbility");
        // Verify full *Text properties are present with content
        Assert.IsTrue(reportJson.Contains("\"EffectText\""), "Report should contain EffectText");
        Assert.IsTrue(reportJson.Contains("\"AbilityText\""), "Report should contain AbilityText");
        Assert.IsTrue(reportJson.Contains("\"ConditionText\""), "Report should contain ConditionText");
        Assert.IsTrue(reportJson.Contains("[CONT]"), "Report should contain translated effect text");
        Assert.IsTrue(reportJson.Contains("5000"), "Report should contain ability text content");

        // Clean up test file
        reportPath.Delete();
    }
}

internal class MockParseInfo : IParseInfo
{
    public string URI => "test://uri";
    public IEnumerable<string> ParserHints { get; set; } = new string[] { };
}
