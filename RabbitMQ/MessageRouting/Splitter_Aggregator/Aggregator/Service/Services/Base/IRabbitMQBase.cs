using RabbitMQ.Client;

namespace Service.Services.Base
{
    public interface IRabbitMQBase
    {
        IConnection CreateConnection();
    }
}