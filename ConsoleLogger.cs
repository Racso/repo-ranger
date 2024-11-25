namespace Lasso;

public class ConsoleLogger : ILogger
{
    public void Info(object message)
    {
        Console.WriteLine(message);
    }

    public void Error(object message)
    {
        Console.Error.WriteLine(message);
    }
}