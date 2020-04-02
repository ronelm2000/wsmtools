using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using Montage.Weiss.Tools.API;

namespace Montage.Weiss.Tools.Impls.Parsers.Cards
{
    public class HeartOfTheCardsURLParser : ICardSetParser
    {
        private ILogger Log = Serilog.Log.ForContext<HeartOfTheCardsURLParser>();

        public bool IsCompatible(String urlOrFile)
        {
            if (!Uri.TryCreate(urlOrFile, UriKind.Absolute, out Uri url))
            {
                Log.Information("Not compatible because not a url: {urlOrFile}", urlOrFile);
                return false;
            }
            if (url.Authority != "heartofthecards.com" && url.Authority != "www.heartofthecards.com")
            {
                Log.Information("Not compatible because {Authority} is not heartofthecards.com", url.Authority);
                return false;
            }

            if (!url.AbsolutePath.StartsWith("/translations/"))
            {
                Log.Information("Not compatible because {AbsolutePath} does not start with /translations.", url.AbsolutePath);
                return false;
            }
            if (url.AbsolutePath == "/translations/")
            {
                Log.Information("Not compatible because absolute path cannot be /translations/ itself; please provide a set html.");
                return false;
            }
            Log.Information("Selected.");
            return true;
        }

        public async IAsyncEnumerable<WeissSchwarzCard> Parse(String url)
        {
            Log.Information("Starting. URI: {url}", url);
            var html = await new Uri(url).DownloadHTML();
            var preSelector = "td > pre";
            var textToProcess = html.QuerySelector(preSelector);
            var majorSeparator = "================================================================================";
            
            var results = textToProcess.TextContent.Split(majorSeparator)
                .AsEnumerable()
                .Skip(1)
                .SkipLast(1)
                .Select(section => ParseHOTCText(section))
                ;
            foreach (var card in results)
                yield return card;
            
        }

        private WeissSchwarzCard ParseHOTCText(string hotcText)
        {
            var cursor = hotcText.AsSpanCursor();
            var res = new WeissSchwarzCard();

            var cardNoText = "Card No.: ";
            var rarityText = "Rarity: ";
            var colorText = "Color: ";
            var sideText = "Side: ";
            var levelText = "Level: ";
            var costText = "Cost: ";
            var powerText = "Power: ";
            var soulText = "Soul: ";
            var traitsText = "Traits: ";
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
            res.Rarity = cursor.CurrentLine.Slice(c => c.IndexOf(rarityText) + rarityText.Length).Trim().ToString();

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
            cursor.Next(linesToCardNoText - 1);
            res.Color = cursor.CurrentLine.Slice(
                            c => c.IndexOf(colorText) + colorText.Length,
                            c => c.IndexOf(sideText)
                            )
                            .Trim()
                            .ToEnum<CardColor>()
                            .Value;

            try
            {
                res.Side = cursor.CurrentLine.Slice(
                                c => c.IndexOf(sideText) + sideText.Length,
                                c => c.IndexOf(sideText) + sideText.Length + c.Slice(c.IndexOf(sideText) + sideText.Length).IndexOf(' ')
                                )
                                .Trim()
                                .ToEnum<CardSide>()
                                .Value;

                var sideString = res.Side.ToString();
                res.Type = cursor.CurrentLine.Slice(
                                c => c.IndexOf(sideString, StringComparison.CurrentCultureIgnoreCase) + sideString.Length
                                )
                                .Trim()
                                .ToEnum<CardType>()
                                .Value;
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

            res.Traits = cursor.CurrentLine
                .Slice(c => c.IndexOf(traitsText) + traitsText.Length)
                .Trim()
                .ToString()
                .Split(",")
                .Select(this.ParseTrait)
                .Where(o => o != null)
                .ToList();

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

        private static readonly Regex traitMatcher = new Regex(@"([^\(]+)(\()([^\)]+)(\))[,]{0,1}");

        private MultiLanguageString ParseTrait(String traitString)
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

        private bool IsValidTrait(string traitString)
        {
            return traitMatcher.Matches(traitString).Count > 0;
        }
        private Trigger[] TranslateTriggers(string triggerString)
        {
            if (triggerString.StartsWith("None")) return new Trigger[] { };
            if (triggerString.StartsWith("2 Soul")) return new Trigger[] { Trigger.Soul, Trigger.Soul };
            triggerString = triggerString.Replace("Draw", "Book");
            return triggerString.Split(" ")
                .Select(s => s.AsSpan().ToEnum<Trigger>())
                .Where(e => e.HasValue)
                .Select(e => e.Value)
                .ToArray();
        }
    }
}
