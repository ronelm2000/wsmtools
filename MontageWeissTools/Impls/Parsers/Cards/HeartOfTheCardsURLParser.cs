﻿using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using Fluent.IO;
using Montage.Card.API.Interfaces.Services;
using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Utilities;
using System.Threading;
using System.Runtime.CompilerServices;
using Montage.Card.API.Exceptions;

namespace Montage.Weiss.Tools.Impls.Parsers.Cards;

public class HeartOfTheCardsURLParser : ICardSetParser<WeissSchwarzCard>
{
    private ILogger Log = Serilog.Log.ForContext<HeartOfTheCardsURLParser>();

    public async Task<bool> IsCompatible(IParseInfo parseInfo)
    {
        return await ValueTask.FromResult(IsCompatibleAsHOTCTextFile(parseInfo) || IsCompatibleAsURL(parseInfo));
    }

    private bool IsCompatibleAsURL(IParseInfo info)
    {
        var urlOrFile = info.URI;
        if (!Uri.TryCreate(urlOrFile, UriKind.Absolute, out Uri? url))
        {
            Log.Debug("Not compatible because not a url: {urlOrFile}", urlOrFile);
            return false;
        }
        if (!url.IsWellFormedOriginalString())
        {
            Log.Debug("Not a proper URL, will ignore: {urlOrFile}", urlOrFile);
            return false;
        }
        if (url.Authority != "heartofthecards.com" && url.Authority != "www.heartofthecards.com")
        {
            Log.Debug("Not compatible because {Authority} is not heartofthecards.com", url.Authority);
            return false;
        }

        if (!url.AbsolutePath.StartsWith("/translations/"))
        {
            Log.Debug("Not compatible because {AbsolutePath} does not start with /translations.", url.AbsolutePath);
            return false;
        }
        if (url.AbsolutePath == "/translations/")
        {
            Log.Debug("Not compatible because absolute path cannot be /translations/ itself; please provide a set html.");
            return false;
        }
        Log.Information("Compatible as a URL.");
        return true;
    }

    private bool IsCompatibleAsHOTCTextFile(IParseInfo info)
    {
        if (!info.ParserHints.Select(s => s.ToLower()).Contains("hotc"))
            return false;
        var possiblyPath = Path.Get(info.URI);
        var isCompatible = possiblyPath.Exists && possiblyPath.Extension == ".txt";
        if (isCompatible)
            Log.Information("Compatible as a Local .txt file.");
        return isCompatible;
    }

    public async IAsyncEnumerable<WeissSchwarzCard> Parse(string url, IProgress<SetParserProgressReport> progress, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Log.Information("Starting. URI: {url}", url);
        string? textToProcess = null;

        var progressReport = new SetParserProgressReport
        {
            ReportMessage = new MultiLanguageString { EN = $"Starting with URI: [{url}]" },
            Percentage = 0
        };
        progress.Report(progressReport);

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.IsWellFormedOriginalString())
        {
            var html = await new Uri(url).DownloadHTML(cancellationToken);
            var preSelector = "td > pre";
            textToProcess = html.QuerySelector(preSelector)?.TextContent;
        }
        else
        {
            var path = Path.Get(url);
            textToProcess = await path.ReadStringAsync(cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();
        progressReport = progressReport with
        {
            ReportMessage = new MultiLanguageString { EN = $"Finished obtaining text: [{url}]" },
            Percentage = 10
        };
        progress.Report(progressReport);

        var majorSeparator = "================================================================================";
        var textSplits = textToProcess?.Split(majorSeparator) ?? Array.Empty<string>();
        var rows = textSplits.Length - 2;
        var results = textSplits.AsEnumerable()
            .Skip(1)
            .SkipLast(1)
            .Select( (section, index) => (index, card: ParseHOTCText(section)))
            ;

        foreach (var cardPair in results)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progressReport = progressReport.WithParsedSerial(cardPair.card, rows);
            progress.Report(progressReport);
            yield return cardPair.card;
        }

        progressReport = progressReport with
        {
            ReportMessage = new MultiLanguageString { EN = $"Parsed all cards." },
            Percentage = 100
        };
        progress.Report(progressReport);
        yield break;
    }

    private WeissSchwarzCard ParseHOTCText(string hotcText)
    {
        var cursor = hotcText.AsSpanCursor();
        var res = new WeissSchwarzCard();

        var cardNoText = "Card No.: ";
        var rarityText = "Rarity:";
        var colorText = "Color: ";
        var sideText = "Side: ";
        var levelText = "Level: ";
        var costText = "Cost: ";
        var powerText = "Power: ";
        var soulText = "Soul: ";
        var traitsText = "Traits: ";
        var trait1Text = "Trait 1: ";
        var triggersText = "Triggers: ";
        var flavorText = "Flavor:";
        var rulesTextText = "TEXT:";

        while (!cursor.CurrentLine.StartsWith(cardNoText))
            cursor.Next();
        var linesToCardNoText = cursor.LineNumber;
        //            ReadOnlySpan<char> cardNoLine = cursor.CurrentLine;
        res.Serial = cursor.CurrentLine.Slice(
                        c => c.IndexOf(cardNoText) + cardNoText.Length,
                        c => c.IndexOf(rarityText)
                        )
                        .Trim()
                        .ToString();
        try
        {
            res.Rarity = cursor.CurrentLine.Slice(c => c.IndexOf(rarityText) + rarityText.Length).Trim().ToString();
        }
        catch (Exception e)
        {
            res.Rarity = HandleRarityCorrections(res.Serial, e);
        }
        if (string.IsNullOrWhiteSpace(res.Rarity))
            res.Rarity = HandleRarityCorrections(res.Serial, null);

        cursor.MoveUp();
        // Log.Information("+1 above Card No: {line}", cursor.CurrentLine.ToString());
        res.Name = new MultiLanguageString();
        res.Name["jp"] = cursor.CurrentLine.ToString();
        if (cursor.LineNumber > 1)
        {
            cursor.MoveUp();
            res.Name["en"] = cursor.CurrentLine.ToString();
        }
        //cursor.Next(3);
        cursor.Next(linesToCardNoText - cursor.LineNumber + 1);
        try
        {
            res.Color = cursor.CurrentLine.Slice(
                            c => c.IndexOf(colorText) + colorText.Length,
                            c => c.IndexOf(sideText)
                            )
                            .Trim()
                            .ToEnum<CardColor>() ?? throw new SetParsingException(new CannotBeParsedCode("CardColor"));
        }
        catch (Exception e)
        {
            res.Color = HandleColorCorrections(res.Serial, e);
        }

        try
        {
            res.Side = cursor.CurrentLine.Slice(
                            c => c.IndexOf(sideText) + sideText.Length,
                            c => c.IndexOf(sideText) + sideText.Length + c.Slice(c.IndexOf(sideText) + sideText.Length).IndexOf(' ')
                            )
                            .Trim()
                            .ToEnum<CardSide>() ?? throw new SetParsingException(new CannotBeParsedCode("CardSide"));

            var sideString = res.Side.ToString();
            res.Type = cursor.CurrentLine.Slice(
                            c => c.IndexOf(sideString, StringComparison.CurrentCultureIgnoreCase) + sideString.Length
                            )
                            .Trim()
                            .ToEnum<CardType>() ?? throw new SetParsingException(new CannotBeParsedCode("CardType"));
        } catch (Exception)
        {
            (res.Side, res.Type) = HandleCorrections(res.Serial);
        }

        cursor.Next();

        switch (res.Type)
        {
            case CardType.Character:
                res.Power = cursor.CurrentLine.Slice(
                        c => c.IndexOf(powerText) + powerText.Length,
                        c => c.IndexOf(soulText)
                    )
                    .Trim()
                    .AsParsed<int>(int.TryParse);

                res.Soul = cursor.CurrentLine.Slice(
                        c => c.IndexOf(soulText) + soulText.Length
                    )
                    .Trim()
                    .AsParsed<int>(int.TryParse);
                goto case CardType.Event;
            case CardType.Event:
                res.Level = cursor.CurrentLine.Slice(
                        c => c.IndexOf(levelText) + levelText.Length,
                        c => c.IndexOf(costText)
                    )
                    .Trim()
                    .AsParsed<int>(int.TryParse);

                res.Cost = cursor.CurrentLine.Slice(
                        c => c.IndexOf(costText) + costText.Length,
                        c => c.IndexOf(powerText)
                    )
                    .Trim()
                    .AsParsed<int>(int.TryParse);
                break;
            default:
                break;
        }

        cursor.Next();
        if (cursor.CurrentLine.ToString().Contains(traitsText))
        {
            res.Traits = cursor.CurrentLine
                .Slice(c => c.IndexOf(traitsText) + traitsText.Length)
                .Trim()
                .ToString()
                .SplitWithRegex(traitMatcher)
                .Select(this.ParseTrait)
                .Where(o => o is not null)
                .Select(o => o!)
                .ToList();
        }
        else if (cursor.CurrentLine.ToString().Contains(trait1Text))
        {
            res.Traits = cursor.CurrentLine
                .Slice(c => c.IndexOf(trait1Text) + trait1Text.Length)
                .Trim()
                .ToString()
                .Split("Trait 2: ")
                .SelectMany(s => s.Trim().SplitWithRegex(traitMatcher))
                .Select(this.ParseTrait)
                .Where(o => o is not null)
                .Select(o => o!)
                .ToList();
        }

        cursor.Next();

        var stringTriggers = cursor.CurrentLine
            .Slice(c => c.IndexOf(triggersText) + triggersText.Length)
            .ToString();
        res.Triggers = TranslateTriggers(stringTriggers.Trim());

        cursor.Next();

        res.Flavor = cursor.CurrentLine.Slice(c => c.IndexOf(flavorText) + flavorText.Length).ToString();
        while (cursor.Next() && !cursor.CurrentLine.StartsWith(rulesTextText))
        {
            res.Flavor += " " + cursor.CurrentLine.ToString();
        }

        var stringEffect = cursor.LinesUntilEOS
            .Slice(c => c.IndexOf(rulesTextText) + rulesTextText.Length)
            .Trim()
            .ToString();

        // Divide the string into separate lines of actual effects.
        var effectSplit = stringEffect
            .Replace("[A]", "[AUTO]")
            .Replace("[C]", "[CONT]")
            .Replace("[S]", "[ACT]")
            .Replace("\n[AUTO]", "\n[A][AUTO]")
            .Replace("\n[CONT]", "\n[A][CONT]")
            .Replace("\n[ACT]", "\n[A][ACT]")
            .Split("\n[A]", StringSplitOptions.RemoveEmptyEntries);
        res.Effect = effectSplit.Select(s => Clean(s)).ToArray();
        res.Remarks = $"Extractor: {this.GetType().Name}";
        //            foreach (var effect in effectSplit) 
        //                Log.Information("Effect {serial}: {effect}", res.Serial, effect);

        Log.Information("Extracted: {serial}", res.Serial);
        return res;
        }

    private CardColor HandleColorCorrections(string serial, Exception innerException)
    {
        return serial switch
        {
            "SG/W70-106" => CardColor.Blue,
            "VA/WE30-55" => CardColor.Red,
            _ => throw new NotImplementedException($"Unsupported color correction for {serial}.", innerException)
        };
    }

    private string HandleRarityCorrections(string serial, Exception? innerException)
    {
        return serial switch
        {
            // https://ws-tcg.com/cardlist/?cardno=LS/W05-124&l
            "LS/W05-124" => "PR",
            _ => throw new NotImplementedException($"Unsupported rarity correction for {serial}.", innerException)
        };
    }

    /// <summary>
    /// Handle exceptional corrections that are caused by HOTC being absolutely wrong.
    /// </summary>
    /// <param name="serial"></param>
    /// <returns></returns>
    private (CardSide, CardType)  HandleCorrections(string serial)
    {
        return serial switch
        {
            "MR/W59-075" => (CardSide.Weiss, CardType.Climax),
            "MR/W59-076" => (CardSide.Weiss, CardType.Climax),
            _ => throw new NotImplementedException()
        };
    }

    private string Clean(string hotcEffectText)
    {
        return hotcEffectText.Trim().Replace("\n", " ").Replace("\r", "");
    }

    private static readonly Regex traitMatcher = new Regex(@"([^\(]+)\(((?:[^()]|(?<Open>[(])|(?<-Open>[)]))*(?(Open)(?!)))\),{0,1}");

    private MultiLanguageString? ParseTrait(String traitString)
    {
        if (!IsValidTrait(traitString)) return null;
        var group = traitMatcher.Matches(traitString).First().Groups;
        Log.Debug("Parsing trait: {traitString}", traitString);
        MultiLanguageString result = new MultiLanguageString();
        result["jp"] = group[1].Value.Trim();
        result["en"] = group[3].Value.Trim();
        Log.Debug("All Groups: {@groups}", group.OfType<Group>().Select(g => g.Value).ToArray());
        return result;
    }

    private WeissSchwarzTrait? ParseTrait(Match match)
    {
        WeissSchwarzTrait result = new WeissSchwarzTrait();
        result["jp"] = match.Groups[1].Value.Trim();
        result["en"] = match.Groups[2].Value.Trim();
        Log.Debug("All Groups: {@groups}", match.Groups.OfType<Group>().Select(g => g.Value).ToArray());
        if (result["jp"] == "特徴なし") // No Traits
            return null;
        else
            return result;
    }

    private bool IsValidTrait(string traitString)
    {
        return traitMatcher.Matches(traitString).Count > 0;
    }
    private Trigger[] TranslateTriggers(string triggerString)
    {
        if (triggerString.StartsWith("None")) return new Trigger[] { };
        if (triggerString.StartsWith("2 Soul")) return new Trigger[] { Trigger.Soul, Trigger.Soul };
        triggerString = triggerString.Replace("Draw", "Book");
        triggerString = triggerString.Replace("Salvage", "Door");
        triggerString = triggerString.Replace("Treasure", "GoldBar");
        triggerString = triggerString.Replace("Stock", "Bag");
        return triggerString.Split(" ")
            .Select(s => s.AsSpan().ToEnum<Trigger>())
            .Where(e => e is not null)
            .Select(e => e!.Value)
            .ToArray();
    }
}
