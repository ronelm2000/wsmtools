using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.Entities.Effect;
using Montage.Weiss.Tools.Entities.Effect.Token;
using Montage.Weiss.Tools.Impls.Services;
using Serilog;

namespace Montage.Weiss.Tools.Test.Translations;

[TestClass]
public partial class RegistryTests
{
    private static readonly ILogger Log = Serilog.Log.ForContext<RegistryTests>();
    private static WeissSchwarzCardTranslatorService? _service;
    private static WeissSchwarzCardTranslatorService Service => _service ??= Global.Container.GetInstance<WeissSchwarzCardTranslatorService>();

    public TestContext TestContext { get; set; }

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
        if (!regex.EndsWith("』"))
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
}