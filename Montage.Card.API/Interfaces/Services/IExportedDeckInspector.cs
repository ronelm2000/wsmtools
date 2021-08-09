using Montage.Card.API.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Card.API.Interfaces.Services
{
    public interface IExportedDeckInspector<D,C> where D : IDeck<C> where C : ICard
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
        /// <param name="deck">The input deck that may or may not have already been inspected by other inspectors.</param>
        /// <param name="options">
        /// Customized inspection options that may or may not be used by the inspector. Note that interactive inspectors must use this
        /// object.
        /// </param>
        public Task<D> Inspect(D deck, InspectionOptions options);
    }

    /// <summary>
    /// A dedicated parameter class for IExportedDeckInspector
    /// </summary>
    public class InspectionOptions
    {
        /// <summary>
        /// Determines if the inspector must act as if the console is non-interactive (meaning there are no user inputs)
        /// </summary>
        public bool IsNonInteractive { get; set; } = false;
        /// <summary>
        /// Determines if the inspector must act as if all warnings are ignored. This flag if true must override IsNonInteractive.
        /// </summary>
        public bool NoWarning { get; set; } = false;
        public static implicit operator InspectionOptions((bool IsNonInteractive, bool NoWarning) tuple) 
            => new InspectionOptions() { 
                IsNonInteractive = tuple.IsNonInteractive, 
                NoWarning = tuple.NoWarning 
            };
    }
}
