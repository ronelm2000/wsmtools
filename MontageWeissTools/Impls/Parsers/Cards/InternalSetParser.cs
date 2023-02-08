using AngleSharp.Common;
using Flurl.Http;
using System.IO;
using System.Text.Json;
using Montage.Weiss.Tools.Impls.Parsers.Cards;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;
using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using System.Runtime.CompilerServices;
using Montage.Weiss.Tools.Utilities;
using System.Linq;
using System.Text.Json.Serialization;

namespace Montage.Weiss.Tools.Impls.Parsers.Cards;

public class InternalSetParser : ICardSetParser<WeissSchwarzCard>
{
    private ILogger Log = Serilog.Log.ForContext<InternalSetParser>();
    //private JsonSerializer _jsonSerializer = new JsonSerializer();

    public async Task<bool> IsCompatible(IParseInfo info)
    {
        await ValueTask.CompletedTask;
        if (Uri.TryCreate(info.URI, UriKind.Absolute, out var uri))
        {
            return uri.LocalPath.EndsWith(".ws-set");
        }
        else
        {
            return Fluent.IO.Path.Get(info.URI).Extension == ".ws-set";
        }
    }

    public async IAsyncEnumerable<WeissSchwarzCard> Parse(string urlOrLocalFile, IProgress<SetParserProgressReport> progress, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Log.Information("Starting...");
        using (Stream s = await GetStreamFromURIOrFile(urlOrLocalFile))
        {
            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };
            var jsonObject = await JsonSerializer.DeserializeAsync<WeissSchwarzCard[]>(s, options: jsonSerializerOptions, cancellationToken: cancellationToken);
            var emptyArray = Array.Empty<WeissSchwarzCard>();
            foreach (var card in jsonObject?.DistinctBy(c => c.Serial) ?? emptyArray)
            {
                yield return card;
            }
        }
    }
    private static async Task<Stream> GetStreamFromURIOrFile(string urlOrLocalFile)
    {
        if (Uri.TryCreate(urlOrLocalFile, UriKind.Absolute, out var uri) && !uri.IsFile)
        {
            return await urlOrLocalFile.WithRESTHeaders().GetStreamAsync();
        }
        else
            return Fluent.IO.Path.Get(urlOrLocalFile).GetStream();
    }
}
