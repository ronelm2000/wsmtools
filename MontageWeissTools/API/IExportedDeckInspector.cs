using Montage.Weiss.Tools.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.API
{
    public interface IExportedDeckInspector
    {
        /// <summary>
        /// Inspects via analysis if the deck can be exported at all. Implementatations of this class should never open new URLs, unless via direct
        /// user intervention, or if the parser command is used. Returns false if the inspection fails.
        /// </summary>
        /// <param name="deck"></param>
        /// <param name="isNonInteractive">Determines if the command itself should be non-interactive. Use this to avoid having to use
        /// Console.ReadKey if the command itself is meant to never interact with a user.</param>
        public Task<WeissSchwarzDeck> Inspect(WeissSchwarzDeck deck, bool isNonInteractive);
    }
}
