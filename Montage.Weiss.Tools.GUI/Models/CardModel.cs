using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Avalonia.Platform;
using Montage.Card.API.Entities.Impls;
using System.Collections.Generic;
using System.Linq;

namespace Montage.Weiss.Tools.GUI.Models;
public class CardModel
{
    public Task<Bitmap?> Image { get; init; }
    public MultiLanguageString Name { get; init; }
    public List<MultiLanguageString> Traits { get; init; }
    public MultiLanguageString TraitView => Traits.Aggregate((a, b) => new MultiLanguageString { EN = string.Join(" - ", a.EN, b.EN), JP = string.Join(" - ", a.EN, b.EN) });

    public CardModel(Uri imagePath)
    {
        Image = Load(imagePath);
    }

    public static async Task<Bitmap?> Load(Uri url)
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
