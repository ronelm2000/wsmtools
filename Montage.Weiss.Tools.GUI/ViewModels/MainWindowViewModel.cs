using Montage.Card.API.Entities.Impls;
using System.Collections.ObjectModel;
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using Montage.Weiss.Tools.Impls.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.CLI;
using System.Data;
using System.Data.Entity;
using System.Linq;
using ReactiveUI;
using System.Reactive;
using Montage.Weiss.Tools.GUI.Extensions;
using Avalonia.Platform.Storage;
using Fluent.IO;
using System.Reactive.Linq;
using System.Threading;
using Montage.Weiss.Tools.Impls.Exporters.Deck;
using Montage.Weiss.Tools.Impls.Parsers.Deck;
using Avalonia.Controls;
using System.Text.RegularExpressions;
using Avalonia.Threading;
using JasperFx.Core;
using Montage.Weiss.Tools.GUI.Utilities;
using Montage.Weiss.Tools.GUI.Views;
using Montage.Weiss.Tools.GUI.ViewModels.Dialogs;

namespace Montage.Weiss.Tools.GUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private static readonly Regex searchRegex = ScryfallStyleRegex();

    private ILogger log;
    public Func<Window>? Parent { get; init; }
    public Lamar.IContainer? Container { get; init; }

    public ObservableCollection<CardEntryViewModel> DatabaseViewList { get; set; }

    public ObservableCollection<CardRatioViewModel> DeckRatioList { get; set; } = [];

    public ReactiveCommand<Unit, Unit> ImportSetCommand { get; init; }
    public ReactiveCommand<Unit, Unit> ImportDeckCommand { get; init; }
    public ReactiveCommand<Unit, Unit> OpenLocalSetCommand { get; init; }
    public ReactiveCommand<Unit, Unit> SaveDeckCommand { get; init; }
    public ReactiveCommand<Unit, Unit> OpenDeckCommand { get; init; }
//    public ReactiveCommand<Unit, CardEntryViewModel> AddCardCommand { get; init; }
    public ReactiveCommand<string, string> UpdateDatabaseViewCommand { get; init; }
    public ReactiveCommand<Unit, Unit> ExportDeckToTabletopCommand { get; init; }

    [ObservableProperty]
    private string _status;

    [ObservableProperty]
    private string _searchBarText;

    [ObservableProperty]
    private string _deckName;

    [ObservableProperty]
    private string _deckRemarks;

    [ObservableProperty]
    private string _deckStats;

    [ObservableProperty]
    public ImportSetViewModel _importSetDC;

    [ObservableProperty]
    public ImportDeckViewModel _importDeckDC;

    public MainWindowViewModel()
    {
        Status = "";
        DeckName = "";
        DeckRemarks = "";
        SearchBarText = "";
        DeckStats = "[ 0 / 0 / 0 / 0 ]";

        if (Design.IsDesignMode)
        {
            DatabaseViewList = new ObservableCollection<CardEntryViewModel>(
            [
                new CardEntryViewModel(
                new Uri("avares://wsm-gui/Assets/Samples/sample_card.jpg"),
                new MultiLanguageString { EN = "Sample 1", JP = "Sample 1 But JP" },
                [
                    new MultiLanguageString { EN = "AAAA", JP = "AAAA JP" },
                    new MultiLanguageString { EN = "BBBB", JP = "BBBB JP" }
                ]
             ),
            new CardEntryViewModel(
                new Uri("avares://wsm-gui/Assets/Samples/sample_card.jpg"),
                new MultiLanguageString { EN = "Sample 2", JP = "Sample 2 But JP" },
                [ new MultiLanguageString { EN = "AAAA", JP = "AAAA JP" } ]
            ),
            new CardEntryViewModel(
                new Uri("avares://wsm-gui/Assets/Samples/sample_card.jpg"),
                new MultiLanguageString { EN = "Sample 3", JP = "Sample 3 But JP" },
                [ new MultiLanguageString { EN = "AAAA", JP = "AAAA JP" } ]
            )
            ]);

            DeckRatioList.Add(new CardRatioViewModel());
            DeckRatioList.Add(new CardRatioViewModel() { Image = new Uri("avares://wsm-gui/Assets/Samples/sample_card.jpg").Load() });
            ImportSetDC = new ImportSetViewModel { IsVisible = false, Parent = () => null };
            ImportDeckDC = new ImportDeckViewModel { IsVisible = false, Parent = () => null };
        }
        else
        {
            DatabaseViewList = [];
            ImportSetDC = new ImportSetViewModel { IsVisible = false, Parent = () => this };
            ImportDeckDC = new ImportDeckViewModel { IsVisible = false, Parent = () => this };
        }

        log = Serilog.Log.Logger.ForContext<MainWindowViewModel>();

        ImportSetCommand = ReactiveCommand.CreateFromTask(ImportSet);
        ImportDeckCommand = ReactiveCommand.CreateFromTask(ImportDeck);
        OpenLocalSetCommand = ReactiveCommand.CreateFromTask(OpenLocalSet);
        SaveDeckCommand = ReactiveCommand.CreateFromTask(SaveDeck);
        OpenDeckCommand = ReactiveCommand.CreateFromTask(OpenDeck);
        UpdateDatabaseViewCommand = ReactiveCommand.CreateFromTask<string, string>(UpdateDatabaseView);
        ExportDeckToTabletopCommand = ReactiveCommand.CreateFromTask(ExportDeckToTabletop);


        this.WhenAnyValue(r => r.SearchBarText)
            .Throttle(TimeSpan.FromSeconds(1))
            .InvokeCommand(UpdateDatabaseViewCommand);
    }

    private async Task ExportDeckToTabletop(CancellationToken token)
    {
        var progressReporter = new ProgressReporter(log, message => Status = message);
        var deck = new WeissSchwarzDeck
        {
            Name = DeckName,
            Remarks = DeckRemarks,
            Ratios = DeckRatioList.ToDictionary(vw => vw.Card, vw => vw.Ratio)
        };

        var command = new ExportVerb
        {
            NonInteractive = true,
            Exporter = "tabletopsim",
            Flags = ["sendtcp", "limit-width(800)"],
            OutCommand = "sharex",
            Progress = progressReporter
        };
        await command.Run(Container!, deck);
    }

    private async Task SaveDeck(CancellationToken token)
    {
        var progressReporter = new ProgressReporter(log, message => Status = message);
        var storage = Parent!().StorageProvider;
        var savePath = await storage.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = "Saving Local Deck...",
            ShowOverwritePrompt = true,
            DefaultExtension = DeckFiles.Patterns?[0][2..] ?? "ws-dek",
            FileTypeChoices = [ DeckFiles ]
        });
        if (savePath is null)
            return;

        var deck = new WeissSchwarzDeck
        {
            Name = DeckName,
            Remarks = DeckRemarks,
            Ratios = DeckRatioList.ToDictionary(vw => vw.Card, vw => vw.Ratio)
        };

        var finalPath = Path.Get(savePath.Path.LocalPath);

        var localDeckExporter = Container!.GetService<LocalDeckJSONExporter>()!;
        await localDeckExporter.Export(deck, progressReporter, finalPath, token);
    }

    private async Task OpenDeck(CancellationToken token)
    {
        var progressReporter = new ProgressReporter(log, message => Status = message);
        var storage = Parent!().StorageProvider;
        var loadPaths = await storage.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Opening Local Deck...",
            FileTypeFilter = [DeckFiles]
        });
        var firstPath = loadPaths.FirstOrDefault();
        if (firstPath is null)
            return;

        var localDeckParser = Container!.GetService<LocalDeckJSONParser>()!;
        var deck = await localDeckParser.Parse(firstPath.Path.AbsolutePath, progressReporter, token);

        DeckName = deck.Name;
        DeckRemarks = deck.Remarks;
        
        DeckRatioList.Clear();
        foreach (var ratio in deck.Ratios)
            DeckRatioList.Add(new CardRatioViewModel(ratio.Key, ratio.Value));

        SortDeck();
        UpdateDeckStats();
    }

    private async Task<string> UpdateDatabaseView(string searchText, CancellationToken token)
    {
        if (searchText is null || string.IsNullOrWhiteSpace(searchText))
            return "";
        if (token.IsCancellationRequested)
            return searchText;

        var progressReporter = new ProgressReporter(log, message => Status = message);
        var searchTerms = searchRegex.Matches(searchText)
            .Select(x => TranslateMatch(x))
            .ToList();

        using var db = Container!.GetInstance<CardDatabaseContext>();
        var searchCardList = await db.WeissSchwarzCards
            .ToAsyncEnumerable()
            .Where(c => searchTerms.All(st => st.Invoke(c)))
            .Distinct(c => c.Serial)
            .Take(300)
            .ToListAsync(token);

        var cacheList = searchCardList.Where(c => c.GetCachedImagePath() is null && c.EnglishSetType != EnglishSetType.Custom).ToAsyncEnumerable();
        await new CacheVerb { }.Cache(Container, progressReporter, cacheList, token);

        if (token.IsCancellationRequested)
            return searchText;

        Log.Information("Refreshing Card List...");

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            DatabaseViewList.Clear();

            foreach (var card in searchCardList)
            {
                var cardView = new CardEntryViewModel(card);
                DatabaseViewList.Add(cardView);
            }
        });

        log.Information("All Serials: {ser}", DatabaseViewList.Select(v => v.Card.Serial).Distinct().Count());
        log.Information("All Cards: {ser}", DatabaseViewList.Count);

        Status = "Done";

        return searchText;

        Func<WeissSchwarzCard,bool> TranslateMatch(Match scryfallMatch)
        {
            if (scryfallMatch.Groups[4].Success)
                return c => c.Serial.Contains(scryfallMatch.Value) || (c.Name.EN?.Contains(scryfallMatch.Value) ?? false) || (c.Name.JP?.Contains(scryfallMatch.Value) ?? false);
            else if (scryfallMatch.Groups[1].Value.Equals("o", StringComparison.CurrentCultureIgnoreCase))
            {
                var value = scryfallMatch.Groups[2].Value;
                if (string.IsNullOrWhiteSpace(value))
                    value = scryfallMatch.Groups[3].Value;
                return c => c.Effect.Any(e => e.Contains(value));
            }
            else if (scryfallMatch.Groups[1].Value.Equals("t", StringComparison.CurrentCultureIgnoreCase))
            {
                var value = scryfallMatch.Groups[2].Value;
                if (string.IsNullOrWhiteSpace(value))
                    value = scryfallMatch.Groups[3].Value;
                var typeValue = (value.ToLower()) switch
                {
                    "climax" or "cx" => CardType.Climax,
                    "event" or "ev" => CardType.Event,
                    "character" or "chara" => CardType.Character,
                    _ => CardType.Character
                };
                return c => c.Type == typeValue;
            }
            else if (scryfallMatch.Groups[1].Value.Equals("c", StringComparison.CurrentCultureIgnoreCase))
            {
                var colors = scryfallMatch.Groups[2].Value.Split(',').Select(c => c.ToLower() switch
                    {
                        "y" or "yellow" => CardColor.Yellow,
                        "g" or "green" => CardColor.Green,
                        "r" or "red" => CardColor.Red,
                        "b" or "blue" => CardColor.Blue,
                        "p" or "purple" => CardColor.Purple,
                        _ => CardColor.Purple
                    })
                    .ToList();
                return c => colors.Contains(c.Color);
            }
            else if (scryfallMatch.Groups[1].Value.Equals("set", StringComparison.CurrentCultureIgnoreCase))
            {
                var majorNSCode = scryfallMatch.Groups[2].Value.ToUpper();
                return c => WeissSchwarzCard.ParseSerial(c.Serial).NeoStandardCode
                    .StartsWith(majorNSCode);
            }
            else if (scryfallMatch.Groups[1].Value.Equals("l", StringComparison.CurrentCultureIgnoreCase) ||
                     scryfallMatch.Groups[1].Value.Equals("level", StringComparison.CurrentCultureIgnoreCase))
            {
                var levelString = Strings.Or(() => scryfallMatch.Groups[2].Value, () => scryfallMatch.Groups[3].Value);
                var level = Parsers.ParseInt(levelString.AsSpan(), style: System.Globalization.NumberStyles.Number);
                if (level is not null)
                    return c => (c.Level ?? 0) == level!;
                else
                    return c => true;
            }
            else if (scryfallMatch.Groups[1].Value.Equals("co", StringComparison.CurrentCultureIgnoreCase) ||
                     scryfallMatch.Groups[1].Value.Equals("cost", StringComparison.CurrentCultureIgnoreCase))
            {
                var costString = Strings.Or(() => scryfallMatch.Groups[2].Value, () => scryfallMatch.Groups[3].Value);
                var cost = Parsers.ParseInt(costString.AsSpan(), style: System.Globalization.NumberStyles.Number);
                if (cost is not null)
                    return c => (c.Cost ?? 0) == cost;
                else
                    return c => true;
            }
            else
            {
                return c => c.Serial.Contains(scryfallMatch.Value) || (c.Name.EN?.Contains(scryfallMatch.Value) ?? false) || (c.Name.JP?.Contains(scryfallMatch.Value) ?? false);
            }
        }
    }

    private async Task OpenLocalSet()
    {
        var progressReporter = new ProgressReporter(log, message => Status = message);
        var storage = Parent!().StorageProvider;
        var localPaths = await storage.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Opening Local Set...",
            //You can add either custom or from the built-in file types. See "Defining custom file types" on how to create a custom one.
            FileTypeFilter = [SetFiles]
        });
        foreach (var path in localPaths)
        {
            await new ParseVerb { URI = path.TryGetLocalPath()! }.Run(Container!, progressReporter);
            var filesFolderPath = Path.Get(path.Path.LocalPath).Up().Combine(path.Name[0..^7] + "-files");
            if (!filesFolderPath.Exists)
                continue;

            var imagePath = Path.Current.Add("Images");

            log.Information("Copying all files");
            log.Information("From: {path}", filesFolderPath.FullPath);
            log.Information("To: {newPath}", imagePath.FullPath);

            foreach (var filesPath in filesFolderPath.AllFiles())
                filesPath.Copy(imagePath, Overwrite.Always);
        }
    }

    internal async Task Load()
    {
        if (Container is null)
            return;
        
        log ??= Serilog.Log.ForContext<MainWindowViewModel>();

        var updater = Container.GetRequiredService<WeissSchwarzDatabaseUpdater>();
        updater.OnStarting += Updater_OnStarting;
        updater.OnEnding += Updater_OnEnding;

        var progressReporter = new ProgressReporter(log, message => Status = message);
        await Container.GetInstance<UpdateVerb>().Run(Container, progressReporter);

        await LoadDatabase(progressReporter);
    }

    private async Task LoadDatabase(ProgressReporter progressReporter)
    {
        DatabaseViewList.Clear();
        DeckRatioList.Clear();

        using var db = Container!.GetInstance<CardDatabaseContext>();
        var initialCardList = db.WeissSchwarzCards.Take(100).ToList();
        var cacheList = initialCardList.Where(c => c.GetCachedImagePath() is null && c.EnglishSetType != EnglishSetType.Custom).ToAsyncEnumerable();
        await new CacheVerb { }.Cache(Container, progressReporter, cacheList);

        foreach (var card in initialCardList)
        {
            var cardView = new CardEntryViewModel(card);
            DatabaseViewList.Add(cardView);
        }

        Status = "Done";
    }

    private async Task Updater_OnEnding(WeissSchwarzDatabaseUpdater sender, Card.API.Services.UpdateEventArgs args)
    {
        log.Information(args.Status);
        Status = args.Status;
        await ValueTask.CompletedTask;
    }

    private async Task Updater_OnStarting(WeissSchwarzDatabaseUpdater sender, Card.API.Services.UpdateEventArgs args)
    {
        log.Information(args.Status);
        Status = args.Status;
        await ValueTask.CompletedTask;
    }

    internal async Task ImportSet()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ImportSetDC.IsVisible = true;
        });
    }

    internal async Task ImportDeck()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ImportDeckDC.IsVisible = true;
        });
    }

    internal async Task AddCard(WeissSchwarzCard card)
    {
        var existingRatio = DeckRatioList.Where(crv => crv.Card.Serial == card.Serial).FirstOrDefault();
        var cardsWithTheSameName = DeckRatioList.Where(crv => crv.Card.Name.AsNonEmptyString() == card.Name.AsNonEmptyString())
            .Select(crv => crv.Ratio)
            .Sum();
        var totalCards = DeckRatioList.Select(crv => crv.Ratio).Sum();
        var totalCXes = DeckRatioList.Where(crv => crv.Card.Type == CardType.Climax)
            .Select(crv => crv.Ratio)
            .Sum();
        if (cardsWithTheSameName >= 4)
            return;
        if (card.Type == CardType.Climax && totalCXes >= 8)
            return;
        if (totalCards >= 50)
            return;
        if (existingRatio is not null)
        {
            existingRatio.Ratio = Math.Min(existingRatio.Ratio + 1, 4);
        }
        else
        {
            DeckRatioList.Add(new CardRatioViewModel(card, 1));
            SortDeck();
        }
        UpdateDeckStats();

        await ValueTask.CompletedTask;
    }

    internal async Task RemoveCard(WeissSchwarzCard card)
    {
        var existingRatio = DeckRatioList.Where(crv => crv.Card.Name.AsNonEmptyString() == card.Name.AsNonEmptyString()).FirstOrDefault();
        if (existingRatio is not null)
        {
            existingRatio.Ratio = Math.Max(existingRatio.Ratio - 1, 0);
            if (existingRatio.Ratio <= 0)
            {
                DeckRatioList.Remove(existingRatio);
                SortDeck();
            }
            UpdateDeckStats();
        }

        await ValueTask.CompletedTask;
    }
    internal void SortDeck()
    {
        DeckRatioList.SortByDescending(crv => (
            level: crv.Card.Level,
            cost: crv.Card.Cost,
            cardTypeKey: TranslateSortKey(crv.Card.Type)
        ));

        static int TranslateSortKey(CardType type) => type switch
        {
            CardType.Climax => 0,
            CardType.Event => 1,
            CardType.Character => 2,
            _ => 3
        };   
    }

    internal void UpdateDeckStats()
    {
        var totalCards = DeckRatioList.Select(crv => crv.Ratio).Sum();
        var totalCharas = DeckRatioList.Where(crv => crv.Card.Type == CardType.Character)
            .Select(crv => crv.Ratio)
            .Sum();
        var totalEvents = DeckRatioList.Where(crv => crv.Card.Type == CardType.Event)
            .Select(crv => crv.Ratio)
            .Sum();
        var totalCXes = DeckRatioList.Where(crv => crv.Card.Type == CardType.Climax)
            .Select(crv => crv.Ratio)
            .Sum();
        DeckStats = $"[ {totalCards} / {totalCharas} / {totalEvents} / {totalCXes} ]";
    }

    public static FilePickerFileType SetFiles { get; } = new("WS Set Files")
    {
        Patterns = ["*.ws-set"],
        AppleUniformTypeIdentifiers = ["public.ronelm2000.ws.set"],
        MimeTypes = ["application/json"]
    };

    public static FilePickerFileType DeckFiles { get; } = new("WS Decks")
    {
        Patterns = ["*.ws-dek"],
        AppleUniformTypeIdentifiers = ["public.ronelm2000.ws.deck"],
        MimeTypes = ["application/json"]
    };

    [GeneratedRegex(@"(?:(\w+)\:(?:(\w+)|\""([\w ]+)\"")|(\w+))")]
    private static partial Regex ScryfallStyleRegex();
}
