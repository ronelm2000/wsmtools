using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Test.Commons;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.HOTC
{

    [TestClass]
    public class ParserTests
    {
        [TestMethod("Full Integration Test (Local Text File) (HOTC) (Typical Use Case)")]
        [DeploymentItem("Resources/shiyoko_prs_hotc.txt")]
        public async Task FullTestRun()
        {
            Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
            Lamar.Container ioc = Program.Bootstrap();

            await new ParseVerb()
            {
                URI = "./shiyoko_prs_hotc.txt",
                ParserHints = new string[] { "hotc" }
            }.Run(ioc);
        }
    }
}
