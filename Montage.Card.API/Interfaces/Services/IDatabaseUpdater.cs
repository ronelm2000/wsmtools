using Montage.Card.API.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Card.API.Interfaces.Services
{
    public interface IDatabaseUpdater<ICDB, C> where ICDB : IDisposable, ICardDatabase<C> where C : ICard
    {
        public Task Update(ICDB database, IActivityLogTranslator translator);
    }
}
