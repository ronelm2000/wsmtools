using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Flurl.Http;
using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Exceptions;
using Montage.Card.API.Interfaces.Components;
using Montage.Card.API.Interfaces.Services;
using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.PostProcessors;
using Montage.Weiss.Tools.Impls.Utilities;
using Montage.Weiss.Tools.Utilities;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Web;

namespace Montage.Weiss.Tools.Impls.Parsers.Cards;

/// <summary>
/// Parses results from the English site. This is done using an exploit on cardsearch that allows more than 100 cards as a single query.
/// This being an exploit means that at some time in the future this won't work.
/// </summary>
public class EnglishWSURLParser : ICardSetParser<WeissSchwarzCard>, IFilter<ICardPostProcessor<WeissSchwarzCard>>
{
    ILogger Log = Serilog.Log.ForContext<EnglishWSURLParser>();

    private static readonly string _WS_CARD_SEARCH_EX_FORMAT = "https://en.ws-tcg.com/cardlist/cardsearch_ex?expansion={0}&view=text&page={1}";
    
    private static readonly Regex _SOUL_MATCHER = new Regex(@"partimages/soul\.gif");

    /// <summary>
    /// These are sets that use the old format. The newest sets are separated by br tags which make them easy to split, while the old ones (this list) do not.
    /// </summary>
    private static readonly string[] _SETS_USING_OLD_FORMAT = new[]
    {
        "W08", "W31", "EN-W01"
    };

    // Selectors used when querying document nodes
    private static readonly string _LIST_ITEM_SELECTOR = "li";
    private static readonly string _ANCHOR_SELECTOR = "a";
    private static readonly string _IMG_SELECTOR = "img";

    private static readonly string _EXPANSION_LINKS_SELECTOR = "ul.p-cards__cardset-link a";
    private static readonly string _EXPANSION_LIST_ITEM_SELECTOR = ".cardlist-Result_List > li";

    private static readonly string _SERIAL_SELECTOR = ".p-cards__detail-textarea > .number";
    private static readonly string _DETAIL_NAME_SELECTOR = ".p-cards__detail-textarea > p.ttl";
    private static readonly string _TRAITS_SELECTOR = ".p-cards__detail-type > dl:nth-child(2) > dd";
    private static readonly string _TYPE_SELECTOR = ".p-cards__detail-type > dl:nth-child(3) > dd";
    private static readonly string _RARITY_SELECTOR = ".p-cards__detail-type > dl:nth-child(4) > dd";
    private static readonly string _SIDE_SELECTOR = ".p-cards__detail-type > dl:nth-child(5) > dd";
    private static readonly string _COLOR_SELECTOR = ".p-cards__detail-type > dl:nth-child(6) > dd";

    private static readonly string _LEVEL_SELECTOR = ".p-cards__detail-status dl:nth-child(1) > dd";
    private static readonly string _COST_SELECTOR = ".p-cards__detail-status dl:nth-child(2) > dd";
    private static readonly string _POWER_SELECTOR = ".p-cards__detail-status dl:nth-child(3) > dd";
    private static readonly string _TRIGGERS_SELECTOR = ".p-cards__detail-status dl:nth-child(4) > dd";
    private static readonly string _SOUL_SELECTOR = ".p-cards__detail-status dl:nth-child(5) > dd";

    private static readonly string _EFFECT_PARAGRAPHS_SELECTOR = ".p-cards__detail.u-mt-22 p";
    private static readonly string _FLAVOR_SELECTOR = ".p-cards__detail-serif.u-mt-22";
    private static readonly string _CARD_IMAGE_SELECTOR = ".p-cards__detail-wrapper-inner > .image > img";


    private static (string LookupString, CardColor Color)[] _COLOR_MAP = new[]
        {
            ("partimages/yellow.gif", CardColor.Yellow),
            ("partimages/green.gif", CardColor.Green),
            ("partimages/red.gif", CardColor.Red),
            ("partimages/blue.gif", CardColor.Blue)
            // Purple is unsupported by ENWS
        };

    private static Dictionary<string, CardColor> _COLOR_EXCEPTION_MAP = new Dictionary<string, CardColor>
    {
        // https://en.ws-tcg.com/cardlist/list/?cardno=FS/S36-PE02
        ["赤"] = CardColor.Red 
    };

    private static (string LookupString, Trigger CardTrigger)[] _TRIGGER_MAP = new[]
    {
        ("partimages/soul.gif", Trigger.Soul),
        ("partimages/bounce.gif", Trigger.Bounce),
        ("partimages/shot.gif", Trigger.Shot),
        ("partimages/choice.gif", Trigger.Choice),
        ("partimages/treasure.gif", Trigger.GoldBar),
        ("partimages/stock.gif", Trigger.Bag),
        ("partimages/standby.gif", Trigger.Standby),
        ("partimages/comeback.gif", Trigger.Door),
        ("partimages/gate.gif", Trigger.Gate),
        ("partimages/draw.gif", Trigger.Book)
    };

    private static (string LookupString, string Replacement)[] _EFFECT_REPLACEMENT_MAP = new[]
    {
        // https://en.ws-tcg.com/cardlist/list/?cardno=FS/S36-E008 and https://en.ws-tcg.com/cardlist/list/?cardno=BD/W63-E003
        ("<img src=\"../partimages/soul.gif\">", "【SOUL】"),
        // https://en.ws-tcg.com/cardlist/list/?cardno=SY/W08-E086
        ("Draw@", "【DRAW】"),
        // https://en.ws-tcg.com/cardlist/list/?cardno=SY/W08-E004
        ("<br>", "\n"),
        ("<img src=\"/wordpress/wp-content/images/partimages/bounce.gif\">", "【BOUNCE】"),
        ("<img src=\"/wordpress/wp-content/images/partimages/shot.gif\">", "【SHOT】"),
        ("<img src=\"/wordpress/wp-content/images/partimages/choice.gif\">", "【CHOICE】"),
        ("<img src=\"/wordpress/wp-content/images/partimages/treasure.gif\">", "【GOLD】"),
        ("<img src=\"/wordpress/wp-content/images/partimages/stock.gif\">", "【BAG】"),
        ("<img src=\"/wordpress/wp-content/images/partimages/standby.gif\">", "【STANDBY】"),
        ("<img src=\"/wordpress/wp-content/images/partimages/comeback.gif\">", "【DOOR】"),
        ("<img src=\"/wordpress/wp-content/images/partimages/gate.gif\">", "【GATE】"),
        ("<img src=\"/wordpress/wp-content/images/partimages/draw.gif\">", "【BOOK】"),
        
        ("<strong>", ""),
        ("</strong>", "")
    };

    private GlobalCookieJar cookieJar;

    public EnglishWSURLParser(GlobalCookieJar cookieJar)
    {
        this.cookieJar = cookieJar;
    }

    public async Task<bool> IsCompatible(IParseInfo parseInfo)
    {
        var urlOrFile = parseInfo.URI;
        try
        {
            await ValueTask.CompletedTask;

            if (!Uri.TryCreate(urlOrFile, UriKind.Absolute, out var uri))
            {
                Log.Debug("Not a URI. Failed compatibility check.", urlOrFile);
                return false;
            }
            else if (uri.Authority != "en.ws-tcg.com")
            {
                Log.Debug("The site is not EN WSTCG website. Failed compatibility check.");
                return false;
            }
            else if (uri.AbsolutePath != "/cardlist/")
            {
                Log.Debug("The site is not based on the cardlist. Failed compatibility check.");
                return false;
            }
            else if (!uri.Query.StartsWith("?cardno="))
            {
                Log.Debug("The site is not based on the cardlist (not having a compatible query). Failed compatibility check.");
                return false;
            }
            else
            {
                return true;
            }
        }
        catch (Exception e)
        {
            Log.Debug("Unspecified Error during compatibility checking.");
            Log.Debug(e.Message);
            return false;
        }
    }

    public bool IsIncluded(ICardPostProcessor<WeissSchwarzCard> item)
    {
        if (item is DeckLogPostProcessor)
        {
            Log.Information("DeckLog is excluded as all of its data is already parsed by this parser.");
            return false;
        } else
        {
            return true;
        }
    }

    public async IAsyncEnumerable<WeissSchwarzCard> Parse(string urlOrLocalFile, IProgress<SetParserProgressReport> progress, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Log.Information("Starting...");

        var progressReport = new SetParserProgressReport
        {
            CardsParsed = 0,
            Percentage = 0,
            ReportMessage = new MultiLanguageString { EN = $"Getting cards from [{urlOrLocalFile}]... " }
        };
        progress.Report(progressReport);

        var uri = new Uri(urlOrLocalFile);

        Log.Information("Loading the page...");

        var cookies = await cookieJar.FindOrCreate(uri.AbsoluteUri, cancellationToken);
        if (cookies.Count == 0) {
            cookies.AddOrReplace("CookieConsent", """
            {stamp:'C88lsVhKsZTmlWfRS4nPyaJlCJHtwki7LoERTcJyFHgUzKNw8RgRNQ==',necessary:true,preferences:true,statistics:true,marketing:true,method:'explicit',ver:1,utc:1772352864544,region:'ph'}")
            """, urlOrLocalFile);
            cookies.AddOrReplace("cardlist_search_sort", "new", urlOrLocalFile);
            cookies.AddOrReplace("cardlist_view", "text", urlOrLocalFile);
        }

        var initialDocument = await uri.WithCookies(cookies)
            .WithHTMLHeaders()
            .GetHTMLAsync(cancellationToken);
        var expansionLink = initialDocument.QuerySelectorAll<IHtmlAnchorElement>(_EXPANSION_LINKS_SELECTOR).FirstOrEmpty()!.Href;

        Log.Information("Found Expansion Link: {expansionLink}", expansionLink);

        var expansionInitialDocument = await expansionLink
            .WithHTMLHeaders()
            .WithCookies(cookies)
            .GetHTMLAsync(cancellationToken);
        var list = expansionInitialDocument.QuerySelectorAll<IHtmlListItemElement>(_EXPANSION_LIST_ITEM_SELECTOR);


        var expansion = HttpUtility.ParseQueryString(new Uri(expansionLink).Query)["expansion"];
        var page = 2;
        var isRedirected = false;

        while (!isRedirected)
        {
            try
            {
                var nextLink = await String.Format(_WS_CARD_SEARCH_EX_FORMAT, expansion, page)
                    .WithHTMLHeaders()
                    .WithCookies(cookies)
                    .WithAutoRedirect(false)
                    .GetAsync(cancellationToken: cancellationToken);
                Log.Information("Trying to access {url}... Status: {statusCode}", nextLink.ResponseMessage?.RequestMessage?.RequestUri, ((int?)nextLink.ResponseMessage?.StatusCode));
                isRedirected = (!nextLink.ResponseMessage?.IsSuccessStatusCode ?? true);
                page++;

                var content = await nextLink.RecieveHTML(cancellationToken);
                list = list.Concat(content.QuerySelectorAll<IHtmlListItemElement>(_LIST_ITEM_SELECTOR).ToArray());
            }
            catch (FlurlHttpException e)
            {
                Log.Information("Trying to access {url}... Failed with exception: {message}", String.Format(_WS_CARD_SEARCH_EX_FORMAT, expansion, page), e.Message);
                isRedirected = true;
            }
        }

        foreach (var li in list)
        {
            var cardLink = li.QuerySelector<IHtmlAnchorElement>(_ANCHOR_SELECTOR)?.Href;
            if (cardLink is not null)
            {
                var card = await ParseSingleCard(cardLink!, cookies, cancellationToken);
                progress.Report(progressReport = progressReport with
                {
                    ReportMessage = new MultiLanguageString { EN = $"Found card: {card.Serial} [{card.Name.EN}]" },
                    Percentage = 50
                });
                yield return card;
            }
        }

        Log.Information("Ending...");
        progressReport = progressReport with
        {
            ReportMessage = new MultiLanguageString { EN = $"Parsed all cards." },
            Percentage = 100
        };
        progress.Report(progressReport);
        yield break;
    }

    private async Task<WeissSchwarzCard> ParseSingleCard(string cardLink, CookieJar cookies, CancellationToken cancellationToken)
    {
        Log.Debug("Parsing card page: {cardLink}", cardLink);

        var document = await cardLink.WithCookies(cookies)
            .WithHTMLHeaders()
            .GetHTMLAsync(cancellationToken);

        WeissSchwarzCard res = new WeissSchwarzCard();

        res.Serial = document.QuerySelector(_SERIAL_SELECTOR)!.InnerHtml;
        res.Name = new MultiLanguageString() { EN = document.QuerySelector(_DETAIL_NAME_SELECTOR)!.InnerHtml, JP = "" };
        res.Traits = document.QuerySelector(_TRAITS_SELECTOR)!.InnerHtml
            .Split("・")
            .Select(s => new WeissSchwarzTrait() { EN = s, JP = "" })
            .ToList();
        res.Type = document.QuerySelector(_TYPE_SELECTOR)!.InnerHtml
            .Trim()
            .ToEnum<CardType>() ?? throw new SetParsingException(new CannotBeParsedCode("CardType"));
        res.Rarity = document.QuerySelector(_RARITY_SELECTOR)!.InnerHtml.Trim();
        res.Side = TranslateToSide(document.QuerySelector(_SIDE_SELECTOR)!.InnerHtml);
        res.Color =TranslateToColor(document.QuerySelector(_COLOR_SELECTOR)!.InnerHtml);

        res.Level = document.QuerySelector(_LEVEL_SELECTOR)!.InnerHtml.Trim().AsParsed<int>(int.TryParse);
        res.Cost = document.QuerySelector(_COST_SELECTOR)!.InnerHtml.Trim().AsParsed<int>(int.TryParse);
        res.Power = document.QuerySelector(_POWER_SELECTOR)!.InnerHtml.Trim().AsParsed<int>(int.TryParse);
        res.Triggers = await TranslateToTriggers(document.QuerySelector(_TRIGGERS_SELECTOR)!.InnerHtml);
        res.Soul = CountSouls(document.QuerySelector(_SOUL_SELECTOR)!.InnerHtml);

        res.Effect = document.QuerySelectorAll(_EFFECT_PARAGRAPHS_SELECTOR)!
            .Select(x => x.InnerHtml)
            .SelectMany(x =>x.Split(new string[] { "<br>", "\n", "<p>", "</p>" }, StringSplitOptions.RemoveEmptyEntries))
            .Select(e => CleanupEffect(e))
            .ToArray();
        
        res.Flavor = document.QuerySelector(_FLAVOR_SELECTOR)!.TextContent?.Trim() ?? string.Empty;
        res.Images.Add(new Uri(document.QuerySelector<IHtmlImageElement>(_CARD_IMAGE_SELECTOR)!.Source!));

        res.Remarks = $"Extractor: {this.GetType().Name}";

        Log.Information("Parsed card: {serial} [{name}]", res.Serial, res.Name.EN);
        return res;
    }

    private string CleanupEffect(string effectText)
    {
        var res = effectText;
        foreach (var textPair in _EFFECT_REPLACEMENT_MAP)
            res = res.Replace(textPair.LookupString, textPair.Replacement);
        return res;
    }

    private async Task<Trigger[]> TranslateToTriggers(string value)
    {
        var doc = await value.ParseHTML();
        return doc.QuerySelectorAll<IHtmlImageElement>(_IMG_SELECTOR)
            .SelectMany(e => _TRIGGER_MAP.Where(t => e.Source?.Contains(t.LookupString) ?? false))
            .Select(t => t.CardTrigger)
            .ToArray();
    }

    private int? CountSouls(string value)
    {
        return _SOUL_MATCHER.Matches(value).Count;
    }

    private CardColor TranslateToColor(string value)
    {
        foreach (var colorLookupPair in _COLOR_MAP)
            if (value.Contains(colorLookupPair.LookupString)) return colorLookupPair.Color;

        if (_COLOR_EXCEPTION_MAP.TryGetValue(value.Trim(), out var entry))
            return entry;

        throw new NotImplementedException();
    }

    private CardSide TranslateToSide(string value)
    {
        var hasWeiss = value.Contains("partimages/w.gif");
        var hasSchwarz = value.Contains("partimages/s.gif");
        if (hasWeiss && hasSchwarz)
            return CardSide.Both;
        else if (hasWeiss)
            return CardSide.Weiss;
        else if (hasSchwarz)
            return CardSide.Schwarz;
        else
            throw new NotImplementedException("Current value is invalid: " + value);
    }
}
