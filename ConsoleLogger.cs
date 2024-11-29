namespace Lasso;

public class ConsoleLogger : ILogger
{
    public bool IsDebugEnabled { get; set; }

    public void Info(object message)
    {
        Console.WriteLine(message);
    }

    public void Debug(object message)
    {
        if (IsDebugEnabled)
            WriteLine(message.ToString(), ConsoleColor.Gray);
    }

    public void Warn(object message)
    {
        WriteLine(message.ToString(), ConsoleColor.Yellow);
    }

    public void Error(object message)
    {
        WriteLine(message.ToString(), ConsoleColor.Red);
    }

    private void WriteLine(string message, ConsoleColor color)
    {
        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = originalColor;
    }
}