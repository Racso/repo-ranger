using System.Text.Json;
using Lasso;

ProgramArgs pargs = new ProgramArgs(args);

string baseDirectory = pargs.GetString("--base-dir", Environment.CurrentDirectory);
string lassoPath = pargs.GetString("--lasso-json", Path.Combine(baseDirectory, "lasso.json"));
string lassoAuthPath = pargs.GetString("--lasso-auth-json", Path.Combine(baseDirectory, "lasso-auth.json"));

List<Repository> repositories = ReadRepositoriesFromJson(lassoPath);
GitHubAuthData authData = JsonSerializer.Deserialize<GitHubAuthData>(File.ReadAllText(lassoAuthPath));

ILogger logger = new ConsoleLogger();
VersionManager versions = new VersionManager();
CommandRunner commandRunner = new CommandRunner();
GitHubOperations github = new GitHubOperations(commandRunner, logger);

foreach (Repository repository in repositories)
{
    logger.Info($"Processing repository at {repository.Url}...");
    logger.Info($"Obtaining tags...");
    List<string> tags = await github.GetRepoTagsAsync(repository.Url, authData.Username, authData.Token);
    logger.Info($"Tags: {string.Join(", ", tags)}");

    List<string> validVersions = versions.FilterMatchingVersions(repository.Version, tags);
    logger.Info($"Valid versions for {repository.Version}: {string.Join(", ", validVersions)}");

    string highestVersion = versions.GetHighestVersion(validVersions);
    logger.Info($"Best available version: {highestVersion}");

    if (string.IsNullOrEmpty(highestVersion))
    {
        logger.Info("No valid version found.");
        continue;
    }

    logger.Info($"Obtaining files for version {highestVersion}...");
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
    logger.Info($"Starting to obtain files for repository at {repo.Url}, version {tag}...");

    string destinationPath = Path.Combine(baseDirectory, repo.Destination);

    // Check if the destination folder exists and is not empty, then delete it
    if (Directory.Exists(destinationPath))
    {
        logger.Info("Clearing existing destination directory...");
        FileUtils.DeleteDirectory(destinationPath);
    }

    logger.Info($"Creating destination directory at: {destinationPath}");
    Directory.CreateDirectory(destinationPath);

    await github.CloneRepoFromTag(repo.Url, tag, destinationPath, username, pat);

    string gitFolderPath = Path.Combine(destinationPath, ".git");
    if (Directory.Exists(gitFolderPath))
    {
        logger.Info("Removing .git folder...");
        FileUtils.DeleteDirectory(gitFolderPath);
    }

    logger.Info($"Finished obtaining files for repository at {repo.Url}.");
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