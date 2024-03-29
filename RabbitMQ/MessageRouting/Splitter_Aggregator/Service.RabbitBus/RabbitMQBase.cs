﻿using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Threading;

namespace Service.RabbitBus
{
    public class RabbitMQBase : IRabbitMQBase
    {
        private readonly RabbitMQConfig rabbitMQConfig;
        public IConnection Connection { get; }

        public RabbitMQBase(IOptions<RabbitMQConfig> settings)
        {
            this.rabbitMQConfig = settings.Value;
            this.Connection = this.CreateConnection();
        }

        private IConnection CreateConnection()
        {
            IConnection connection = null;
            IConnectionFactory factory = new ConnectionFactory()
            {
                HostName = this.rabbitMQConfig.RabbitMQHostName,
                UserName = this.rabbitMQConfig.RabbitMQUserName,
                Password = this.rabbitMQConfig.RabbitMQPassword,
                Port = AmqpTcpEndpoint.UseDefaultPort,
                Protocol = Protocols.DefaultProtocol,
                VirtualHost = "/",
            };
            bool tryAgain = true;
            while (tryAgain == true)
            {
                try
                {
                    connection = factory.CreateConnection("AggregatorConnection");
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
