using CommandLine;
using Fluent.IO;
using Flurl.Http;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;

namespace Montage.Weiss.Tools.CLI;

[Verb("cache", HelpText = "Downloads all related images and updates it into a file; also edits the image metadata to show proper attribution.")]
public class CacheVerb : IVerbCommand
{
    private ILogger Log = Serilog.Log.ForContext<CacheVerb>();
    private static readonly string _IMAGE_CACHE_PATH = $"{AppDomain.CurrentDomain.BaseDirectory}/Images/";

    [Value(0, HelpText = "Indicates either Release ID or a full Serial ID.")]
    public string ReleaseIDorFullSerialID { get; set; } = string.Empty;

    [Value(1, HelpText = "Indicates the Language. This is only applicable if you indicated the Release ID.", Default = "")]
    public string Language { get; set; } = "";

    public async Task Run(IContainer ioc, IProgress<CommandProgressReport> progress, CancellationToken cancellationToken = default)
    {
        Log.Information("Starting.");
        var report = CommandProgressReport.Starting(CommandProgressReportVerbType.Caching);
        progress.Report(report);

        var language = InterpretLanguage(Language);

        using (var db = ioc.GetInstance<CardDatabaseContext>())
        {
            await db.Database.MigrateAsync(cancellationToken);
            var list = GenerateQuery(db, language);

            await Cache(ioc, progress, list, cancellationToken);

            Log.Information("Done.");
            Log.Information("PS: Please refrain from executing this command continuously as this may cause your IP address to get tagged as a DDoS bot.");
            Log.Information("    Only cache the images you may need.");
            Log.Information("    -ronelm2000");
        }

        report = report.AsDone(CommandProgressReportVerbType.Caching);
        progress.Report(report);
    }

    public async Task Cache(IContainer container, IProgress<CommandProgressReport> progress, IAsyncEnumerable<WeissSchwarzCard> list, CancellationToken cancellationToken = default)
    {
        Func<Flurl.Url, CookieSession> _cookieSession = (url) => container.GetInstance<GlobalCookieJar>()[url.Root];
        await foreach (var card in list.WithCancellation(cancellationToken))
        {
            var report = new CommandProgressReport
            {
                MessageType = MessageType.InProgress,
                ReportMessage = new Card.API.Entities.Impls.MultiLanguageString
                {
                    EN = $"Caching [{card.Serial}]..."
                },
                Percentage = 50
            };
            progress.Report(report);
            await AddCachedImageAsync(card, _cookieSession, cancellationToken);
        }
    }

    public async Task Cache(IContainer container, IProgress<CommandProgressReport> progress, WeissSchwarzCard card, CancellationToken cancellationToken = default)
    {
        Func<Flurl.Url, CookieSession> _cookieSession = (url) => container.GetInstance<GlobalCookieJar>()[url.Root];
        var report = new CommandProgressReport
            {
                MessageType = MessageType.InProgress,
                ReportMessage = new Card.API.Entities.Impls.MultiLanguageString
                {
                    EN = $"Caching [{card.Serial}]..."
                },
                Percentage = 50
            };
        progress.Report(report);
        await AddCachedImageAsync(card, _cookieSession, cancellationToken);
    }

    private IAsyncEnumerable<WeissSchwarzCard> GenerateQuery(CardDatabaseContext db, CardLanguage? language)
    {
        if (language == null)
        {
            try
            {
                var tuple = WeissSchwarzCard.ParseSerial(ReleaseIDorFullSerialID);
            }
            catch (Exception)
            {
                Log.Error("Serial cannot be parsed properly. Did you mean to cache a release set? If so, please indicate the language (EN/JP) as well.");
                return AsyncEnumerable.Empty<WeissSchwarzCard>();
            }

            var query = from card in db.WeissSchwarzCards.AsQueryable()
                        where card.Serial.ToLower() == ReleaseIDorFullSerialID.ToLower()
                        select card;
            return query.ToAsyncEnumerable().Take(1);
        }
        else
        {
            var releaseID = ReleaseIDorFullSerialID.ToLower().Replace("%", "");
            var query = from card in db.WeissSchwarzCards.AsQueryable()
                        where EF.Functions.Like(card.Serial.ToLower(), $"%/{releaseID}%")
                        select card;
            return query.ToAsyncEnumerable().Where(c => c.Language == language.Value);
        }
    }

    private async Task AddCachedImageAsync(WeissSchwarzCard card, Func<Flurl.Url, CookieSession> _cookieSession, CancellationToken ct = default)
    {
        try
        {
            if (card.EnglishSetType == EnglishSetType.Custom)
            {
                Log.Information("Skipping [{card}] (Custom Card Detected). Please manually put images into the Images folder.", card.Serial);
                return;
            }
            if (card.Images.Count < 1)
            {
                Log.Warning("Skipping [{card}]. Cannot find any images to cache.", card.Serial);
                return;
            }

            var imgURL = card.Images.Last();
            Log.Information("Caching: {imgURL}", imgURL);
            var session = _cookieSession(imgURL);
            using (System.IO.Stream netStream = await card.GetImageStreamAsync(session, ct))
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
                await img.SaveAsJpegAsync(savePath.FullPath, ct);
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
