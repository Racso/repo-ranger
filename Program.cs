using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

string testPath = "C:\\Dev\\Lasso";
string testLassoPath = Path.Combine(testPath, "lasso.json");
string testlassoAuthPath = Path.Combine(testPath, "lasso-auth.json");

List<Repository> repositories = ReadRepositoriesFromJson(testLassoPath);
GitHubAuthData authData = JsonSerializer.Deserialize<GitHubAuthData>(File.ReadAllText(testlassoAuthPath));

VersionManager versions = new VersionManager();

foreach (Repository repository in repositories)
{
    Console.WriteLine($"Url: {repository.Url}");
    Console.WriteLine($"Destination: {repository.Destination}");
}

// Test: fetch repositories
foreach (Repository repository in repositories)
{
    Console.WriteLine($"Processing repository at {repository.Url}...");
    Console.WriteLine($"Obtaining tags...");
    List<string> tags = await GetRepoTagsAsync(repository.Url, authData.Username, authData.Token);
    Console.WriteLine($"Tags: {string.Join(", ", tags)}");

    List<string> validVersions = versions.FilterMatchingVersions(repository.Version, tags);
    Console.WriteLine($"Valid versions for {repository.Version}: {string.Join(", ", validVersions)}");

    string highestVersion = versions.GetHighestVersion(validVersions);
    Console.WriteLine($"Best available version: {highestVersion}");

    if (string.IsNullOrEmpty(highestVersion))
    {
        Console.WriteLine("No valid version found.");
        continue;
    }

    Console.WriteLine($"Obtaining files for version {highestVersion}...");
    await ObtainRepoFilesAsync(repository, highestVersion, authData.Username, authData.Token);
}

List<Repository> ReadRepositoriesFromJson(string filePath)
{
    string jsonContent = File.ReadAllText(filePath);
    JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    Dictionary<string, List<Repository>> data = JsonSerializer.Deserialize<Dictionary<string, List<Repository>>>(jsonContent, options);
    return data?["repositories"] ?? new List<Repository>();
}

async Task ObtainRepoFilesAsync(Repository repo, string tag, string username, string pat)
{
    Console.WriteLine($"Starting to obtain files for repository at {repo.Url}, version {tag}...");

    string destinationPath = Path.Combine(Environment.CurrentDirectory, repo.Destination);

    // Check if the destination folder exists and is not empty, then delete it
    if (Directory.Exists(destinationPath))
    {
        Console.WriteLine("Clearing existing destination directory...");
        DeleteDirectory(destinationPath);
    }

    Console.WriteLine($"Creating destination directory at: {destinationPath}");
    Directory.CreateDirectory(destinationPath);

    string urlWithCredentials = repo.Url.Insert(8, $"{username}:{pat}@");

    string command = $"git clone --depth 1 --branch {tag} {urlWithCredentials} {destinationPath}";
    Console.WriteLine("Executing command: " + command);

    Process process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c {command}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        }
    };

    process.OutputDataReceived += (sender, e) =>
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            Console.WriteLine("Output: " + e.Data);
        }
    };

    process.ErrorDataReceived += (sender, e) =>
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            Console.WriteLine("Error: " + e.Data);
        }
    };

    Console.WriteLine("Starting process...");
    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();
    await process.WaitForExitAsync();
    Console.WriteLine("Process finished.");

    // Attempt to remove the .git folder
    string gitFolderPath = Path.Combine(destinationPath, ".git");
    if (Directory.Exists(gitFolderPath))
    {
        Console.WriteLine("Removing .git folder...");
        DeleteDirectory(gitFolderPath);
    }

    Console.WriteLine($"Finished obtaining files for repository at {repo.Url}.");
}

async Task<List<string>> GetRepoTagsAsync(string repoUrl, string username, string pat)
{
    string urlWithCredentials = repoUrl.Insert(8, $"{username}:{pat}@");
    var command = $"git ls-remote --tags {urlWithCredentials}";
    var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c {command}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        }
    };

    List<string> output = new List<string>();

    process.Start();

    using (var reader = process.StandardOutput)
    {
        string line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            Match match = Regex.Match(line, @"refs/tags/(?<tag>[v0-9.]+)$");
            if (match.Success)
            {
                output.Add(match.Groups["tag"].Value);
            }
        }
    }

    using (var errorReader = process.StandardError)
    {
        string errorLine;
        while ((errorLine = await errorReader.ReadLineAsync()) != null)
        {
            Console.WriteLine("Error: " + errorLine);
        }
    }

    await process.WaitForExitAsync();

    return output;
}

void DeleteDirectory(string directory)
{
    foreach (string subdirectory in Directory.EnumerateDirectories(directory))
    {
        DeleteDirectory(subdirectory);
    }

    foreach (string fileName in Directory.EnumerateFiles(directory))
    {
        FileInfo fileInfo = new FileInfo(fileName)
        {
            Attributes = FileAttributes.Normal
        };
        fileInfo.Delete();
    }

    Directory.Delete(directory);
}

class Repository
{
    public string Url { get; set; }
    public string Destination { get; set; }
    public string Version { get; set; }
}

class GitHubAuthData
{
    public string Username { get; set; }
    public string Token { get; set; }
}