using Montage.Card.API.Entities;
using Montage.Card.API.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Card.API.Interfaces.Services
{
    public interface IDeckParser<D, C> where D : IDeck<C> where C : ICard
    {
        public string[] Alias { get; }
        public int Priority { get; }
        public Task<bool> IsCompatible(string urlOrFile);
        public Task<D> Parse(string sourceUrlOrFile, IProgress<DeckParserProgressReport> progress, CancellationToken cancellationToken = default);

        [Obsolete]
        public Task<D> Parse(string sourceUrlOrFile)
            => Parse(sourceUrlOrFile, NoOpProgress<DeckParserProgressReport>.Instance);
    }
}
