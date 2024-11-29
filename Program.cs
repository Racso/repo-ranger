using System.Text.Json;
using RepoRanger;

const string Version = "0.2.0";

IntroAsciiWriter.Run(Version);

try
{
    await MainAsync(args);
}
catch (GitHubOperationException)
{
}
catch (Exception ex)
{
    Console.WriteLine($"--------------------------------------------------");
    Console.WriteLine($"FATAL ERROR");
    Console.WriteLine($"{ex}");
}


async Task MainAsync(string[] args)
{
    ProgramArgs pargs = new ProgramArgs(args);

    string baseDirectory = pargs.GetString("--base-dir", Environment.CurrentDirectory);
    string jsonPath = pargs.GetString("--json", Path.Combine(baseDirectory, "ranger.json"));
    string authPath = pargs.GetString("--auth", Path.Combine(baseDirectory, "ranger-auth.json"));
    string lockPath = pargs.GetString("--lock", Path.Combine(baseDirectory, "ranger.lock"));
    bool verbose = pargs.Has("--verbose") || pargs.Has("-v");

    ILogger logger = new ConsoleLogger { IsDebugEnabled = verbose };
    VersionManager versions = new VersionManager();
    CommandRunner commandRunner = new CommandRunner();
    GitHubOperations github = new GitHubOperations(commandRunner, logger);
    LockFileManager lockFileManager = new LockFileManager(lockPath);

    List<Repository> repositories = ReadRepositoriesFromJson(jsonPath);
    if (repositories.Count == 0)
    {
        logger.Warn("No repositories have been defined. Exiting.");
        return;
    }

    GitHubAuthData authData = ReadAuthDataFromJson(authPath);

    foreach (Repository repository in repositories)
    {
        logger.Info($"Processing repository at {repository.Url}...");

        RefData refToUse;

        (RefType refType, string refName) = ParseVersion(repository.Version);

        if (refType == RefType.Tag)
        {
            logger.Debug($"Obtaining tags...");
            List<RefData> tags = await github.GetRepoTagsAsync(repository.Url, authData.Username, authData.Token);
            logger.Debug($"Tags: {string.Join(", ", tags.Select(t => t.Name))}");

            List<RefData> validVersions = versions.FilterMatchingVersions(repository.Version, tags);
            logger.Debug($"Valid version tags for {repository.Version}: {string.Join(", ", validVersions.Select(t => t.Name))}");

            refToUse = versions.GetHighestVersion(validVersions);

            if (string.IsNullOrEmpty(refToUse.Name))
            {
                logger.Error($"No valid version found for repository at {repository.Url}, version {repository.Version}.");
                return;
            }

            logger.Debug($"Using tag '{refToUse.Name}' with hash {refToUse.Hash}.");
        }
        else
        {
            logger.Debug($"Obtaining hash for branch: {refName}...");
            RefData branchRef = await github.GetBranchAsync(repository.Url, refName, authData.Username, authData.Token);
            refToUse = branchRef;

            if (string.IsNullOrEmpty(refToUse.Hash))
            {
                logger.Error("No valid branch found for repository at {repository.Url}, branch {refName}.");
                return;
            }

            logger.Debug($"Using branch '{refToUse.Name}' with hash {refToUse.Hash}.");
        }

        if (string.IsNullOrEmpty(refToUse.Hash))
        {
            logger.Error($"No valid hash found for repository at {repository.Url}, version {repository.Version}.");
            return;
        }

        RepoLockData lockedData = lockFileManager.GetLockData(repository.Url);

        if (lockedData != null && lockedData != RepoLockData.Empty)
        {
            if (lockedData.LockedHash == refToUse.Hash)
            {
                logger.Info($"Repository at {repository.Url} is already up to date. Skipping...");
                continue;
            }

            if (lockedData.Version == refToUse.Name)
                logger.Warn($"Repository at {repository.Url} is locked to version {refToUse.Name}, but the hash has changed. Forcing update.");
        }

        logger.Info($"Updating repository at {repository.Url}. Ref: {refToUse.Name}, hash: {refToUse.Hash}");
        try
        {
            await ObtainRepoFilesAsync(repository, refToUse, authData.Username, authData.Token);
            lockFileManager.Update(repository.Url, repository.Destination, refToUse);
        }
        catch (Exception ex)
        {
            logger.Error($"Failed to obtain files for repository at {repository.Url}: {ex.Message}");
            throw;
        }
    }

    lockFileManager.Commit();
    logger.Info("All repositories processed successfully. Lock file updated.");

    (RefType VersionType, string VersionValue) ParseVersion(string version)
    {
        if (version.StartsWith("b:"))
            return (RefType.Branch, version.Substring(2).Trim());

        return (RefType.Tag, version.Trim());
    }

    List<Repository> ReadRepositoriesFromJson(string filePath)
    {
        try
        {
            string jsonContent = File.ReadAllText(filePath);
            JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            Dictionary<string, List<Repository>> data = JsonSerializer.Deserialize<Dictionary<string, List<Repository>>>(jsonContent, options);
            return data?["repositories"] ?? new List<Repository>();
        }
        catch (FileNotFoundException)
        {
            logger.Error($"Ranger JSON file not found at {filePath}");
            return new List<Repository>();
        }
    }

    GitHubAuthData ReadAuthDataFromJson(string s)
    {
        try
        {
            string fileText = File.ReadAllText(s);
            return JsonSerializer.Deserialize<GitHubAuthData>(fileText);
        }
        catch (FileNotFoundException)
        {
            logger.Warn($"Auth file not found at {s}. Access to repositories may be limited. If you encounter access issues, please provide a valid auth file.");
            return new GitHubAuthData();
        }
    }

    async Task ObtainRepoFilesAsync(Repository repo, RefData refData, string username, string pat)
    {
        logger.Debug($"Starting to obtain files for repository at {repo.Url}, ref {refData}...");

        string destinationPath = Path.Combine(Environment.CurrentDirectory, repo.Destination);

        // Check if the destination folder exists and is not empty, then delete it
        if (Directory.Exists(destinationPath))
        {
            logger.Debug("Clearing existing destination directory...");
            FileUtils.DeleteDirectory(destinationPath);
        }

        logger.Debug($"Creating destination directory at: {destinationPath}");
        Directory.CreateDirectory(destinationPath);

        await github.CloneRepoFromRefName(repo.Url, refData.Name, destinationPath, username, pat);

        string gitFolderPath = Path.Combine(destinationPath, ".git");
        if (Directory.Exists(gitFolderPath))
        {
            logger.Debug("Removing .git folder...");
            FileUtils.DeleteDirectory(gitFolderPath);
        }

        logger.Debug($"Finished obtaining files for repository at {repo.Url}.");
    }
}