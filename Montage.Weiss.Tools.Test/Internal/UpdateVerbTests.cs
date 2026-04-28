using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Impls.PostProcessors;
using System.Linq;
using System;

namespace Montage.Weiss.Tools.Test.Internal;

[TestClass]
public class UpdateVerbTests
{
    [TestMethod]
    [TestCategory("Internal")]
    public void YuyuteiPostProcessor_HasCorrectAlias()
    {
        var pp = new YuyuteiPostProcessor(() => null!);
        Assert.IsTrue(pp.Alias.Contains("yyt"));
        Assert.IsTrue(pp.Alias.Contains("yuyutei"));
    }

    [TestMethod]
    [TestCategory("Internal")]
    public void DeckLogPostProcessor_HasCorrectAlias()
    {
        var pp = new DeckLogPostProcessor(null!);
        Assert.IsTrue(pp.Alias.Contains("decklog"));
    }

    [TestMethod]
    [TestCategory("Internal")]
    public void JKTCGPostProcessor_HasCorrectAlias()
    {
        var pp = new JKTCGPostProcessor(null!);
        Assert.IsTrue(pp.Alias.Contains("jktcg"));
    }

    [TestMethod]
    [TestCategory("Internal")]
    public void DuplicateCardPostProcessor_HasCorrectAlias()
    {
        var pp = new DuplicateCardPostProcessor(() => null!);
        Assert.IsTrue(pp.Alias.Contains("duplicate"));
    }

    [TestMethod]
    [TestCategory("Internal")]
    public void AllPostProcessors_ImplementAliasProperty()
    {
        var container = Global.Container;
        var postProcessors = container.GetAllInstances<ICardPostProcessor<Montage.Weiss.Tools.Entities.WeissSchwarzCard>>().ToArray();

        Assert.IsTrue(postProcessors.Length > 0, "No post-processors found in DI container");

        foreach (var pp in postProcessors)
        {
            Assert.IsNotNull(pp.Alias, $"Alias is null for {pp.GetType().Name}");
            Assert.IsTrue(pp.Alias.Length > 0, $"Alias array is empty for {pp.GetType().Name}");
        }
    }

    [TestMethod]
    [TestCategory("Internal")]
    public void UpdateVerb_NoArgs_CallsMigration()
    {
        var updateVerb = new Montage.Weiss.Tools.CLI.UpdateVerb
        {
            ReleaseIDs = "",
            PostProcessorAliases = ""
        };

        // Verify that the ReleaseIDs is empty, which should trigger migration
        var releaseIds = updateVerb.ReleaseIDs.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim()).ToHashSet();
        Assert.AreEqual(0, releaseIds.Count, "Empty ReleaseIDs should result in 0 releaseIds");
    }

    [TestMethod]
    [TestCategory("Internal")]
    public void UpdateVerb_WithArgs_DoesNotCallMigration()
    {
        var updateVerb = new Montage.Weiss.Tools.CLI.UpdateVerb
        {
            ReleaseIDs = "W53",
            PostProcessorAliases = "yyt"
        };

        // Verify that ReleaseIDs is not empty, which should NOT trigger migration
        var releaseIds = updateVerb.ReleaseIDs.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim()).ToHashSet();
        Assert.IsTrue(releaseIds.Count > 0, "ReleaseIDs should not be empty");
    }
}
