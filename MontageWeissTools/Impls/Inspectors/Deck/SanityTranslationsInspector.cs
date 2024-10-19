using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Parsers.Cards;
using Montage.Weiss.Tools.Utilities;

namespace Montage.Weiss.Tools.Impls.Inspectors.Deck;

public class SanityTranslationsInspector : IExportedDeckInspector<WeissSchwarzDeck, WeissSchwarzCard>
{
    public readonly ILogger Log; // = Serilog.Log.ForContext<SanityTranslationsInspector>();

    public int Priority => 0;

    public Func<InspectionOptions, Task<bool>> Prompter { get; set; } = static async (options) => await ValueTask.FromResult(ConsoleUtils.Prompted(options.IsNonInteractive, options.NoWarning));

    public SanityTranslationsInspector (ILogger log)
    {
        Log = log.ForContext<SanityTranslationsInspector>();
    }

    public async Task<WeissSchwarzDeck> Inspect(WeissSchwarzDeck deck, InspectionOptions options)
    {
        Log.Information("Starting.");
        var allEmptyTranslations = deck.Ratios.Keys.Where(card => String.IsNullOrWhiteSpace(card.Name.EN) && EncoreDecksParser.HasNoTranslations(card))
                                                    .Select(card => card.ReleaseID)
                                                    .Distinct();
        var setsWithTranslations = deck.Ratios.Keys.Where(card => !String.IsNullOrWhiteSpace(card.Name.EN))
                                                    .Select(card => card.ReleaseID)
                                                    .Distinct()
                                                    .ToList();
        var allNonTranslatedCards = deck.Ratios.Keys.Where(card => String.IsNullOrWhiteSpace(card.Name.EN) && EncoreDecksParser.HasNoTranslations(card))
                                        .Distinct()
                                        .ToList();
        
        if (allEmptyTranslations.Any())
        {
            Log.Warning("The following sets (based on Release ID) do not seem to have proper English translations: {allEmptyTranslations}", allEmptyTranslations.ToList());
            Log.Warning("Cards with missing EN Translations: {allNonTranslatedCards}", allNonTranslatedCards.Select(card => card.Serial).ToList());
            if (allNonTranslatedCards.Any(c => c.Rarity == "PR"))
            {
                Log.Warning("If you suspect that these are untranslated PRs, you may also go back and manually export PRs using any of the ff. where applicable and retry:\n" +
                    "\t./wstools parse https://www.heartofthecards.com/translations/schwarz_promos.html\n" +
                    "\t./wstools parse https://www.heartofthecards.com/translations/weiss_promos.html");
            }
            Log.Warning("This may result in a deck generator with only Japanese text.");
            Log.Warning("Do you wish to continue? [Y/N] (Default is N)");
            if (await Prompter(options))
                return deck;
            else
            {
                Log.Information("Operation cancelled.");
                Log.Information("If you need to add card data from other sources, use this command: {command}", "wstools parse link_url");
                Log.Information("For more information, please see: {url}", new Uri("https://github.com/ronelm2000/wsmtools"));
                return WeissSchwarzDeck.Empty;
            }
        } else
        {
            await Task.CompletedTask; //placebo way to stop warning about async/await
            return deck;
        }
    }
}
