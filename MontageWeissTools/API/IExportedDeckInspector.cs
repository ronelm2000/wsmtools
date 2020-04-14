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
        /// Gets the Priority of a particular issue.
        /// This value must be 0 except for the following conditions:
        /// <list type="bullet">
        /// <item>You have an inspector that needs to be done before a particular inspector.</item>
        /// <item>You have an inspector that needs to be  done only after a particular inspector.</item>
        /// </list>
        /// </summary>
        public int Priority { get; }

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
