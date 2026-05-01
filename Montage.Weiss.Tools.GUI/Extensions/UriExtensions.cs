using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.GUI.Extensions;

public static class UriExtensions
{
    public static async Task<Bitmap?> Load(this Uri url)
    {
        if (url.Scheme == "avares")
            return new Bitmap(AssetLoader.Open(url));

        using var httpClient = new HttpClient();
        try
        {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsByteArrayAsync();
            return new Bitmap(new MemoryStream(data));
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"An error occurred while downloading image '{url}' : {ex.Message}");
            return null;
        }
    }
}
