using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Montage.Card.API.Entities.Impls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Montage.Card.API.Entities
{
    public interface ICardDatabase<C> where C : ICard
    {
        DatabaseFacade Database { get; }
        DbSet<ActivityLog> MigrationLog { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
