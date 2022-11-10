using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Card.API.Entities;
using Montage.Card.API.Services;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Exporters.Database.Cockatrice;
using Montage.Weiss.Tools.Test.Commons;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.Cockatrice
{
    [TestClass]
    public class ExporterTests
    {
        [TestMethod("Cockatrice Database Exporter Test (Full)")]
        public async Task TestExporterForDatabase()
        {
            Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
            Lamar.Container ioc = Program.Bootstrap();
            var progressReporter = NoOpProgress<object>.Instance;
            using var db = ioc.GetInstance<CardDatabaseContext>();

            /*
            await new ParseVerb()
            {
                URI = "https://heartofthecards.com/translations/love_live!_sunshine_school_idol_festival_6th_anniversary_booster_pack.html"
            }.Run(ioc);
            */

            await new ParseVerb()
            {
                URI = "https://www.encoredecks.com/api/series/5d3232ec7cd9b718cd126e2e/cards"
            }.Run(ioc, progressReporter);

            await new ParseVerb()
            {
                URI = "https://www.heartofthecards.com/translations/little_busters!_anime_booster_pack.html"
            }.Run(ioc, progressReporter);

            IDatabaseExportInfo info = new MockDatabaseExportInfo();
            await new CockatriceExporter().Export(db, info);
        }
    }

    internal class MockDatabaseExportInfo : IDatabaseExportInfo
    {
        public IEnumerable<string> ReleaseIDs => new string[] { };
        public string Source => "";
        public string Destination => "./Export/";
        public string Parser => "";
        public string Exporter => "";
        public string OutCommand => "";
        public IEnumerable<string> Flags => new string[] { };
        public bool NonInteractive => true;

        public IProgress<DeckExportProgressReport> Progress => NoOpProgress<DeckExportProgressReport>.Instance;
    }
}
