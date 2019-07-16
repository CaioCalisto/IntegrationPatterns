using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Service.Configuration;
using System;
using System.Threading;

namespace Service.Services.Base
{
    public class RabbitMQBase : IRabbitMQBase
    {
        private RabbitMQConfig rabbitMQConfig;

        public RabbitMQBase(IOptions<RabbitMQConfig> settings)
        {
            this.rabbitMQConfig = settings.Value;
        }

        public IConnection CreateConnection()
        {
            IConnection connection = null;
            IConnectionFactory factory = new ConnectionFactory()
            {
                HostName = this.rabbitMQConfig.RabbitMQHostName,
                UserName = this.rabbitMQConfig.RabbitMQUserName,
                Password = this.rabbitMQConfig.RabbitMQPassword,
                Port = this.rabbitMQConfig.RabbitMQPort
            };
            bool tryAgain = true;
            while (tryAgain == true)
            {
                try
                {
                    connection = factory.CreateConnection("MyConnection");
                    tryAgain = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(3000);
                }
            }
            return connection;
        }
    }
}
