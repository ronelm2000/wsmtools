using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Weiss.Tools.Entities.Effect;
using Montage.Weiss.Tools.Entities.Effect.Token;
using Montage.Weiss.Tools.Impls.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Montage.Weiss.Tools.Test.Translations;

public static class TranslationTestDataHelpers
{
    /// <summary>
    /// List of excluded ability type names for tests requiring atomic conditions.
    /// These ability tokens are classified to perform greedy captures that are known to not capture ending punctuations.
    /// </summary>
    public static HashSet<string> ExcludedAbilityTypeNames =>
        new()
        {
            "PowerBoostWithFollowingAbilityToken",
            "GiveMultipleAbilitiesToken",
            "EncoreToken",
            "BackupPrefixToken",
            "IfYouDoToken",
            "ChooseOtherCharacterAndGiveAbilityToken",
            "PowerBoostWithFollowingAbilitiesToken",
        };

    /// <summary>
    /// Returns all token types with their regex patterns for registry validation.
    /// </summary>
    public static IEnumerable<(Type type, string regex)> GetTokenRegexValues(WeissSchwarzCardTranslatorService service)
    {
        var conditionList = service.ConditionListRegistry.GetAllTokens()
                      .Select(t => (t.GetType(), t.Matcher.ToString()));

        var effectList = service.EffectListRegistry.GetAllTokens()
            .Select(t => (t.GetType(), t.Matcher.ToString()));

        var effects = service.EffectRegistry.GetAllTokens()
            .Select(t => (t.GetType(), t.Matcher.ToString()));

        var reminders = service.ReminderTextRegistry.GetAllTokens()
            .Select(t => (t.GetType(), t.Matcher.ToString()));

        var all = conditionList.Concat(effectList).Concat(effects).Concat(reminders);
        return all;
    }

    /// <summary>
    /// Returns ability token regex values excluding types known to not capture ending punctuation.
    /// </summary>
    public static IEnumerable<(Type type, string regex)> GetAbilityTokenRegexValues(WeissSchwarzCardTranslatorService service)
    {
        var effectList = service.EffectListRegistry.GetAllTokens()
            .Select(t => (t.GetType(), t.Matcher.ToString()))
            .Where(t => !ExcludedAbilityTypeNames.Contains(t.Item1.Name));

        return effectList;
    }

    /// <summary>
    /// Returns all ability token regex values including excluded types.
    /// </summary>
    public static IEnumerable<(Type type, string regex)> GetAbilityTokenRegexValuesV2(WeissSchwarzCardTranslatorService service)
    {
        var effectList = service.EffectListRegistry.GetAllTokens()
            .Select(t => (t.GetType(), t.Matcher.ToString()));

        return effectList;
    }

    /// <summary>
    /// Returns condition tokens with their names for atomic condition testing.
    /// </summary>
    public static IEnumerable<(string tokenName, CardTextToken<List<CardEffectCondition>> condition)> GetConditionTokenRegexValues(WeissSchwarzCardTranslatorService service)
        => service.ConditionListRegistry.GetAllTokens()
            .Select(t => (t.GetType().Name, t));

    /// <summary>
    /// Returns all CSV translation cross-check data from the translations directory.
    /// Each row is a test case comparing Japanese input to expected English output.
    /// </summary>
    public static IEnumerable<TestDataRow<(string serial, string jpEffect, string enEffect, string labels)>> TranslateCsvCrossCheckAllData()
    {
        var translationsDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Resources", "Translations"));
        var csvFiles = Directory.GetFiles(translationsDirectory, "*.csv").ToList();

        Assert.IsTrue(csvFiles.Count > 0, $"No cross-check CSV files found in: {translationsDirectory}");

        foreach (var csvPath in csvFiles)
        {
            var csvFile = Path.GetFileName(csvPath);

            using var reader = new StreamReader(csvPath);
            var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                BadDataFound = null
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
                    TestCategories = new[] { "CI", row.Serial },
                    DisplayName = $"CSV-Cross-Check#{row.Serial}#{effectID}"
                };
            }
        }
    }

    /// <summary>
    /// CSV row record for translation cross-check data.
    /// </summary>
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
}

public partial class RegistryTests
{
    private static IEnumerable<(Type tokenType, string regex)> GetTokenRegexValues()
        => TranslationTestDataHelpers.GetTokenRegexValues(Service);

    private static IEnumerable<(Type tokenType, string regex)> GetAbilityTokenRegexValues()
        => TranslationTestDataHelpers.GetAbilityTokenRegexValues(Service);

    private static IEnumerable<(Type tokenType, string regex)> GetAbilityTokenRegexValuesV2()
        => TranslationTestDataHelpers.GetAbilityTokenRegexValuesV2(Service);

    private static IEnumerable<(string tokenName, CardTextToken<List<CardEffectCondition>> condition)> GetConditionTokenRegexValues()
        => TranslationTestDataHelpers.GetConditionTokenRegexValues(Service);
}

public partial class TranslationTests
{
    private static IEnumerable<TestDataRow<(string serial, string jpEffect, string enEffect, string labels)>> TranslateCsvCrossCheckAllData()
            => TranslationTestDataHelpers.TranslateCsvCrossCheckAllData();
}