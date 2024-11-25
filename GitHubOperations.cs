using System.Text.RegularExpressions;

namespace Lasso;

public class GitHubOperations
{
    private readonly CommandRunner commandRunner;
    private readonly ILogger logger;

    public GitHubOperations(CommandRunner commandRunner, ILogger logger)
    {
        this.commandRunner = commandRunner;
        this.logger = logger;
    }

    public async Task CloneRepoFromTag(string url, string tag, string directory, string username, string pat)
    {
        logger.Info($"Starting to obtain files for repository at {url}, version {tag}...");

        string destinationPath = Path.Combine(Environment.CurrentDirectory, directory);

        logger.Info($"Creating destination directory at: {destinationPath}");
        Directory.CreateDirectory(destinationPath);

        UriBuilder uriBuilder = new UriBuilder(url)
        {
            UserName = username,
            Password = pat
        };
        string urlWithCredentials = uriBuilder.Uri.ToString();

        string command = $"git clone --depth 1 --branch {tag} -c advice.detachedHead=false {urlWithCredentials} {destinationPath}";
        logger.Info("Executing command: " + command);

        List<CommandRunner.ResultLine> resultLines = await commandRunner.RunCommandAsync(command);

        foreach (CommandRunner.ResultLine line in resultLines)
        {
            if (line.Type == CommandRunner.ResultLineType.Standard)
                logger.Info(line.Text);
            else
                logger.Error(line.Text);
        }
    }

    public async Task<List<string>> GetRepoTagsAsync(string repoUrl, string username, string pat)
    {
        string urlWithCredentials = repoUrl.Insert(8, $"{username}:{pat}@");
        string command = $"git ls-remote --tags {urlWithCredentials}";

        List<CommandRunner.ResultLine> resultLines = await commandRunner.RunCommandAsync(command);
        List<string> output = new List<string>();

        foreach (CommandRunner.ResultLine line in resultLines)
        {
            if (line.Type == CommandRunner.ResultLineType.Standard)
            {
                Match match = Regex.Match(line.Text, @"refs/tags/(?<tag>[v0-9.]+)$");
                if (match.Success)
                    output.Add(match.Groups["tag"].Value);
            }
            else
            {
                logger.Error(line.Text);
            }
        }

        return output;
    }
}