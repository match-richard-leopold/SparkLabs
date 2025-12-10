namespace SparkLabs.Worker.Handlers;

public interface IMessageHandler
{
    Task HandleAsync(string messageJson);
}
