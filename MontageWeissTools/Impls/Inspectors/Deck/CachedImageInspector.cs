﻿using Fluent.IO;
using Montage.Card.API.Interfaces.Services;
using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using SixLabors.ImageSharp;

namespace Montage.Weiss.Tools.Impls.Inspectors.Deck;

public class CachedImageInspector : IExportedDeckInspector<WeissSchwarzDeck, WeissSchwarzCard>
{
    private readonly ILogger Log = Serilog.Log.ForContext<CachedImageInspector>();
    private readonly string _imageCachePath = "./Images/";

    public int Priority => 1;

    public async Task<WeissSchwarzDeck> Inspect(WeissSchwarzDeck deck, InspectionOptions options)
    {
        Log.Information("Starting...");
        var imageFolder = Path.Get(_imageCachePath);
        try
        {
            foreach (var card in deck.Ratios.Keys)
            {
                try
                {
                    var serialImage = imageFolder
                        .Files($"{card.Serial.Replace('-', '_').AsFileNameFriendly()}.*", true)
                        .WhereExtensionIs(".png", ".jpeg", ".jpg", "jfif")
                        .First();
                    var relativeFileName = _imageCachePath + serialImage.FileName;
                    serialImage = await InspectImage(serialImage, relativeFileName, options.CancellationToken);
                    if (serialImage != null)
                    {
                        Log.Information($"Using cached image: {serialImage}");
                        card.CachedImagePath = serialImage.FullPath;
                    }
                } catch (InvalidOperationException)
                {
                    // Do nothing.
                }
            }
        } catch (System.IO.DirectoryNotFoundException)
        {
            // Do nothing.
        }
        return deck;
    }

    private async Task<Path?> InspectImage(Path serialImage, string relativeFileName, CancellationToken ct)
    {
        try
        {
            Log.Debug($"Inspecting cache image candidate: {relativeFileName}");
            //Image fixedImage = null;
            var res = serialImage;
            await using System.IO.Stream s = serialImage.GetStream();
            using Image img = await Image.LoadAsync(s, ct);
            
            Log.Debug("Image can be loaded. Is the ratio reasonable?");

            var aspectRatio = (img.Width * 1.0d) / img.Height;
            var flooredAspectRatio = Math.Floor(aspectRatio * 100);
            if (flooredAspectRatio < 70 || flooredAspectRatio > 72)
            {
                Log.Warning("Image Ratio ({aspectRatio}) isn't correct (it must be approx. 0.71428571428); Not using cached image ({relativeFileName}).", aspectRatio, relativeFileName);
                return null;
            }

            if (img.Width < 400)
            {
                Log.Warning("The image is of low quality; Not using cached image ({relativeFileName}).", relativeFileName);
                return null;
            }               
            
            return serialImage;
        }
        catch (UnknownImageFormatException)
        {
            Log.Debug("The URL does not point to a valid image. Card Image Inspection failed ({relativeFileName}).", relativeFileName);
            return null;
        }
        catch (Exception e)
        {
            Log.Debug("Other reason occurred: {e}. Card Image Inspection Failed ({relativeFileName})", e, relativeFileName);
            return null;
        }
    }
    
}
