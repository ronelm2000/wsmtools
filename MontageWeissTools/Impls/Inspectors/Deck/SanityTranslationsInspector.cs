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

        public async Task<WeissSchwarzDeck> Inspect(WeissSchwarzDeck deck, bool isNonInteractive)
        {
            var allEmptyTranslations = deck.Ratios.Keys.Where(card => String.IsNullOrWhiteSpace(card.Name.EN))
                                                        .Select(card => card.ReleaseID)
                                                        .Distinct();

            if (allEmptyTranslations.Any())
            {
                Log.Warning("The following sets (based on Release ID) do not have proper English translations: {allEmptyTranslations}", allEmptyTranslations.ToList());
                Log.Warning("This may result in a deck generator with only Japanese text.");
                Log.Warning("Do you wish to continue? [Y/N] (Default is N)");
                if (ConsoleUtils.Prompted(isNonInteractive))
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
}
