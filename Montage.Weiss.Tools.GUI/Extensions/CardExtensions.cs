using Avalonia.Media.Imaging;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.GUI.Extensions;
public static class CardExtensions
{
    public static async Task<Bitmap?> LoadImage(this WeissSchwarzCard card)
    {
        var cachedPath = card.GetCachedImagePath();
        if (cachedPath is null)
            return null;

        await using var stream = cachedPath.GetStream();
        return Bitmap.DecodeToWidth(stream, 200, BitmapInterpolationMode.LowQuality);
    }

}
