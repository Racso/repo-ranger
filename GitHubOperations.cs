﻿using System.Text.RegularExpressions;

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

    public async Task CloneRepoFromRefName(string url, string refName, string directory, string username, string pat)
    {
        logger.Debug($"Starting to obtain files for repository at {url}, version {refName}...");

        string destinationPath = Path.Combine(Environment.CurrentDirectory, directory);

        UriBuilder uriBuilder = new UriBuilder(url)
        {
            UserName = username,
            Password = pat
        };
        string urlWithCredentials = uriBuilder.Uri.ToString();

        string command = $"git clone --depth 1 --branch {refName} -c advice.detachedHead=false {urlWithCredentials} {destinationPath}";
        logger.Debug("Cloning repository...");

        List<CommandRunner.ResultLine> resultLines = await commandRunner.RunCommandAsync(command);

        foreach (CommandRunner.ResultLine line in resultLines)
        {
            if (line.Type == CommandRunner.ResultLineType.Standard)
                logger.Info(line.Text);
            else
                logger.Error(line.Text);
        }
    }

    public async Task<List<RefData>> GetRepoTagsAsync(string repoUrl, string username, string pat)
    {
        string urlWithCredentials = repoUrl.Insert(8, $"{username}:{pat}@");
        string command = $"git ls-remote --tags {urlWithCredentials}";

        List<CommandRunner.ResultLine> resultLines = await commandRunner.RunCommandAsync(command);
        List<RefData> output = new List<RefData>();

        foreach (CommandRunner.ResultLine line in resultLines)
        {
            if (line.Type == CommandRunner.ResultLineType.Standard)
            {
                Match match = Regex.Match(line.Text, @"(?<hash>[a-f0-9]+)\s+refs/tags/(?<tag>[v0-9.]+)$");
                if (match.Success)
                {
                    output.Add(new RefData
                    {
                        Type = RefType.Tag,
                        Name = match.Groups["tag"].Value,
                        Hash = match.Groups["hash"].Value
                    });
                }
            }
            else
            {
                logger.Error(line.Text);
            }
        }

        return output;
    }

    public async Task<RefData> GetBranchAsync(string repoUrl, string branchName, string username, string pat)
    {
        string urlWithCredentials = repoUrl.Insert(8, $"{username}:{pat}@");
        string command = $"git ls-remote --heads {urlWithCredentials} {branchName}";

        List<CommandRunner.ResultLine> resultLines = await commandRunner.RunCommandAsync(command);

        foreach (CommandRunner.ResultLine line in resultLines)
        {
            if (line.Type == CommandRunner.ResultLineType.Standard)
            {
                Match match = Regex.Match(line.Text, @"(?<hash>[a-f0-9]+)\s+refs/heads/(?<branch>[^\s]+)$");
                if (match.Success && match.Groups["branch"].Value == branchName)
                {
                    return new RefData
                    {
                        Type = RefType.Branch,
                        Name = branchName,
                        Hash = match.Groups["hash"].Value
                    };
                }
            }
            else
            {
                logger.Error(line.Text);
            }
        }

        return new RefData();
    }
}