﻿namespace RepoRanger;

public class ProgramArgs
{
    private Dictionary<string, string> rawMap;
    private string command;

    public ProgramArgs()
    {
    }

    public ProgramArgs(string[] args)
    {
        SetArgs(args);
    }

    public void SetArgs(params string[] args)
    {
        if (rawMap != null)
            throw new Exception($"Tried to initialize ProgramArgs more than once.");

        command = args.Length > 0 && !args[0].StartsWith('-') ? args[0] : "";

        rawMap = new Dictionary<string, string>(args.Length);
        foreach (string arg in args)
        {
            string[] parts = arg.Split('=');
            if (parts.Length == 2)
                GetMap()[parts[0]] = parts[1];
            else
                GetMap()[arg] = "";
        }
    }

    public bool Has(string key)
        => GetMap().ContainsKey(key);

    public string GetString(string key, string defaultValue = "")
        => GetMap().GetValueOrDefault(key, defaultValue);

    public int GetInt(string key, int defaultValue = 0)
        => int.TryParse(GetString(key), out int value) ? value : defaultValue;

    public string GetCommand()
        => command;

    public bool IsCommand(string command)
        => this.command == command;

    private Dictionary<string, string> GetMap()
    {
        if (rawMap == null)
            throw new Exception($"Tried to use ProgramArgs without initializing it first.");

        return rawMap;
    }
}