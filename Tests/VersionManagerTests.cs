using NUnit.Framework;

namespace RepoRanger.Tests;

[TestFixture]
public class VersionManagerTests
{
    private VersionManager _versionManager;

    [SetUp]
    public void SetUp()
    {
        _versionManager = new VersionManager();
    }

    private List<RefData> ConvertToRefDataList(List<string> versions)
    {
        return versions.Select(v => new RefData { Name = v, Type = RefType.Tag }).ToList();
    }

    [Test]
    public void FilterMatchingVersions_ShouldReturnMatchingVersions()
    {
        var versionDef = "1.2.*";
        var availableVersions = new List<string> { "1.2.0", "1.2.1", "1.3.0", "1.2.2" };
        var availableRefData = ConvertToRefDataList(availableVersions);

        var result = _versionManager.FilterMatchingVersions(versionDef, availableRefData);

        Assert.That(result, Is.EquivalentTo(new List<string> { "1.2.0", "1.2.1", "1.2.2" }));
    }

    [Test]
    public void FilterMatchingVersions_NoMatches_ShouldReturnEmptyList()
    {
        var versionDef = "2.0.*";
        var availableVersions = new List<string> { "1.2.0", "1.2.1", "1.3.0" };
        var availableRefData = ConvertToRefDataList(availableVersions);

        var result = _versionManager.FilterMatchingVersions(versionDef, availableRefData);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetHighestVersion_ShouldReturnHighestVersion()
    {
        var versions = new List<string> { "1.2.0", "1.2.1", "1.3.0", "1.2.2" };
        var refDataList = ConvertToRefDataList(versions);

        var result = _versionManager.GetHighestVersion(refDataList);

        Assert.AreEqual("1.3.0", result);
    }

    [Test]
    public void GetHighestVersion_WithVersionPrefix_ShouldReturnHighestVersion()
    {
        var versions = new List<string> { "v1.2.0", "v1.3.0", "v1.2.2" };
        var refDataList = ConvertToRefDataList(versions);

        var result = _versionManager.GetHighestVersion(refDataList);

        Assert.AreEqual("v1.3.0", result);
    }
}