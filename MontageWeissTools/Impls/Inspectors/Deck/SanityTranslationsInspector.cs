using Microsoft.EntityFrameworkCore.Diagnostics;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.Inspectors.Deck
{
    public class SanityTranslationsInspector : IExportedDeckInspector
    {
        public readonly ILogger Log; // = Serilog.Log.ForContext<SanityTranslationsInspector>();

        public int Priority => 0;

        public SanityTranslationsInspector (ILogger log)
        {
            Log = log.ForContext<SanityTranslationsInspector>();
        }

        public async Task<WeissSchwarzDeck> Inspect(WeissSchwarzDeck deck, InspectionOptions options)
        {
            var allEmptyTranslations = deck.Ratios.Keys.Where(card => String.IsNullOrWhiteSpace(card.Name.EN))
                                                        .Select(card => card.ReleaseID)
                                                        .Distinct();
            var setsWithTranslations = deck.Ratios.Keys.Where(card => !String.IsNullOrWhiteSpace(card.Name.EN))
                                                        .Select(card => card.ReleaseID)
                                                        .Distinct()
                                                        .ToList();
            var allNonTranslatedCards = deck.Ratios.Keys.Where(card => String.IsNullOrWhiteSpace(card.Name.EN))
                                            //.Select(card => card.Serial)
                                            .Distinct()
                                            .ToList();

            if (allEmptyTranslations.Any())
            {
                if (setsWithTranslations.Any(rid => allEmptyTranslations.Contains(rid)))
                {
                    Log.Warning("The following cards have missing translations: {allNonTranslatedCards}", allNonTranslatedCards.Select(card => card.Serial).ToList());
                    Log.Warning("These are very likely untranslated PRs, please go back and manually export PRs using any of the ff. where applicable and retry:\n" +
                        "\t./wstools export https://www.heartofthecards.com/translations/schwarz_promos.html\n" +
                        "\t./wstools export https://www.heartofthecards.com/translations/weiss_promos.html");
                    Log.Warning("Note that to avoid too much bandwidth, images are not exported this way. You will need to put an image manually on the Images sub-folder. The process to improve this process is still in progress.");
                    return WeissSchwarzDeck.Empty;
                }
                else
                {
                    Log.Warning("The following sets (based on Release ID) do not have proper English translations: {allEmptyTranslations}", allEmptyTranslations.ToList());
                    Log.Warning("Cards with missing EN Translations: {allNonTranslatedCards}", allNonTranslatedCards.Select(card => card.Serial).ToList());
                    Log.Warning("This may result in a deck generator with only Japanese text.");
                    Log.Warning("Do you wish to continue? [Y/N] (Default is N)");
                    if (ConsoleUtils.Prompted(options.IsNonInteractive, options.NoWarning))
                        return deck;
                    else
                    {
                        Log.Information("Operation cancelled.");
                        Log.Information("If you need to add card data from other sources, use this command: {command}", "wstools parse link_url");
                        Log.Information("For more information, please see: {url}", new Uri("https://github.com/ronelm2000/wsmtools"));
                        return WeissSchwarzDeck.Empty;
                    }
                }
            } else
            {
                await Task.CompletedTask; //placebo way to stop warning about async/await
                return deck;
            }
        }
    }
}
