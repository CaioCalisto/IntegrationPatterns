using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading;

namespace Order
{
    class Program
    {
        static IConnection connection;
        static string routerExchangeName = "ROUTER_EXCHANGE";

        static void Main(string[] args)
        {
            connection = CreateConnection();

            
        }

        static void SendMessage(Messages.Message<Messages.Out.Order> message, string forward)
        {
            using (IModel channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: routerExchangeName, type: ExchangeType.Direct);
                byte[] body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                channel.BasicPublish(exchange: routerExchangeName, routingKey: forward, basicProperties: null, body: body);
            }
        }

        static IConnection CreateConnection()
        {
            IConnection connection = null;
            IConnectionFactory factory = new ConnectionFactory()
            {
                HostName = "rabbitmq",
                UserName = "guest",
                Password = "guest",
                Port = 5672
            };
            bool tryAgain = true;
            while (tryAgain == true)
            {
                try
                {
                    connection = factory.CreateConnection();
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
