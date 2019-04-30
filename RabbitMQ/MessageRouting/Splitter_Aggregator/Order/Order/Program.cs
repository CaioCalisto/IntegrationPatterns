using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Order
{
    class Program
    {
        static IConnection connection;

        static void Main(string[] args)
        {
            connection = CreateConnection();
            string splitterQueueName = "PRODUCT_SPLITTER_QUEUE";
            string processedQueueName = "ORDERPROCESSED_QUEUE";

            // Start listening response of aggregator / order processed
            IModel aggregatorChannel = connection.CreateModel();
            aggregatorChannel.QueueDeclare(queue: processedQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            EventingBasicConsumer consumer = new EventingBasicConsumer(aggregatorChannel);
            consumer.Received += ConsumerReceived;
            aggregatorChannel.BasicConsume(queue: processedQueueName, autoAck: true, consumer: consumer);

            // Create and Send orders
            bool continueSending = true;
            int id = 0;
            while (continueSending == true)
            {
                Message.Order order = null;
                if (id % 2 == 0)
                {
                    order = new Message.Order(id, DateTime.Now, new Message.Customer(1, "first@google.com"), new List<Message.OrderItem>()
                    {
                        new Message.OrderItem(1, 5, Message.Type.BOOK),
                        new Message.OrderItem(3, 1, Message.Type.ELETRONIC)
                    });
                }
                else
                {
                    order = new Message.Order(id, DateTime.Now, new Message.Customer(2, "second@msn.com"), new List<Message.OrderItem>()
                    {
                        new Message.OrderItem(2, 1, Message.Type.BOOK),
                        new Message.OrderItem(4, 2, Message.Type.ELETRONIC)
                    });
                }
                using (IModel splitterChannel = connection.CreateModel())
                {
                    splitterChannel.QueueDeclare(queue: splitterQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                    byte[] body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(order));
                    splitterChannel.BasicPublish(exchange: "", routingKey: splitterQueueName, basicProperties: null, body: body);
                    Console.WriteLine($"ORDER SENT, ID {id}");
                }
                Thread.Sleep(3000);

                id++;
            }
        }

        private static void ConsumerReceived(object sender, BasicDeliverEventArgs args)
        {
            string message = Encoding.UTF8.GetString(args.Body);
            Response.Response order = JsonConvert.DeserializeObject<Response.Response>(message);
            Console.WriteLine($"ORDER PROCESSED: {order.OrderId} - {order.Date} - {order.Message}");
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
