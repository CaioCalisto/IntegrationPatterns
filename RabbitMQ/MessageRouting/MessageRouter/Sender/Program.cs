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
            string exchangeName = "ROUTING_EXCHANGE";
            IConnection connection = CreateConnection();

            bool continueSending = true;
            while(continueSending == true)
            { 
				// Channel is "Model" in .Net
                using (IModel channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct);
                    DateTime now = DateTime.Now;
                    string infoMsg = $"Info Message {now}";
                    string errorMsg = $"Erro Message {now}";

                    byte[] infoBody = Encoding.UTF8.GetBytes(infoMsg);
                    channel.BasicPublish(exchange: exchangeName, routingKey: "info", basicProperties: null, body: infoBody);

                    byte[] erroBody = Encoding.UTF8.GetBytes(errorMsg);
                    channel.BasicPublish(exchange: exchangeName, routingKey: "error", basicProperties: null, body: erroBody);

                    Console.WriteLine($"Messages SENT: {infoMsg} - {errorMsg}");
                }
                Thread.Sleep(10000);
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
