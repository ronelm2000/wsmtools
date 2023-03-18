using LinqKit;
using Montage.Card.API.Entities;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.Exporters.Database;

public abstract class CommonDatabaseExporter : IDatabaseExporter<CardDatabaseContext, WeissSchwarzCard>
{
    public abstract string[] Alias { get; }
    public abstract Task Export(CardDatabaseContext database, IDatabaseExportInfo info, CancellationToken cancellationToken);

    protected IQueryable<WeissSchwarzCard> CreateQuery(IQueryable<WeissSchwarzCard> query, IDatabaseExportInfo info)
    {
        var releaseIDLimitations = info.ReleaseIDs.ToList();
        var serialLimitations = info.Serials.ToList();
        var predicate = PredicateBuilder.New<WeissSchwarzCard>();

        foreach (var rid in releaseIDLimitations)
            predicate = predicate.Or(c => c.Serial.Contains(rid + "-"));

        foreach (var serial in serialLimitations)
            predicate = predicate.Or(c => c.Serial == serial);

        if (releaseIDLimitations.Count + serialLimitations.Count > 0)
            return query.Where(predicate);
        else
            return query;
    }
}
