using System.Text.RegularExpressions;

namespace RepoRanger;

class VersionManager
{
    public List<RefData> FilterMatchingVersions(string versionDef, IEnumerable<RefData> availableVersions)
    {
        string pattern = "^" + Regex.Escape(versionDef).Replace("\\*", ".*") + "$";
        return availableVersions.Where(v => Regex.IsMatch(StripPrefix(v.Name), pattern)).ToList();
    }

    public RefData GetHighestVersion(IEnumerable<RefData> versions)
    {
        return versions.OrderByDescending(v => new Version(StripPrefix(v.Name))).FirstOrDefault();
    }

    private string StripPrefix(string version)
    {
        return version.StartsWith("v") ? version.Substring(1) : version;
    }
}