using Lasso;
using NUnit.Framework;

[TestFixture]
public class VersionManagerTests
{
    private VersionManager _versionManager;

    [SetUp]
    public void SetUp()
    {
        _versionManager = new VersionManager();
    }

    [Test]
    public void FilterMatchingVersions_ShouldReturnMatchingVersions()
    {
        var versionDef = "1.2.*";
        var availableVersions = new List<string> { "1.2.0", "1.2.1", "1.3.0", "1.2.2" };

        var result = _versionManager.FilterMatchingVersions(versionDef, availableVersions);

        Assert.That(result, Is.EquivalentTo(new List<string> { "1.2.0", "1.2.1", "1.2.2" }));
    }

    [Test]
    public void FilterMatchingVersions_NoMatches_ShouldReturnEmptyList()
    {
        var versionDef = "2.0.*";
        var availableVersions = new List<string> { "1.2.0", "1.2.1", "1.3.0" };

        var result = _versionManager.FilterMatchingVersions(versionDef, availableVersions);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetHighestVersion_ShouldReturnHighestVersion()
    {
        var versions = new List<string> { "1.2.0", "1.2.1", "1.3.0", "1.2.2" };

        var result = _versionManager.GetHighestVersion(versions);

        Assert.AreEqual("1.3.0", result);
    }

    [Test]
    public void GetHighestVersion_WithVersionPrefix_ShouldReturnHighestVersion()
    {
        var versions = new List<string> { "v1.2.0", "v1.3.0", "v1.2.2" };

        var result = _versionManager.GetHighestVersion(versions);

        Assert.AreEqual("v1.3.0", result);
    }
}