namespace Lasso;

public interface ILogger
{
    void Info(object message);
    void Debug(object message);
    void Warn(object message);
    void Error(object message);
}