using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Card.API.Entities.Impls;
public record DeckMergeProgressReport : UpdateProgressReport
{
    public static DeckMergeProgressReport Starting()
     => new DeckMergeProgressReport
        {
            Percentage = 0,
            ReportMessage = new MultiLanguageString
            {
                EN = "Starting..."
            }
        };
}
