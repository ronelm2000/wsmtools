using Microsoft.EntityFrameworkCore;
using Montage.Card.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Card.API.Utilities;
public static class DatabaseExtensions
{
    public static async Task AttachOrAddAsync<T,K>(this DbContext dbContext, IEnumerable<T> entities) where T : class, IIdentifiable<K> where K : struct
    {
        var existingEntities = entities.Where(e => !e.Id.Equals(T.Empty));
        dbContext.AttachRange(existingEntities);
        foreach (var entity in entities.Except(existingEntities))
        {
            entity.AssignNewID();
            await dbContext.AddAsync(entity);
        }
    }
}
