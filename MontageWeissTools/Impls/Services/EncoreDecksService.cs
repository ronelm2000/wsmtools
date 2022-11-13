using Flurl.Http;
using Montage.Weiss.Tools.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.Services;

public class EncoreDecksService
{
    private static readonly EncoreDeckAPI defaultEDAPI = new EncoreDeckAPI();

    public async Task<List<EncoreDeckSetListEntry>> GetSetListEntries(CancellationToken cancellationToken)
    {
        return await defaultEDAPI.SetList
            .WithRESTHeaders()
            .GetJsonAsync<List<EncoreDeckSetListEntry>>(cancellationToken);
    }

    public string GetCardListURI(EncoreDeckSetListEntry result) => defaultEDAPI.SetCards(result.ID);
}

public class EncoreDeckAPI
{
    public string Deck => "https://www.encoredecks.com/api/deck";
    public string SetCards(string setID) => $"https://www.encoredecks.com/api/series/{setID}/cards";
    public string SetList => "https://www.encoredecks.com/api/serieslist";
}

public readonly record struct EncoreDeckSetListEntry {
    public EncoreDeckSetListEntry()
    {
    }

    [Newtonsoft.Json.JsonProperty("_id")]
    public string ID { get; init; } = default!;
    public bool Enabled { get; init; } = true;
    public string Set { get; init; } = default!;
    public string Side { get; init; } = default!;
    public string Release { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string Lang { get; init; } = default!;
    public string Hash { get; init; } = default!;

    internal bool HasMatch(string searchTerm)
    {
        return $"{Side}{Release}".Contains(searchTerm) || Set.Contains(searchTerm);
    }
}
