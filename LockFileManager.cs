using System.Text.Json;

namespace RepoRanger;

public class LockFileManager
{
    private readonly string lockFilePath;
    private readonly Dictionary<string, RepoLockData> lockData;

    public LockFileManager(string lockFilePath)
    {
        this.lockFilePath = lockFilePath;
        this.lockData = LoadLockFile();
    }

    private Dictionary<string, RepoLockData> LoadLockFile()
    {
        if (File.Exists(lockFilePath))
        {
            string jsonContent = File.ReadAllText(lockFilePath);
            return JsonSerializer.Deserialize<Dictionary<string, RepoLockData>>(jsonContent) ?? new Dictionary<string, RepoLockData>();
        }

        return new Dictionary<string, RepoLockData>();
    }

    public void Update(string repoName, string destination, RefData refData)
    {
        lockData[repoName] = new RepoLockData
        {
            Destination = destination,
            Version = refData.Name,
            LockedHash = refData.Hash
        };
    }

    public string GetLockedHash(string repoName)
    {
        return lockData.TryGetValue(repoName, out RepoLockData data) ? data.LockedHash : string.Empty;
    }

    public RepoLockData GetLockData(string repoName)
    {
        return lockData.TryGetValue(repoName, out RepoLockData data) ? data : RepoLockData.Empty;
    }

    public void Commit()
    {
        string jsonContent = JsonSerializer.Serialize(lockData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(lockFilePath, jsonContent);
    }
}

public class RepoLockData
{
    public string Destination { get; set; }
    public string Version { get; set; }
    public string LockedHash { get; set; }

    public static readonly RepoLockData Empty = new();
}