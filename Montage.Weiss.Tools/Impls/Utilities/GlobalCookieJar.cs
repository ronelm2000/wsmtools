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

    public async Task<CookieJar> FindOrCreate(string url, CancellationToken cancel = default)
    {
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            Log.Debug("Detected malformed URL: {url}, try to use prepend https:// instead.", url);
            var fixedUrl = $"https://{url}";
            if (Uri.IsWellFormedUriString(fixedUrl, UriKind.Absolute))
                url = fixedUrl;
            else
                throw new ArgumentException($"The URL '{url}' is not well-formed.");
        }

        if (sessions.TryGetValue(url, out var existing))
            return existing;

        await url.WithRESTHeaders().WithCookies(out var cookieJar).GetHTMLAsync(cancel);
        if (sessions.TryAdd(url, cookieJar))
            return cookieJar;
        else
            return new CookieJar();
    }
}
