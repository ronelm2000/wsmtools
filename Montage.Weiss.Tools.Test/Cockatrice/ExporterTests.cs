using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Services;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Exporters.Database.Cockatrice;
using Montage.Weiss.Tools.Test.Commons;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.Cockatrice;

[TestClass]
public class ExporterTests
{
    public TestContext TestContext { get; set; }

    [TestMethod(DisplayName = "Database Exporter Test (Cockatrice)")]
    public async Task TestExporterForDatabase()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        var ioc = Program.Bootstrap();
        var progressReporter = NoOpProgress<object>.Instance;

        await new ParseVerb()
        {
            URI = "https://www.encoredecks.com/api/series/5d3232ec7cd9b718cd126e2e/cards"
        }.Run(ioc, progressReporter, TestContext.CancellationToken);

        await new ParseVerb()
        {
            URI = "https://www.heartofthecards.com/translations/little_busters!_anime_booster_pack.html"
        }.Run(ioc, progressReporter, TestContext.CancellationToken);

        await using var db = ioc.GetInstance<CardDatabaseContext>();
        IDatabaseExportInfo info = new MockDatabaseExportInfo();
        await new CockatriceExporter().Export(db, info, TestContext.CancellationToken);
    }
}

internal class MockDatabaseExportInfo : IDatabaseExportInfo
{
    public IEnumerable<string> ReleaseIDs => new string[] { };
    public IEnumerable<string> Serials => new string[] { };
    public string Source => "";
    public string Destination => "./Export/";
    public string Parser => "";
    public string Exporter => "";
    public string OutCommand => "";
    public IEnumerable<string> Flags => new string[] { };
    public bool NonInteractive => true;

    public IProgress<DeckExportProgressReport> Progress => NoOpProgress<DeckExportProgressReport>.Instance;
}
