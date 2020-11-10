using AngleSharp;
using AngleSharp.Dom;
using Flurl.Http;
using Serilog;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Utilities
{
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
                    Log.Warning("There is an known error certifcate error with www.encoredecks.com. This is ignored tempoaririly as I contact the developer.");
                    return true;
                } else
                {
                    return false;
                }
            else
                return true;
        }

        public static async Task<IDocument> DownloadHTML(this Uri uri)
        {
            //HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(uri);
            //var response = myReq.GetResponse();
            //Log.Information("Response Content Length: {ContentLength}", response.ContentLength);
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                var values = new NameValueCollection();
                //values.Add("Referer", "http://www.nseindia.com/products/content/equities/equities/bulk.htm");
                values.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36");
                values.Add("X-Requested-With", "XMLHttpRequest");
                values.Add("Accept", "*/*");
                values.Add("Accept-Language", "en-US,en;q=0.8");
                wc.Headers.Add(values);

                var content = wc.DownloadString(uri);

                var config = Configuration.Default.WithDefaultLoader()
                        .WithCss()
                        ;
                var context = BrowsingContext.New(config);
                return await context.OpenAsync(req => req.Content(content));
            }
        }

        public static async Task<IDocument> DownloadHTML(this Uri uri, params (string Key, string Value)[] keyValuePairs)
        {
            //HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(uri);
            //var response = myReq.GetResponse();
            //Log.Information("Response Content Length: {ContentLength}", response.ContentLength);
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;

                var values = new NameValueCollection();
                //values.Add("Referer", "http://www.nseindia.com/products/content/equities/equities/bulk.htm");
                values.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36");
                values.Add("X-Requested-With", "XMLHttpRequest");
                values.Add("Accept", "*/*");
                values.Add("Accept-Language", "en-US,en;q=0.8");
                foreach (var kvp in keyValuePairs)
                {
                    values.Remove(kvp.Key);
                    values.Add(kvp.Key, kvp.Value);
                }
                wc.Headers.Add(values);

                var content = wc.DownloadString(uri);
                var config = Configuration.Default.WithDefaultLoader()
                        .WithCss()
                        ;
                var context = BrowsingContext.New(config);
                return await context.OpenAsync(req =>
                {
                    req.Content(content);
                    req.Address(uri);
                });
            }
        }

        public static IFlurlRequest WithReferrer(this string urlString, string referrerUrl)
        {
            return urlString.AllowAnyHttpStatus().WithReferrer(referrerUrl);
        }

        public static IFlurlRequest WithReferrer(this Flurl.Url urlString, string referrerUrl)
        {
            return urlString.AllowAnyHttpStatus().WithReferrer(referrerUrl);
        }

        public static IFlurlRequest WithReferrer(this IFlurlRequest request, string referrerUrl)
        {
            return request.WithHeader("Referer", referrerUrl);
        }



        public static IFlurlRequest WithRESTHeaders(this IFlurlRequest request)
        {
            return request  .WithClient(customizedClient)
                            .WithHeader("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36")
                            .WithHeader("Accept", "text/plain");
        }
        public static IFlurlRequest WithRESTHeaders(this string urlString) => new FlurlRequest(urlString).WithRESTHeaders();
        public static IFlurlRequest WithRESTHeaders(this Uri url) => new FlurlRequest(url).WithRESTHeaders();
        public static IFlurlRequest WithRESTHeaders(this Flurl.Url url) => new FlurlRequest(url).WithRESTHeaders();


        public static IFlurlRequest WithHTMLHeaders(this string urlString)
        {
            return urlString.WithHeaders(new
            {
                User_Agent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36",
                Accept = "*/*",
                Accept_Language = "en-US,en;q=0.8"
            });
        }

        public static IFlurlRequest WithHTMLHeaders(this IFlurlRequest req)
        {
            return req.WithHeaders(new
            {
                User_Agent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36",
                Accept = "*/*",
                Accept_Language = "en-US,en;q=0.8"
            });
        }

        public static IFlurlRequest WithImageHeaders(this Uri url)
        {
            return url.AbsoluteUri.WithTimeout(TimeSpan.FromMinutes(10)) //
                .WithHeaders(new
                {
                    User_Agent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36",
                    Accept = "*/*",
                    Referer = url.Authority,
                })
                .WithHeader("Accept-Encoding", "gzip, deflate, br");
                //.WithHeader("Host", url.Authority) //
                //.WithHeader("Connection", "keep-alive");
        }

        public static async Task<IDocument> GetHTMLAsync(this IFlurlRequest flurlReq)
        {
            //var content = wc.DownloadString(uri);
            var config = Configuration.Default.WithDefaultLoader()
                    .WithCss()
                    ;
            var context = BrowsingContext.New(config);
            var stream = await flurlReq.GetStreamAsync();
            return await context.OpenAsync(req =>
            {
                req.Address(flurlReq.Url.ToString());
                req.Content(stream, true);
            });

        }

        public static async Task<IDocument> RecieveHTML(this Task<HttpResponseMessage> flurlReq)
        {
            var config = Configuration.Default.WithDefaultLoader()
                    .WithCss()
                    //.With(I)
                    ;
            var context = BrowsingContext.New(config);
            var resReq = await flurlReq;
            var stream = await resReq.Content.ReadAsStreamAsync(); //.ReceiveStream();
            return await context.OpenAsync(req =>
            {
                req.Address(resReq.RequestMessage.RequestUri.AbsoluteUri);
                req.Content(stream, true);
            });
        }

    }
}
