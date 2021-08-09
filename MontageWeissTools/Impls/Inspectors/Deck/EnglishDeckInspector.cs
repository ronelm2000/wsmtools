using Microsoft.EntityFrameworkCore.Internal;
using Montage.Card.API.Interfaces.Components;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Exporters.Deck;
using Montage.Weiss.Tools.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;

namespace Montage.Weiss.Tools.Impls.Inspectors.Deck
{
    public class EnglishDeckInspector : IExportedDeckInspector<WeissSchwarzDeck, WeissSchwarzCard>, IFilter<IDeckExporter<WeissSchwarzDeck, WeissSchwarzCard>>
    {
        private readonly ILogger Log = Serilog.Log.ForContext<SanityImageInspector>();

        public int Priority => 1;

        public async Task<WeissSchwarzDeck> Inspect(WeissSchwarzDeck deck, InspectionOptions options)
        {
            var englishEditionCards = deck.Ratios.Keys.Where(card => card.EnglishSetType == EnglishSetType.EnglishEdition);
            if (HeededWarningOnEnglishEditionSets(englishEditionCards, options)) return WeissSchwarzDeck.Empty;

            var japaneseImportCards = deck.Ratios.Keys.Where(card => card.EnglishSetType == EnglishSetType.JapaneseImport);
            if (HeededWarningOnJapaneseImports(japaneseImportCards, options)) return WeissSchwarzDeck.Empty;
            
            return await Task.FromResult(deck);
        }

        private bool HeededWarningOnEnglishEditionSets(IEnumerable<WeissSchwarzCard> englishEditionCards, InspectionOptions options)
        {
            var list = englishEditionCards.ToList();
            if (list.Count == 0) return false;

            Log.Warning("The following cards are English Edition cards. These may be unsupported by Cockatrice. Do you wish to continue? (Y/N)");
            Log.Warning("{list}", list.Select(c => c.Serial).ConcatAsString(","));
            if (ConsoleUtils.Prompted(options.IsNonInteractive, options.NoWarning))
                return false;
            else
            {
                Log.Information("Operation cancelled.");
                Log.Information("Please ensure that the deck uses Japanese cards or Japanese imported sets.");
                return true;
            }
        }
        private bool HeededWarningOnJapaneseImports(IEnumerable<WeissSchwarzCard> japaneseImportCards, InspectionOptions options)
        {
            var list = japaneseImportCards.ToList();
            if (list.Count == 0) return false;

            Log.Information("The following cards are English cards, but these will be instead translated to Japanese equivalents.");
            Log.Information("{list}", list.Select(c => c.Serial).ConcatAsString(","));
            if (options.IsNonInteractive) return false;

            Log.Information("Do you wish to continue? (Y/N)");
            if (ConsoleUtils.Prompted(options.IsNonInteractive, options.NoWarning))
                return false;
            else
            {
                Log.Information("Operation cancelled.");
                Log.Information("Please ensure that the deck uses Japanese cards.");
                return true;
            }
        }
        public bool IsIncluded(IDeckExporter<WeissSchwarzDeck, WeissSchwarzCard> item)
        {
            return item is CockatriceDeckExporter;
        }
    }
}
