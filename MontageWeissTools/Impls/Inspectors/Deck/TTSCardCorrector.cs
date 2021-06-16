using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Exporters.Deck.TTS;
using Montage.Weiss.Tools.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.Inspectors.Deck
{
    public class TTSCardCorrector : IExportedDeckInspector, IFilter<IDeckExporter>
    {
        public int Priority => 0;

        public async Task<WeissSchwarzDeck> Inspect(WeissSchwarzDeck deck, InspectionOptions options)
        {
            foreach (var card in deck.Ratios.Keys.ToArray())
            {
                var newCard = card.Clone();
                newCard.Effect = card.Effect.Select(eff => FixBrokenCharacters(eff)).ToArray();
                deck.ReplaceCard(card, newCard);
            }
            await Task.CompletedTask;
            return deck;
        }

        private string FixBrokenCharacters(string effect)
        {
            return effect.ReplaceAll(
                ("【", "["),
                ("】", "]"),
                ("《", "<<"),
                ("》", ">>"),
                // These are shortcuts from HOTC
                ("[A]", "[AUTO]"),
                ("[C]", "[CONT]"),
                ("[S]", "[ACT]")
                );
        }

        public bool IsIncluded(IDeckExporter item)
        {
            return item is TTSDeckExporter;
        }
    }
}
