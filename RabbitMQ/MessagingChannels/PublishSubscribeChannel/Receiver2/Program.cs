using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;

namespace Subscribe
{
    class Program
    {
        static void Main(string[] args)
        {
            string exchangeName = "PUBLISHSUBSCRIBE_EXCHANGE";
            IConnection connection = CreateConnection();

            IModel channel = connection.CreateModel();
            channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Fanout);
            string queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "");
            Console.WriteLine("Receiver2 waiting for messages");
            EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
            consumer.Received += ConsumerReceived;
            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            while (true == true)
            {
                Console.WriteLine("Receiver2 listening...");
                Thread.Sleep(1500);
            }
        }

        private static IConnection CreateConnection()
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

        private static void ConsumerReceived(object sender, BasicDeliverEventArgs args)
        {
            string message = Encoding.UTF8.GetString(args.Body);
            Console.WriteLine($"Receiver2: {message}");
        }
    }
}
