using System;
using RabbitMQ.Client;
using System.Text;
using System.Threading;

namespace Send
{
    class Program
    {
        static void Main(string[] args)
        {
            IConnection connection = null;
            bool tryAgain = true;
            while(tryAgain == true)
            {
                try
                {
                    connection = CreateConnection();
                    tryAgain = false;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(3000);
                }
            }

            bool continueSending = true;
            while(continueSending == true)
            { 
                string mensagem = $"New message {DateTime.Now}";
                string queueName = "SimpleSendReceive_QUEUE";
				
				// Channel is "Model" in .Net
                using (IModel channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                    byte[] body = Encoding.UTF8.GetBytes(mensagem);
                    channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
                    Console.WriteLine($"Message SENT: {mensagem}");
                }
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
    }
}
