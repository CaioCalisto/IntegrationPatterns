using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace Validation_A
{
    class Program
    {
        static IConnection connection;
        static string routerExchangeName = "ROUTER_EXCHANGE";

        static void Main(string[] args)
        {
            connection = CreateConnection();
            IModel receiveChannel = connection.CreateModel();
            receiveChannel.ExchangeDeclare(exchange: routerExchangeName, type: ExchangeType.Direct);
            string queueName = receiveChannel.QueueDeclare().QueueName;
            receiveChannel.QueueBind(queue: queueName, exchange: routerExchangeName, routingKey: "A");
            Console.WriteLine("Book Service waiting for messages");
            EventingBasicConsumer consumer = new EventingBasicConsumer(receiveChannel);
            consumer.Received += ConsumerReceived;
            receiveChannel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            while (true == true)
            {
                Console.WriteLine("Book Service listening...");
                Thread.Sleep(20000);
            }
        }

        private static void ConsumerReceived(object sender, BasicDeliverEventArgs args)
        {
            string message = Encoding.UTF8.GetString(args.Body);
            Messages.Message<Messages.Orders.Order> response = JsonConvert.DeserializeObject<Messages.Message<Messages.Orders.Order>>(message);

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
