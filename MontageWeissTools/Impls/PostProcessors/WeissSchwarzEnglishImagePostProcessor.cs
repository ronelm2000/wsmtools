using Flurl.Http;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using Serilog;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.PostProcessors
{
    public class WeissSchwarzEnglishImagePostProcessor : ICardPostProcessor
    {
        public int Priority => -1;
        private static readonly string _imagePrefix = "https://en.ws-tcg.com/wp/wp-content/images/cardimages";
        private readonly ILogger Log = Serilog.Log.ForContext<WeissSchwarzEnglishImagePostProcessor>();


        public async Task<bool> IsCompatible(List<WeissSchwarzCard> cards)
        {
            var firstCard = cards.First();
            if (firstCard.Language != CardLanguage.English)
                return false;
            else if (await IsImageOutput(ConstructURL(firstCard)))
                return true;
            else
            {
                Log.Warning("Set is not using the new WS TCG website format. Skipping.");
                return false;
            }
        }

        public async IAsyncEnumerable<WeissSchwarzCard> Process(IAsyncEnumerable<WeissSchwarzCard> originalCards)
        {
            await foreach (var card in originalCards)
            {
                var cardURI = ConstructURL(card);
                if (await IsImageOutput(cardURI))
                {
                    card.Images.Add(cardURI);
                    Log.Information("Added: {image}", cardURI);
                }
                yield return card;
            }
        }

        private async Task<bool> IsImageOutput(Uri uri)
        {
            try
            {
                var stream = await uri.WithImageHeaders()
                        .WithTimeout(TimeSpan.FromSeconds(5))
                        .GetStreamAsync()
                        ;
                var image = Image.Load(stream);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private Uri ConstructURL(WeissSchwarzCard card)
        {
            return new Uri($"{_imagePrefix}/{card.TitleCode}/{card.Serial.Replace('-', '_').AsFileNameFriendly()}.png");
        }
    }
}
