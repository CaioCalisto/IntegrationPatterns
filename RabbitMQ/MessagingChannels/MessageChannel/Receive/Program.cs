using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;

namespace Receive
{
    class Program
    {
        static void Main(string[] args)
        {
            IConnection connection = null;
            string queueName = "SimpleSendReceive_QUEUE";
            bool tryAgain = true;
            while (tryAgain == true)
            {
                try
                {
                    connection = CreateConnection();
                    tryAgain = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(3000);
                }
            }
			
			// Channel is "Model" in .Net
            IModel channel = connection.CreateModel();
            channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
            consumer.Received += ConsumerReceived;
            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            while(true == true)
            {
                Console.WriteLine("I am listening...");
                Thread.Sleep(10000);
            } 
        }

        static IConnection CreateConnection()
        {
            IConnectionFactory factory = new ConnectionFactory()
            {
                HostName = "rabbitmq",
                UserName = "guest",
                Password = "guest",
                Port = 5672
            };
            return factory.CreateConnection();
        }

        private static void ConsumerReceived(object sender, BasicDeliverEventArgs args)
        {
            string mensagem = Encoding.UTF8.GetString(args.Body);
            Console.WriteLine($"Message RECEIVED: {mensagem}");
        }
    }
}
