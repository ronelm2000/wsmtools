using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Montage.Card.API.Entities.Impls;

namespace Montage.Card.API.Entities;

public interface ICardDatabase<C> where C : ICard
{
    DatabaseFacade Database { get; }
    DbSet<ActivityLog> MigrationLog { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
