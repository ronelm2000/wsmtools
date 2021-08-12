using CommandLine;
using Fluent.IO;
using Flurl.Http;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Utilities;
using Montage.Weiss.Tools.Utilities;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.CLI
{
    [Verb("cache", HelpText = "Downloads all related images and updates it into a file; also edits the image metadata to show proper attribution.")]
    public class CacheVerb : IVerbCommand
    {
        private static readonly string _IMAGE_CACHE_PATH = "./Images/";
        private ILogger Log = Serilog.Log.ForContext<CacheVerb>();

        [Value(0, HelpText = "Indicates either Release ID or a full Serial ID.")]
        public string ReleaseIDorFullSerialID { get; set; }

        [Value(1, HelpText = "Indicates the Language. This is only applicable if you indicated the Release ID.", Default = "")]
        public string Language { get; set; } = "";

        public async Task Run(IContainer ioc)
        {
            Log.Information("Starting.");
            var language = InterpretLanguage(Language);
            IAsyncEnumerable<WeissSchwarzCard> list = null;

            Func<Flurl.Url, CookieSession> _cookieSession = (url) => ioc.GetInstance<GlobalCookieJar>()[url.Root];


            using (var db = ioc.GetInstance<CardDatabaseContext>())
            {
                await db.Database.MigrateAsync();
                if (language == null)
                {
                    try
                    {
                        var tuple = WeissSchwarzCard.ParseSerial(ReleaseIDorFullSerialID);
                    }
                    catch (Exception)
                    {
                        Log.Error("Serial cannot be parsed properly. Did you mean to cache a release set? If so, please indicate the language (EN/JP) as well.");
                        return;
                    }

                    var query = from card in db.WeissSchwarzCards.AsQueryable()
                                where card.Serial.ToLower() == ReleaseIDorFullSerialID.ToLower()
                                select card;
                    list = query.ToAsyncEnumerable().Take(1);
                } 
                else
                {
                    var releaseID = ReleaseIDorFullSerialID.ToLower().Replace("%","");
                    var query = from card in db.WeissSchwarzCards.AsQueryable()
                                where EF.Functions.Like(card.Serial.ToLower(), $"%/{releaseID}%")
                                select card;
                    list = query.ToAsyncEnumerable().Where(c => c.Language == language.Value);
                }

                await foreach (var card in list)
                    await AddCachedImageAsync(card, _cookieSession);

                Log.Information("Done.");
                Log.Information("PS: Please refrain from executing this command continuously as this may cause your IP address to get tagged as a DDoS bot.");
                Log.Information("    Only cache the images you may need.");
                Log.Information("    -ronelm2000");

            }
        }

        private async Task AddCachedImageAsync(WeissSchwarzCard card, Func<Flurl.Url, CookieSession> _cookieSession)
        {
            try
            {
                var imgURL = card.Images.Last();
                Log.Information("Caching: {imgURL}", imgURL);
                var session = _cookieSession(imgURL);
                using (System.IO.Stream netStream = await card.GetImageStreamAsync(session))
                using (Image img = Image.Load(netStream))
                {
                    var imageDirectoryPath = Path.Get(_IMAGE_CACHE_PATH);
                    if (!imageDirectoryPath.Exists) imageDirectoryPath.CreateDirectory();
                    if (img.Height < img.Width)
                    {
                        Log.Debug("Image is probably incorrectly oriented, rotating it 90 degs. clockwise to compensate.");
                        img.Mutate(ipc => ipc.Rotate(90));
                    }
                    img.Metadata.ExifProfile ??= new ExifProfile();
                    img.Metadata.ExifProfile.SetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.Copyright, card.Images.Last().Authority);
                    var savePath = Path.Get(_IMAGE_CACHE_PATH).Combine($"{card.Serial.Replace('-', '_').AsFileNameFriendly()}.jpg");
                    savePath.Open(img.SaveAsJpeg);
                }
            } catch (InvalidOperationException e) when (e.Message == "Sequence contains no elements")
            {
                Log.Warning("Cannot be cached as no image URLs were found: {serial}", card.Serial);
            }
        }

        private CardLanguage? InterpretLanguage(string language)
        {
            return language switch
            {
                var l when (l.ToLower() == "en") => CardLanguage.English,
                var l when (l.ToLower() == "jp") => CardLanguage.Japanese,
                _ => null // meaning any
            };
        }
    }
}
