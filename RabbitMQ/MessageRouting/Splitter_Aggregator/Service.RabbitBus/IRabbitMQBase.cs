using RabbitMQ.Client;

namespace Service.RabbitBus
{
    public interface IRabbitMQBase
    {
        IConnection Connection { get; }
    }
}