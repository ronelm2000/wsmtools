using Fluent.IO;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Montage.Card.API.Entities;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.Exporters.Database
{
    public class JSONExporter : CommonDatabaseExporter
    {
        private ILogger Log = Serilog.Log.ForContext<JSONExporter>();

        public override string[] Alias => new[] { "json", "json-db" }; 

        public override async Task Export(CardDatabaseContext database, IDatabaseExportInfo info, CancellationToken cancellationToken)
        {
            Log.Information("Starting...");
            var query = CreateQuery(database.WeissSchwarzCards, info);
            var destPath = Path.Get(info.Destination);
            if (!destPath.HasExtension)
                destPath = destPath.CreateDirectory().Combine("result.json");

            var jsonObject = await GenerateJSONAsync(query, cancellationToken);
            await using (var stream = destPath.GetOpenWriteStream())
            {
                await JsonSerializer.SerializeAsync(stream, jsonObject, cancellationToken: cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            Log.Information("Done.");
            Log.Information($"Saved: {destPath.ToString()}");
        }

        async Task<List<WeissSchwarzCard>> GenerateJSONAsync(IQueryable<WeissSchwarzCard> query, CancellationToken cancellationToken)
        {
            return await query.ToListAsync(cancellationToken);
        }
    }
}
