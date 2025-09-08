using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using Fluent.IO;
using JasperFx.Core;
using Microsoft.Extensions.DependencyInjection;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Services;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.GUI.Extensions;
using Montage.Weiss.Tools.GUI.Utilities;
using Montage.Weiss.Tools.GUI.ViewModels.Dialogs;
using Montage.Weiss.Tools.GUI.ViewModels.Query;
using Montage.Weiss.Tools.Impls.Exporters.Deck;
using Montage.Weiss.Tools.Impls.Inspectors.Deck;
using Montage.Weiss.Tools.Impls.Parsers.Deck;
using Montage.Weiss.Tools.Impls.Services;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.GUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private static readonly Regex searchRegex = ScryfallStyleRegex();

    private ILogger log;
    private bool _isBootstrapped = false;
    private SourceList<CardEntryViewModel> _databaseViewSourceList;
    private ReadOnlyObservableCollection<CardEntryViewModel> _databaseViewList;

    public Func<Window>? Parent { get; init; }
    public Lamar.IContainer? Container { get; init; }

    public ReadOnlyObservableCollection<CardEntryViewModel> DatabaseViewList => _databaseViewList;

    public ObservableCollection<CardRatioViewModel> DeckRatioList { get; set; } = [];

    public ObservableCollection<CardSearchQueryViewModel> SearchQueries { get; set; } = [];

    public ReactiveCommand<Unit, Unit> ImportSetCommand { get; init; }
    public ReactiveCommand<Unit, Unit> ImportDeckCommand { get; init; }
    public ReactiveCommand<Unit, Unit> OpenLocalSetCommand { get; init; }
    public ReactiveCommand<Unit, Unit> SaveDeckCommand { get; init; }
    public ReactiveCommand<Unit, Unit> OpenDeckCommand { get; init; }
    public ReactiveCommand<Unit, bool> UpdateDatabaseViewCommand { get; init; }
    public ReactiveCommand<Unit, Unit> ExportDeckToTabletopCommand { get; init; }
    public ReactiveCommand<Unit, Unit> ExportToProxyDocumentCommand { get; init; }
    public ReactiveCommand<Unit, Unit> ExportToTranslationDocumentCommand { get; init; }
    public ReactiveCommand<Unit, Unit> InjectSearchQueryCommand { get; init; }

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
    private ImportSetViewModel _importSetDC;

    [ObservableProperty]
    private ImportDeckViewModel _importDeckDC;

    [ObservableProperty]
    private ImportTranslationsViewModel _importTranslationsDC;

    [ObservableProperty]
    private NoTranslationsWarningViewModel _noTranslationsWarningDC;

    [ObservableProperty]
    private string? destinationBookmark;

    public MainWindowViewModel()
    {
        Status = "";
        DeckName = "";
        DeckRemarks = "";
        SearchBarText = "";
        DeckStats = "[ 0 / 0 / 0 / 0 ]";

        if (Design.IsDesignMode)
        {
            _databaseViewSourceList = new SourceList<CardEntryViewModel>();
            _databaseViewSourceList.AddRange(
                [
                    new CardEntryViewModel(
                    new Uri("avares://wsm-gui/Assets/Samples/sample_card.jpg"),
                    new MultiLanguageString { EN = "Sample 1", JP = "Sample 1 But JP" },
                    [
                        new MultiLanguageString { EN = "AAAA", JP = "AAAA JP" },
                        new MultiLanguageString { EN = "BBBB", JP = "BBBB JP" }
                    ]
                 ) { Parent = this },
                new CardEntryViewModel(
                    new Uri("avares://wsm-gui/Assets/Samples/sample_card.jpg"),
                    new MultiLanguageString { EN = "Sample 2", JP = "Sample 2 But JP" },
                    [ new MultiLanguageString { EN = "AAAA", JP = "AAAA JP" } ]
                ) { Parent = this },
                new CardEntryViewModel(
                    new Uri("avares://wsm-gui/Assets/Samples/sample_card.jpg"),
                    new MultiLanguageString { EN = "Sample 3", JP = "Sample 3 But JP" },
                    [ new MultiLanguageString { EN = "AAAA", JP = "AAAA JP" } ]
                ) { Parent = this }
                ]
            );

            DeckRatioList.Add(new CardRatioViewModel());
            DeckRatioList.Add(new CardRatioViewModel() { Image = new Uri("avares://wsm-gui/Assets/Samples/sample_card.jpg").Load() });
            ImportSetDC = new ImportSetViewModel { IsVisible = false, Parent = () => null };
            ImportDeckDC = new ImportDeckViewModel { IsVisible = false, Parent = () => null };
            ImportTranslationsDC = new ImportTranslationsViewModel { IsVisible = false, Parent = () => null };
            NoTranslationsWarningDC = new NoTranslationsWarningViewModel { IsVisible = false, Parent = () => null };
        }
        else
        {
            _databaseViewSourceList = new SourceList<CardEntryViewModel>();
            ImportSetDC = new ImportSetViewModel { IsVisible = false, Parent = () => this };
            ImportDeckDC = new ImportDeckViewModel { IsVisible = false, Parent = () => this };
            ImportTranslationsDC = new ImportTranslationsViewModel { IsVisible = false, Parent = () => this };
            NoTranslationsWarningDC = new NoTranslationsWarningViewModel { IsVisible = false, Parent = () => this };
        }

        log = Serilog.Log.Logger.ForContext<MainWindowViewModel>();

        _databaseViewSourceList.Connect()
            .Bind(out _databaseViewList)
            .Subscribe();

        ImportSetCommand = ReactiveCommand.CreateFromTask(ImportSet);
        ImportDeckCommand = ReactiveCommand.CreateFromTask(ImportDeck);
        OpenLocalSetCommand = ReactiveCommand.CreateFromTask(OpenLocalSet);
        SaveDeckCommand = ReactiveCommand.CreateFromTask(SaveDeck);
        OpenDeckCommand = ReactiveCommand.CreateFromTask(OpenDeck);
        UpdateDatabaseViewCommand = ReactiveCommand.CreateFromTask<Unit, bool>((_, t) => UpdateDatabaseView(t));
        ExportDeckToTabletopCommand = ReactiveCommand.CreateFromTask(ExportDeckToTabletop);
        ExportToProxyDocumentCommand = ReactiveCommand.CreateFromTask(ExportToProxyDocument);
        ExportToTranslationDocumentCommand = ReactiveCommand.CreateFromTask(ExportToTranslationDocument);
        InjectSearchQueryCommand = ReactiveCommand.CreateFromTask(InjectSearchQuery);

        this.WhenAnyValue(r => r.SearchBarText)
            .Merge(SearchQueries.ToObservableChangeSet(x => x).Select(changes => "bruh"))
            .ObserveOn(RxApp.TaskpoolScheduler)
            .Select(sender => Unit.Default)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .SubscribeOn(RxApp.TaskpoolScheduler)
            .InvokeCommand(UpdateDatabaseViewCommand);

        OpenLocalSetCommand.ThrownExceptions.Subscribe(ReportException);
        UpdateDatabaseViewCommand.ThrownExceptions.Subscribe(ReportException);
    }

    private async Task InjectSearchQuery()
    {
        var aaaa = SearchBarText;
        var searchRegexResults = searchRegex.Matches(SearchBarText);
        var searchTerms = searchRegexResults
            .SelectMany(x => TranslateMatch(x))
            .ToList();

        await Observable.Start(() =>
        {
            SearchQueries.AddRange(searchTerms, 0);
            SearchBarText = searchRegex.Replace(SearchBarText, "");
        }, RxApp.MainThreadScheduler);

        IEnumerable<CardSearchQueryViewModel> TranslateMatch(Match searchRegexMatch)
        {
            var valueString = Strings.Or(() => searchRegexMatch.Groups[2].Value, () => searchRegexMatch.Groups[3].Value);
            CardSearchQueryViewModel? result = searchRegexMatch.Groups[1].Value switch
            {
                ("c" or "color") when valueString is not null => new ColorQueryViewModel(valueString.Split(",")),
                ("co" or "cost") when valueString is not null => new CostQueryViewModel(valueString.Split(",")),
                ("cx" or "climax") when valueString is not null => new ClimaxComboQueryViewModel(valueString),
                ("l" or "level") when valueString is not null => new LevelQueryViewModel(valueString.Split(",")),
                ("o" or "e" or "effect") when valueString is not null => new EffectQueryViewModel(valueString),
                ("set" or "ns") when valueString is not null => new NeoStandardCodeQueryViewModel(valueString.Split(",")),
                ("tr" or "trait" or "traits") when valueString is not null => new TraitQueryViewModel(valueString.Split(",")),
                _ => null
            };
            if (result is not null)
            {
                yield return result;
            }
        }
    }

    private void ReportException(Exception exception)
    {
        Log.Error(exception, "Error occurred");
        Status = exception.Message;
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
            Destination = $"{AppDomain.CurrentDomain.BaseDirectory}/Export/",
            Flags = ["sendtcp", "limit-width(800)"],
            OutCommand = "sharex",
            Progress = progressReporter
        };
        await command.Run(Container!, deck);
    }

    private async Task ExportToProxyDocument(CancellationToken token)
    {
        var storage = Parent!().StorageProvider;
        var folder = DestinationBookmark switch
        {
            string b => (await storage.OpenFolderBookmarkAsync(b))!,
            _ => await AssignExportBookmark(storage)
        };

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
            Exporter = "doc",
            Destination =  folder.TryGetLocalPath() ?? string.Empty,
            Flags = ["nowarn"],
            Progress = progressReporter
        };
        await command.Run(Container!, deck);
    }

    private async Task ExportToTranslationDocument(CancellationToken token)
    {
        var storage = Parent!().StorageProvider;
        var folder = DestinationBookmark switch
        {
            string b => (await storage.OpenFolderBookmarkAsync(b))!,
            _ => await AssignExportBookmark(storage)
        };

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
            Exporter = "trans-doc",
            Destination =  folder.TryGetLocalPath() ?? string.Empty,
            Flags = ["nowarn"],
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
            FileTypeChoices = [DeckFiles]
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

    private async Task<bool> UpdateDatabaseView(string sender, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(SearchBarText) && SearchQueries.Count == 0)
            return false;
        if (token.IsCancellationRequested)
            return false;
        return await UpdateDatabaseView(token);
    }

    private async Task<bool> UpdateDatabaseView(IReadOnlyCollection<CardSearchQueryViewModel> changedModels, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(SearchBarText) && SearchQueries.Count == 0)
            return false;
        if (token.IsCancellationRequested)
            return false;
        return await UpdateDatabaseView(token);
    }

    private async Task<bool> UpdateDatabaseView(CancellationToken token)
    {
        if (Container is null)
            return false;
        if (!_isBootstrapped)
            return false;

        Log.Information("Executing on: {name}", Thread.CurrentThread.Name);

        var progressReporter = new ProgressReporter(log, message => Status = message);
        var searchTerms = searchRegex.Matches(SearchBarText)
            .Select(x => TranslateMatch(x))
            .ToList();

        Log.Information("Loading Database and Obtaining Data...");
        await Dispatcher.UIThread.InvokeAsync(() => Status = "Loading Database", DispatcherPriority.ApplicationIdle);

        using var db = Container.GetInstance<CardDatabaseContext>();
        var searchCardList = await db.WeissSchwarzCards
            .AsNoTracking()
            .ToAsyncEnumerable()
            .Where(c => searchTerms.All(st => st.Invoke(c)) && SearchQueries.All(sq => sq.ToPredicate().Invoke(c)))
            .Distinct(c => c.Serial)
            .OrderBy(c =>
            {
                var serial = WeissSchwarzCard.ParseSerial(c.Serial);
                return (serial.ReleaseID, serial.SetID, !c.IsFoil, c.Rarity);
            })
            .Take(5000)
            .ToListAsync(token);

        await Dispatcher.UIThread.InvokeAsync(() => Status = "Caching when applicable...", DispatcherPriority.ApplicationIdle);

        var cacheList = searchCardList.Where(c => c.GetCachedImagePath() is null && c.EnglishSetType != EnglishSetType.Custom).ToAsyncEnumerable();
        await new CacheVerb { }.Cache(Container, progressReporter, cacheList, token);

        if (token.IsCancellationRequested)
            return false;

        Log.Information("Refreshing Card List...");
        log.Information("All Cards: {ser}", searchCardList?.Count ?? 0);

        if (searchCardList is null || searchCardList.Count == 0)
            return false;

        Log.Information("Dehydrating list.");
        await Dispatcher.UIThread.InvokeAsync(() =>
            _databaseViewSourceList.Edit(list =>
            {
                list.Clear();
            }), DispatcherPriority.ApplicationIdle
            );

        Log.Information("Refreshing list.");
        foreach (var card in searchCardList)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Status = $"Loading [{card.Serial}]";
                _databaseViewSourceList.Add(new CardEntryViewModel(card) { Parent = this });
                //                _databaseViewSourceList.Edit(list => list.Add(model));
            },
            DispatcherPriority.ApplicationIdle
            );
        }

        log.Information("All Serials: {ser}", _databaseViewSourceList.Items.Select(v => v.Card.Serial).Distinct().Count());
        log.Information("All Cards: {ser}", _databaseViewSourceList.Count);

        Status = "Done";

        return true;

        Func<WeissSchwarzCard, bool> TranslateMatch(Match scryfallMatch)
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
            else if (scryfallMatch.Groups[1].Value.Equals("tr", StringComparison.CurrentCultureIgnoreCase) ||
                     scryfallMatch.Groups[1].Value.Equals("trait", StringComparison.CurrentCultureIgnoreCase) ||
                     scryfallMatch.Groups[1].Value.Equals("traits", StringComparison.CurrentCultureIgnoreCase))
            {
                var traitString = Strings.Or(() => scryfallMatch.Groups[2].Value, () => scryfallMatch.Groups[3].Value);
                var traits = traitString?.Split(',') ?? [];
                return c => c.Traits.Any(t => traits.Contains(t.EN ?? string.Empty) || traits.Contains(t.JP ?? string.Empty));
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

            var imagePath = Path.Current.Combine("Images");
            if (!imagePath.Exists)
                imagePath.CreateDirectory();

            log.Information("Copying all files");
            log.Information("From: {path}", filesFolderPath.FullPath);
            log.Information("To: {newPath}", imagePath.FullPath);

            await Task.Run(() => filesFolderPath.AllFiles().Copy(imagePath, Overwrite.Always));
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

        Log.Information("Intializing Inspectors...");

        var sanityInspector = Container.GetService<ILocatorService>()!.FindInspector<SanityTranslationsInspector>();
        sanityInspector.Prompter = async (options) =>
        {
            return await NoTranslationsWarningDC.ShowDialogAsync(options.Cards);
        };

        var fileProcessor = Container.GetService<IFileOutCommandProcessor>()!;
        var defaultStreamDriver = fileProcessor.CreateFileStream;
        var defaultOpenDriver = fileProcessor.OpenFile;
        fileProcessor.CreateFileStream = async (destinationFolder, fileName) =>
        {
            if (!string.IsNullOrWhiteSpace(destinationFolder))
                return await defaultStreamDriver(destinationFolder, fileName);

            var storage = Parent!().StorageProvider;
            var saveFolder = await storage.OpenFolderBookmarkAsync(DestinationBookmark!);
            var saveFile = await saveFolder!.CreateFileAsync(fileName);
            return await saveFile!.OpenWriteAsync();
        };
        fileProcessor.OpenFile = async (destinationFolder, fileName) =>
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(destinationFolder))
                    await defaultOpenDriver(destinationFolder, fileName);

                var storage = Parent!().StorageProvider;
                var folder = await storage.OpenFolderBookmarkAsync(DestinationBookmark!);
                var file = await folder!.GetFileAsync(fileName);
                await Parent()!.Launcher.LaunchFileAsync(file!);
            }
            catch (Exception e)
            {
                Log.Error(e, "Cannot open file,");
            }
        };

        var progressReporter = new ProgressReporter(log, message => Status = message);
        await Container.GetInstance<UpdateVerb>().Run(Container, progressReporter);

        await LoadDatabase(progressReporter);
    }

    private async Task LoadDatabase(ProgressReporter progressReporter)
    {
        DeckRatioList.Clear();

        await Dispatcher.UIThread.InvokeAsync(() => Status = "Loading Database...", DispatcherPriority.ApplicationIdle);

        using var db = Container!.GetInstance<CardDatabaseContext>();
        var initialCardList = db.WeissSchwarzCards.AsNoTracking().Take(100).ToList();
        var cacheList = initialCardList.Where(c => c.GetCachedImagePath() is null && c.EnglishSetType != EnglishSetType.Custom).ToAsyncEnumerable();
        await new CacheVerb { }.Cache(Container, progressReporter, cacheList);

        await Dispatcher.UIThread.InvokeAsync(() => Status = "Loading List...", DispatcherPriority.ApplicationIdle);

        _databaseViewSourceList.Edit(list => list.Clear());

        foreach (var card in initialCardList)
        {
            await Dispatcher.UIThread.InvokeAsync(() => _databaseViewSourceList.Add(new CardEntryViewModel(card) { Parent = this }), DispatcherPriority.ApplicationIdle);
        }

        _isBootstrapped = true;

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

    internal async Task OpenModifyTranslations(CardRatioViewModel cardRatioViewModel)
    {
        await ImportTranslationsDC.ApplyTarget(cardRatioViewModel.Card);
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ImportTranslationsDC.IsVisible = true;
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

    private async Task<IStorageFolder> AssignExportBookmark(IStorageProvider storage) {
        var folders = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions { AllowMultiple = false });
        var folder = folders[0];
        if (folder.CanBookmark)
            DestinationBookmark = await folder.SaveBookmarkAsync() ?? throw new Exception("Cannot save bookmark");
        return folder;
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

    public static FilePickerFileType DocumentFiles { get; } = new("Word Document")
    {
        Patterns = ["*.docx"],
        AppleUniformTypeIdentifiers = ["org.openxmlformats.wordprocessingml.document"],
        MimeTypes = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"]
    };

    [GeneratedRegex(@"(?:(\w+)\:(?:(\w+)|""(.*?)"")|(\w+))")]
    private static partial Regex ScryfallStyleRegex();
}
