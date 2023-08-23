using Flurl.Http;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Utilities;
using Montage.Weiss.Tools.Utilities;
using SixLabors.ImageSharp;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Montage.Weiss.Tools.Impls.Inspectors.Deck;

public class SanityImageInspector : IExportedDeckInspector<WeissSchwarzDeck, WeissSchwarzCard>
{
    private readonly ILogger Log = Serilog.Log.ForContext<SanityImageInspector>();

    private GlobalCookieJar _globalCookieJar;
    private CardDatabaseContext _db;

    public SanityImageInspector(GlobalCookieJar globalCookieJar, CardDatabaseContext db)
    {
        this._globalCookieJar = globalCookieJar;
        this._db = db;
    }

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
                var newCard = await AddImageFromConsole(card, options);
                if (newCard is not null && inspectedDeck.ReplaceCard(card, newCard))
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

        Log.Information("Detecting broken image links...");
        var brokenLinkKeyCards = deck.Ratios.Keys.ToAsyncEnumerable()
            .WhereAwaitWithCancellation(async (card, ct) => !(await card.IsImagePresentAsync(_globalCookieJar[card.Images[^1].Host], ct)));

        await foreach (var card in brokenLinkKeyCards.WithCancellation(options.CancellationToken))
        {
            var nonBrokenLink = card.Images.Reverse<Uri>()
                .Concat(_db.FindFoils(card).SelectMany(c => c.Images))           
                .ToAsyncEnumerable()
                .FirstAwaitWithCancellationAsync(async (u, ct) => await IsImagePresent(u, ct), options.CancellationToken);

            var modifiedCard = card.Clone();
            modifiedCard.Images.Add(await nonBrokenLink);
            deck.ReplaceCard(card, modifiedCard);

            Log.Information("Replaced {card}'s URL to: {url}", card, nonBrokenLink);
        }

        Log.Debug("Finished inspection.");
        return inspectedDeck;
    }

    private async Task<bool> IsImagePresent(Uri url, CancellationToken ct)
    {
        try
        {
            using Image img = await Image.LoadAsync(await url.WithImageHeaders().GetStreamAsync(), ct);
            return !IsBadQuality(img);
        } catch (Exception ex)
        {
            return false;
        }
    }

    private bool IsBadQuality(Image img)
    {
        var aspectRatio = (img.Width * 1.0d) / img.Height;
        var flooredAspectRatio = Math.Floor(aspectRatio * 100);
        if (flooredAspectRatio < 67 || flooredAspectRatio > 72)
        {
            Log.Information("Image Ratio ({aspectRatio}) isn't correct (it must be approx. 0.71428571428); Rejecting replacement image.", aspectRatio);
            return true;
        }

        if (img.Width < 400)
        {
            Log.Warning("The image is of low quality. Rejecting replacement image.");
            return true;
        }

        return false;
    }

    private async Task<WeissSchwarzCard?> AddImageFromConsole(WeissSchwarzCard card, InspectionOptions options)
    {
        var modifiedCard = card.Clone();
        Log.Information("Please enter a new image URL: ");
        var newURIString = Console.ReadLine() ?? string.Empty;
        try
        {
            Log.Information("Validating URL...");
            var newURI = new Uri(newURIString);
            Log.Information("Looks good; validating image... (will not check if the image itself is correct!)");
            using (Image img = Image.Load(await newURI.WithImageHeaders().GetStreamAsync()))
            {
                Log.Information("Image can be loaded. Is the ratio reasonable?");
                var aspectRatio = (img.Width * 1.0d) / img.Height;
                var flooredAspectRatio = Math.Floor(aspectRatio * 100);
                if (flooredAspectRatio < 67 || flooredAspectRatio > 72)
                {
                    Log.Information("Image Ratio ({aspectRatio}) isn't correct (it must be approx. 0.71428571428); Failed inspection.", aspectRatio);
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
