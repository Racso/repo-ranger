namespace Lasso;

public interface ILogger
{
    void Info(object message);
    void Error(object message);
}