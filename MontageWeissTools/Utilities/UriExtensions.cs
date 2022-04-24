using AngleSharp;
using AngleSharp.Dom;
using Flurl.Http;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Montage.Weiss.Tools.Utilities;

public static class UriExtensions
{
    static readonly HttpClient client = new HttpClient();
    static readonly IFlurlClient customizedClient = new FlurlClient(new HttpClient(new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => LogErrorButContinue(msg,cert,chain,errors)
            }
            )
        );

    private static bool LogErrorButContinue(HttpRequestMessage msg, X509Certificate2 cert, X509Chain chain, SslPolicyErrors errors)
    {
        if (errors != SslPolicyErrors.None)
            if (cert.Thumbprint == "cf0b1d5c188a542271330ad489d9c7bde9a8abd0")
            {
                Log.Warning("There is an known error certifcate error with www.encoredecks.com. This is ignored temporarily as I contact the developer.");
                return true;
            } else
            {
                return false;
            }
        else
            return true;
    }

    public static Task<IDocument> DownloadHTML(this Uri uri, params (string Key, string Value)[] keyValuePairs)
        => DownloadHTML(uri, CancellationToken.None, keyValuePairs);

    public static async Task<IDocument> DownloadHTML(this Uri uri, CancellationToken cancellationToken = default)
    {
        var content = await uri.WithClient(customizedClient)
            .WithHeaders(new
            {
                User_Agent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36",
                X_Requested_With = "XMLHttpRequest",
                Accept = "*/*",
                Accept_Language = "en-US,en;q=0.8"
            })
            .GetStringAsync(cancellationToken: cancellationToken)
            ;
        var config = Configuration.Default.WithDefaultLoader()
                .WithCss()
                ;
        var context = BrowsingContext.New(config);
        return await context.OpenAsync(req =>
        {
            req.Content(content);
            req.Address(uri);
        }, cancellationToken);        
    }

    public static async Task<IDocument> DownloadHTML(this Uri uri, CancellationToken cancel, params (string Key, string Value)[] keyValuePairs)
    {
        var req = uri.WithClient(customizedClient)
            .WithHeaders(new
            {
                User_Agent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36",
                X_Requested_With = "XMLHttpRequest",
                Accept = "*/*",
                Accept_Language = "en-US,en;q=0.8"
            });

        foreach (var kvp in keyValuePairs)
            req = req.WithHeader(kvp.Key, kvp.Value);

        var content = await req.GetStringAsync(cancellationToken: cancel);
        var config = Configuration.Default.WithDefaultLoader()
                .WithCss()
                ;
        var context = BrowsingContext.New(config);
        return await context.OpenAsync(req =>
            {
                req.Content(content);
                req.Address(uri);
            }, cancel: cancel);   
    }

    public static IFlurlRequest WithReferrer(this IFlurlRequest request, string referrerUrl) => request.WithHeader("Referer", referrerUrl);
    public static IFlurlRequest WithReferrer(this string urlString, string referrerUrl) => new FlurlRequest(urlString).WithReferrer(referrerUrl);
    public static IFlurlRequest WithReferrer(this Flurl.Url url, string referrerUrl) => new FlurlRequest(url).WithReferrer(referrerUrl);
    public static IFlurlRequest WithRESTHeaders(this IFlurlRequest request)
    {
        return request.WithClient(customizedClient)
                        .WithHeader("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36")
                        .WithHeader("Accept", "*/*")
                        .WithHeader("Accept-Encoding", "gzip, deflate, br")
                        .WithHeader("Referer", request.Url.Authority);
    }
    public static IFlurlRequest WithRESTHeaders(this string urlString) => new FlurlRequest(urlString).WithRESTHeaders();
    public static IFlurlRequest WithRESTHeaders(this Uri url) => new FlurlRequest(url).WithRESTHeaders();
    public static IFlurlRequest WithRESTHeaders(this Flurl.Url url) => new FlurlRequest(url).WithRESTHeaders();

    public static IFlurlRequest WithHTMLHeaders(this IFlurlRequest req)
    {
        return req.WithHeaders(new
        {
            User_Agent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36",
            Accept = "*/*",
            Accept_Language = "en-US,en;q=0.8"
        });
    }
    public static IFlurlRequest WithHTMLHeaders(this string urlString) => WithHTMLHeaders(new FlurlRequest(urlString));

    public static IFlurlRequest WithImageHeaders(this Uri url)
    {
        return url.AbsoluteUri.WithTimeout(TimeSpan.FromMinutes(10)) //
            .WithHeaders(new
            {
                User_Agent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36",
                Accept = "*/*",
                Host = url.Authority
                /*,
                Referer = url.Authority
                */
            })
            .WithHeader("Accept-Encoding", "gzip, deflate, br") //
            .WithHeader("Postman-Token", System.Guid.NewGuid().ToString()) //
            .WithHeader("Connection", "keep-alive");
        //.WithHeader("Host", url.Authority) //
    }

    public static async Task<IDocument> GetHTMLAsync(this IFlurlRequest flurlReq, CancellationToken cancel = default)
    {
        //var content = wc.DownloadString(uri);
        var config = Configuration.Default.WithDefaultLoader()
                .WithCss()
                ;
        var context = BrowsingContext.New(config);
        var stream = await flurlReq.GetStreamAsync(cancel);
        return await context.OpenAsync(req =>
        {
            req.Address(flurlReq.Url.ToString());
            req.Content(stream, true);
        }, cancel: cancel);
    }

    public static async Task<IDocument> RecieveHTML(this Task<IFlurlResponse> flurlResponse, CancellationToken cancel = default)
    {
        var config = Configuration.Default.WithDefaultLoader()
                .WithCss()
                ;
        var context = BrowsingContext.New(config);
        var response = await flurlResponse;
        var url = response.ResponseMessage.RequestMessage.RequestUri.AbsoluteUri;
        var stream = await response.GetStreamAsync(); //.ReceiveStream();
        return await context.OpenAsync(req =>
        {
            req.Address(url);
            req.Content(stream, true);
        }, cancel: cancel);
    }
}
