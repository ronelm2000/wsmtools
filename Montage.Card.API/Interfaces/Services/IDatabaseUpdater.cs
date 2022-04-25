using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Montage.Card.API.Interfaces.Services
{
    public interface IDatabaseUpdater<ICDB, C> where ICDB : IDisposable, ICardDatabase<C> where C : ICard
    {
        public Task Update(ICDB database, IActivityLogTranslator translator, DatabaseUpdateArgs? args = default);
    }

    public class DatabaseUpdateArgs
    {
        public bool DisplayLogOverride { get; set; } = false;
        public IProgress<DatabaseUpdateReport> Progress { get; set; } = NoOpProgress<DatabaseUpdateReport>.Instance;
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
    }
}
