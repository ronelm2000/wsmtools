﻿using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Interfaces.Components;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Web;

namespace Montage.Weiss.Tools.Impls.PostProcessors;

/// <summary>
/// Applies post-processing by searching in yuyutei for images of the cards and inserting them in.
/// </summary>
public class YuyuteiPostProcessor : ICardPostProcessor<WeissSchwarzCard>, ISkippable<IParseInfo>
{
    private readonly static ILogger Log = Serilog.Log.ForContext<YuyuteiPostProcessor>();

    public int Priority => 0;

    private static readonly string rarityClassPrefix = "rarity_";
    private static readonly string cardUnitListItemSelector = "#main .card_unit";
    private static readonly string cardUnitImageSelector = ".image_box > a > .image > img";
    private static readonly string cardUnitSerialSelector = ".headline > p.id";
    private static readonly string cardUnitPriceSelector = ".price";
    private static readonly string cardUnitSaleSelector = ".sale";

    // Exceptional Sets
    private static string[] gfbException = new[] { "W33", "W38" };

    // Dependencies
    private readonly Func<CardDatabaseContext> _database;

    public YuyuteiPostProcessor(IContainer container)
    {
        _database = () => container.GetInstance<CardDatabaseContext>();
    }

    public async Task<bool> IsCompatible(List<WeissSchwarzCard> cards)
    {
        await ValueTask.CompletedTask;
        if (cards.Count == 0)
            return false;
        if (cards.First().Language != CardLanguage.Japanese)
            return false;
        var list = cards.Select(c => c.ReleaseID).Distinct().ToList();
        if (IsExceptional(list))
        {
            return true;
        }
        else if (list.Count > 1)
        {
            Log.Warning("Yuyutei Image Post-Processor is disabled for sets with multiple Release IDs; please add those images manually when prompted.");
            return false;
        }
        else
            return true;
    }

    private bool IsExceptional(List<string> releaseIDList)
    {
        if (releaseIDList.OrderBy(s => s).SequenceEqual(gfbException))
        {
            Log.Information("This is normally not allowed, but GFB vol. 2 has cards from GFB vol. 1, so YYT should allow this.");
            return true;
        }
        else
            return false;
    }

    public async Task<bool> IsIncluded(IParseInfo info)
    {
        await Task.CompletedTask;
        if (info.ParserHints.Select(s => s.ToLower()).Contains("skip:yyt"))
        {
            Log.Information("Skipping due to the parser hint [skip:yyt].");
            return false;
        }
        else if (info.ParserHints.Select(s => s.ToLower()).Contains("skip:external"))
        {
            Log.Information("Skipping due to the parser hint [skip:external].");
            return false;
        }
        else if (info.ParserHints.Select(s => s.ToLower()).Contains("noskip:yyt"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public async IAsyncEnumerable<WeissSchwarzCard> Process(IAsyncEnumerable<WeissSchwarzCard> originalCards, IProgress<PostProcessorProgressReport> progress, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var yuyuteiSellPage = "https://yuyu-tei.jp/game_ws/sell/sell_price.php?name=";

        //var cards = await originalCards.ToArrayAsync(cancellationToken);
        //.Skip(Math.Min(cards.Length - 1, 10)).First(); 
        var firstCard = await originalCards.Take(10).LastAsync(cancellationToken); // wtf GFB. Why you do this to me.
        var setCode = firstCard.ReleaseID;
        var lang = firstCard.Language;
        var postProcessingDateTime = DateTime.Now;

        if (lang == CardLanguage.English) // Yuyutei Inquiry will just crash
        {
            await foreach (var card in originalCards) yield return card;
            yield break;
        }

        Log.Information("Starting...");

        yuyuteiSellPage += HttpUtility.UrlEncode(setCode);
        Log.Information("Loading: {yuyuteiSellPage}", yuyuteiSellPage);

        IDocument yuyuteiSearchPage = await new Uri(yuyuteiSellPage).DownloadHTML(cancellationToken, ("Referer", "https://yuyu-tei.jp/")).WithRetries(10);

        var cardUnitListItems = yuyuteiSearchPage.QuerySelectorAll(cardUnitListItemSelector);
        // Caution on https://yuyu-tei.jp/game_ws/sell/sell_price.php?name=BD%2fW54
        var yytInfoDict = cardUnitListItems.AsParallel()
            .Select(Serialize)
            .Distinct(yyti => yyti.Serial + yyti.Rarity)
            .ToDictionary(yyti => new WSKey(yyti.Serial, yyti.Rarity), yyti => yyti);

        Log.Information("Processing all new cards...");
        Log.Debug("YYT Info Dict Count: {count}", yytInfoDict.Count);

        await foreach (var originalCard in originalCards)
            if (yytInfoDict.TryGetValue(new WSKey(originalCard.Serial, originalCard.Rarity), out var info))
            {
                yield return Process(originalCard, info, postProcessingDateTime);
            }
            else
            {
                Log.Information("Cannot find info for: {serial} {rarity}", originalCard.Serial, originalCard.Rarity);
                yield return originalCard;
            }

        Log.Information("Ended.");
        yield break;
    }

    private static WSYYTInfo Serialize(IElement cardUnitDiv)
    {
        WSYYTInfo info = new();
        info.Rarity = cardUnitDiv.ClassList
            .Where(s => s.StartsWith(rarityClassPrefix))
            .Select(s => s.Substring(rarityClassPrefix.Length)).First();
        info.Serial = cardUnitDiv.QuerySelector(cardUnitSerialSelector)!.Text().Trim();
        info.ImageUri = cardUnitDiv.QuerySelector<IHtmlImageElement>(cardUnitImageSelector)
            ?.Source
            ?.Replace("ws/90_126", "ws/front") ?? string.Empty;
        var priceStringSelector = cardUnitDiv.QuerySelector(cardUnitPriceSelector);
        var priceString = priceStringSelector
            ?.QuerySelector(cardUnitSaleSelector)
            ?.Text()
            .Trim()
            .Replace("円", "") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(priceString))
            priceString = priceStringSelector?.Text()
                .Trim()
                .Replace("円", "") ?? "0";
        info.Price = int.Parse(priceString);

        return HandleExceptions(info);
    }

    private static WSYYTInfo HandleExceptions(WSYYTInfo rawInfo)
    {
        return rawInfo switch
        {
            // Fix Exceptional Serial on GU/57 caused by the serial being the same serial in SEC and in normal rarity.
            var tup when tup.Rarity == "SEC" && tup.Serial.StartsWith("GU/W57") => rawInfo with { Rarity = tup.Serial + tup.Rarity },
            // Fix Exceptional CC rarity when it's supposed to be a regular C for all Extra Boosters
            var tup when (tup.Serial.Contains("/WE") || tup.Serial.Contains("/SE"))
                       && (tup.Rarity.EndsWith("CC") || tup.Rarity.EndsWith("CU"))
              => rawInfo with { Serial = tup.Rarity.Substring(0, tup.Rarity.Length - 2) + tup.Rarity.Last() },
            _ => rawInfo
        };
    }

    private static WeissSchwarzCard Process(WeissSchwarzCard original, WSYYTInfo info, DateTime timestamp)
    {
        Log.Information("Processing {serial}", original.Serial);

        if (original.Images.Any(u => u.Authority == "yuyu-tei.jp"))
            original.Images.Add(new Uri(info.ImageUri));
        Dictionary<DateTime, int> priceTable = null!;
        if (original.AdditionalInfo.Any(i => i.Key == "yyt.price.info"))
            priceTable = original.AdditionalInfo
                .First(i => i.Key == "yyt.price.info")
                .DeserializeValue<Dictionary<DateTime, int>>() ?? new();
        else
            priceTable = new();

        priceTable.Add(timestamp, info.Price);

        original.AddOptionalInfo("yyt.price.info", priceTable);

        return original;
    }

    private record WSYYTInfo
    {
        internal string Serial { get; set; } = string.Empty;
        internal string Rarity { get; set; } = string.Empty;
        internal string ImageUri { get; set; } = string.Empty;
        internal int Price { get; set; }
    }

    private readonly record struct WSKey (string Serial, string Rarity);
}
