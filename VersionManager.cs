using System.Text.RegularExpressions;

class VersionManager
{
    public List<string> FilterMatchingVersions(string versionDef, IEnumerable<string> availableVersions)
    {
        string pattern = "^" + Regex.Escape(versionDef).Replace("\\*", ".*") + "$";
        return availableVersions.Where(v => Regex.IsMatch(StripPrefix(v), pattern)).ToList();
    }

    public string GetHighestVersion(IEnumerable<string> versions)
    {
        return versions.OrderByDescending(v => new Version(StripPrefix(v))).FirstOrDefault();
    }

    private string StripPrefix(string version)
    {
        return version.StartsWith("v") ? version.Substring(1) : version;
    }
}