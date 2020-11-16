using AngleSharp.Dom;
using Flurl.Http;
using Lamar;
using LamarCodeGeneration;
using Microsoft.Extensions.DependencyInjection;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Entities.Exceptions;
using Montage.Weiss.Tools.Utilities;
using Newtonsoft.Json;
using Octokit;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.Parsers.Deck
{
    /// <summary>
    /// Implements a Deck Parser that sources deck information from DeckLog.
    /// Note that parsing the deck this way means the deck has no name or description, but the source link will be appended.
    /// </summary>
    public class DeckLogParser : IDeckParser, IDisposable
    {
        private Regex urlMatcher = new Regex(@"(.*):\/\/decklog\.bushiroad\.com\/view\/([^\?]*)(.*)");
        private string deckLogApiUrlPrefix = "https://decklog.bushiroad.com/system/app/api/view/";
        private string awsWeissSchwarzSitePrefix = "https://s3-ap-northeast-1.amazonaws.com/static.ws-tcg.com/wordpress/wp-content/cardimages/";
        private ILogger Log = Serilog.Log.ForContext<DeckLogParser>();
        private readonly Func<CardDatabaseContext> _database;

        private bool disposedValue;

        public string[] Alias => new[] { "decklog" };

        public int Priority => 1;

        public DeckLogParser(IContainer ioc)
        {
            this._database = () => ioc.GetInstance<CardDatabaseContext>();
        }

        public async Task<bool> IsCompatible(string urlOrFile)
        {
            await Task.CompletedTask;
            if (Uri.TryCreate(urlOrFile, UriKind.Absolute, out _))
            {
                return urlMatcher.IsMatch(urlOrFile);
            }else
            {
                return false;
            }
        }

        public async Task<WeissSchwarzDeck> Parse(string sourceUrlOrFile)
        {
            var document = await sourceUrlOrFile.WithHTMLHeaders().GetHTMLAsync();
            //var deckView = document.QuerySelector(".deckview");
            //var cardControllers = deckView.QuerySelectorAll(".card-controller-inner");
            var deckID = urlMatcher.Match(sourceUrlOrFile).Groups[2];
            Log.Information("Parsing ID: {deckID}", deckID);
            var response = await $"{deckLogApiUrlPrefix}{deckID}" //
                .WithReferrer(sourceUrlOrFile) //
                .PostJsonAsync(null);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.GetStringAsync());
            //var json = JsonConverter.CreateDefault().Deserialize<dynamic>(new JsonReader(await response.Content.ReadAsStreamAsync()));
            var newDeck = new WeissSchwarzDeck();
            var missingSerials = new List<string>();
            newDeck.Name = json.title.ToString();
            newDeck.Remarks = json.memo.ToString();
            using (var db = _database())
            {
                List<dynamic> items = new List<dynamic>();
                items.AddRange(json.list);
                items.AddRange(json.sub_list);
                foreach (var cardJSON in items)
                {
                    string serial = cardJSON.card_number.ToString();
                    serial = serial.Replace('＋', '+');
                    if (serial == null)
                    {
                        Log.Warning("serial is null for some reason!");
                    }
                    var card = await db.WeissSchwarzCards.FindAsync(serial);
                    int quantity = cardJSON.num;
                    if (card != null)
                    {
                        Log.Debug("Adding: {card} [{quantity}]", card?.Serial, quantity);
                        if (newDeck.Ratios.TryGetValue(card, out int oldVal))
                            newDeck.Ratios[card] = oldVal + quantity;
                        else
                        {
                            var url = awsWeissSchwarzSitePrefix + cardJSON.img;
                            Log.Debug("Adding URL into Images: {url}", url);
                            card.Images.Add(new Uri(url));
                            newDeck.Ratios.Add(card, quantity);
                        }
                    } else
                    {
                        missingSerials.Add(serial);
                        //throw new DeckParsingException($"MISSING_SERIAL_{serial}");
                        Log.Debug("Serial has been effectively skipped because it's not found on the local db: [{serial}]", serial);
                    }
                }
            }
            if (missingSerials.Count > 0)
                throw new DeckParsingException($"The following serials are missing from the DB:\n{missingSerials.ConcatAsString("\n")}");
            else
            {
                Log.Debug($"Result Deck: {JsonConvert.SerializeObject(newDeck.AsSimpleDictionary())}");
                return newDeck;
            }
        }
    }
}
