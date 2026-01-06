using Flurl.Http;
using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.Utilities;

namespace Montage.Weiss.Tools.Impls.Utilities;

/// <summary>
/// This class manages cookie sessions per site on a global level.
/// </summary>
public class GlobalCookieJar
{
    public Dictionary<string, CookieJar> sessions;

    public GlobalCookieJar()
    {
        sessions = new Dictionary<string, CookieJar>();
    }

    //public CookieJar this[string url] => (sessions.TryGetValue(url, out var res)) ?  res : sessions.Add<string,CookieJar>(url, new CookieJar()) ?? new CookieJar();

    public async Task<CookieJar> FindOrCreate(string url, CancellationToken cancel = default)
    {
        if (sessions.TryGetValue(url, out var existing))
            return existing;

        await url.WithRESTHeaders().WithCookies(out var cookieJar).GetHTMLAsync(cancel);
        if (sessions.TryAdd(url, cookieJar))
            return cookieJar;
        else
            return new CookieJar();
    }

}
