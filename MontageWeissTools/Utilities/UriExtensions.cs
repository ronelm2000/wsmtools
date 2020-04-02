using AngleSharp;
using AngleSharp.Dom;
using Flurl.Http;
using Serilog;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Utilities
{
    public static class UriExtensions
    {
        static readonly HttpClient client = new HttpClient();

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
                return await context.OpenAsync(req => req.Content(content));
            }
        }

        public static IFlurlRequest WithRESTHeaders(this string urlString)
        {
            return urlString.WithHeader("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36")
                            .WithHeader("Accept", "text/plain");
        }

        public static IFlurlRequest WithHTMLHeaders(this string urlString)
        {
            return urlString.WithHeaders(new 
            {
                User_Agent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36",
                Accept = "*/*",
                Accept_Language = "en-US,en;q=0.8"
            });
        }

        public static IFlurlRequest WithImageHeaders(this Uri url)
        {
            return url.AbsoluteUri.WithHeaders(new
            {
                User_Agent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36",
                Accept = "*/*",
                Referer = url.Authority
            });
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

    }
}
