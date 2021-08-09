using Fluent.IO;
using Montage.Card.API.Entities;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.Exporters
{
    /// <summary>
    /// A Deck Exporter whose output is purely a JSON file that coincides with the format of WeissSchwarzDeck, exactly.
    /// </summary>
    public class LocalDeckJSONExporter : IDeckExporter
    {
        private ILogger Log = Serilog.Log.ForContext<LocalDeckJSONExporter>();
        private JsonSerializerOptions _defaultOptions = new JsonSerializerOptions()
        {
            WriteIndented = true
        };

        public string[] Alias => new[]{ "local", "json" };

        public async Task Export(WeissSchwarzDeck deck, IExportInfo info)
        {
            Log.Information("Exporting as Deck JSON.");
            var jsonFilename = Path.CreateDirectory(info.Destination).Combine($"deck_{deck.Name.AsFileNameFriendly()}.json");
            var simplifiedDeck = new
            {
                Name = deck.Name,
                Remarks = deck.Remarks,
                Ratios = deck.AsSimpleDictionary()
            };
                
            jsonFilename.Open(
                async s => await JsonSerializer.SerializeAsync(s, simplifiedDeck, options: _defaultOptions),
                System.IO.FileMode.Create, 
                System.IO.FileAccess.Write, 
                System.IO.FileShare.ReadWrite
            );
            Log.Information($"Done: {jsonFilename.FullPath}");

            if (!String.IsNullOrWhiteSpace(info.OutCommand))
                await ExecuteCommandAsync(info.OutCommand, jsonFilename);
        }

        private async Task ExecuteCommandAsync(string outCommand, Path jsonFilename)
        {
            ConsoleUtils.RunExecutable(outCommand, $"\"{jsonFilename.FullPath}\"");
            await Task.CompletedTask;
        }
    }
}
