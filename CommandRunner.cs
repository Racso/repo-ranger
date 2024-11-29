using System.Diagnostics;

namespace RepoRanger;

public class CommandRunner
{
    public async Task<int> RunCommandAsync(string command, List<CommandOutputLine> resultLines)
    {
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

        process.OutputDataReceived += (_, e) => TryAddResultLine(e.Data, CommandOutputLineType.Standard);
        process.ErrorDataReceived += (_, e) => TryAddResultLine(e.Data, CommandOutputLineType.Error);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        return process.ExitCode;

        void TryAddResultLine(string line, CommandOutputLineType type)
        {
            if (!string.IsNullOrEmpty(line))
                resultLines.Add(new CommandOutputLine { Text = line, Type = type });
        }
    }
}

public struct CommandOutputLine
{
    public string Text { get; set; }
    public CommandOutputLineType Type { get; set; }
}

public enum CommandOutputLineType
{
    Standard,
    Error
}