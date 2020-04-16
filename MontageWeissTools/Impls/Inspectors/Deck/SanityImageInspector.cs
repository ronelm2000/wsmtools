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

namespace Montage.Weiss.Tools.Impls.Inspectors.Deck
{
    public class SanityImageInspector : IExportedDeckInspector
    {
        private readonly ILogger Log = Serilog.Log.ForContext<SanityImageInspector>();

        public int Priority => 0;

        public async Task<WeissSchwarzDeck> Inspect(WeissSchwarzDeck deck, InspectionOptions options)
        {
            Log.Debug("Starting...");
            var keyCards = deck.Ratios.Keys.Where(c => ((!c.Images?.Any()) ?? false) && String.IsNullOrWhiteSpace(c.CachedImagePath));
            var inspectedDeck = deck.Clone();
            foreach (var card in keyCards)
            {
                Log.Information("{serial} has no image URL nor a cached image. This deck cannot be exported without an image. Do you want to supply an image instead? [Y/N] (Default is N)", card.Serial);
                if (ConsoleUtils.Prompted(options.IsNonInteractive, false))
                {
                    if (inspectedDeck.ReplaceCard(card, await AddImageFromConsole(card, options)))
                        Log.Information("Checking for other missing images (if any)...");
                    else
                        return WeissSchwarzDeck.Empty;
                }
                else
                {
                    Log.Information("Selected No; inspection failed.");
                    return WeissSchwarzDeck.Empty;
                }
            }
            Log.Debug("Finished inspection.");
            return inspectedDeck;
        }

        private async Task<WeissSchwarzCard> AddImageFromConsole(WeissSchwarzCard card, InspectionOptions options)
        {
            var modifiedCard = card.Clone();
            Log.Information("Please enter a new image URL: ");
            var newURIString = Console.ReadLine();
            try
            {
                Log.Information("Validating URL...");
                var newURI = new Uri(newURIString);
                Log.Information("Looks good; validating image... (will not check if the image itself is correct!)");
                using (Image img = Image.Load(await newURI.WithImageHeaders().GetStreamAsync()))
                {
                    Log.Information("Image can be loaded. Is the ratio reasonable?");
                    var aspectRatio = (img.Width * 1.0d) / img.Height;
                    if (Math.Floor(aspectRatio * 100) - 71 <= 1)
                    {
                        Log.Information("Image Ratio ({aspectRatio}) isn't correct. Failed inspection.", aspectRatio);
                        return null;
                    }
                    else
                    {
                        if (img.Width < 400)
                        {
                            Log.Warning("The image is of low quality; this may not be good for exporting purposes. Continue? [Y/N] (Default is N)");
                            if (!ConsoleUtils.Prompted(options.IsNonInteractive, options.NoWarning)) return null;
                        }
                        modifiedCard.Images.Add(newURI);
                        Log.Information("All preliminary tests passed. Modified {card}.", card.Serial);
                    }
                }
            }
            catch (UnknownImageFormatException)
            {
                Log.Error("The URL does not point to a valid image. Inspection failed.");
                return null;
            }
            catch (Exception e)
            {
                Log.Error("{e}", e);
                return null;
            }
            return modifiedCard;
        }

        /*
        private bool Prompted(bool isNonInteractive)
        {
            if (isNonInteractive) return false;
            var result = Console.ReadKey(false).Key == ConsoleKey.Y;
            Console.Write("\r");
            return result;
        }
        */
    }
}
