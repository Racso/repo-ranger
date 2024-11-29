using System.Diagnostics;

namespace RepoRanger;

public class CommandRunner
{
    public async Task<List<ResultLine>> RunCommandAsync(string command)
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

        var resultLines = new List<ResultLine>();

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                resultLines.Add(new ResultLine { Text = e.Data, Type = ResultLineType.Standard });
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                resultLines.Add(new ResultLine { Text = e.Data, Type = ResultLineType.Error });
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        return resultLines;
    }

    public struct ResultLine
    {
        public string Text { get; set; }
        public ResultLineType Type { get; set; }
    }

    public enum ResultLineType
    {
        Standard,
        Error
    }
}