using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace Splitter
{
    class Program
    {
        static string exchangeName = "PRODUCTS_EXCHANGE";
        static IConnection connection;

        static void Main(string[] args)
        {
            connection = CreateConnection();

            // Start listening orders
            string splitterQueueName = "PRODUCT_SPLITTER_QUEUE";
            IModel aggregatorChannel = connection.CreateModel();
            aggregatorChannel.QueueDeclare(queue: splitterQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            EventingBasicConsumer consumer = new EventingBasicConsumer(aggregatorChannel);
            consumer.Received += ConsumerReceived;
            aggregatorChannel.BasicConsume(queue: splitterQueueName, autoAck: true, consumer: consumer);

            while (true == true)
            {
                Console.WriteLine("Splitter listening...");
                Thread.Sleep(5000);
            }
        }

        private static void ConsumerReceived(object sender, BasicDeliverEventArgs args)
        {
            string message = Encoding.UTF8.GetString(args.Body);
            OrderMessage.Order order = JsonConvert.DeserializeObject<OrderMessage.Order>(message);
            SplitMessageAndSend(order);
        }

        private static void SplitMessageAndSend(OrderMessage.Order order)
        {
            int messageLength = order.OrderItems.Count();            
            int itemSeq = 0;
            foreach(OrderMessage.OrderItem item in order.OrderItems)
            {
                if (item.ItemType == OrderMessage.Type.BOOK)
                {
                    SendBook(new OrderItemSplitted(order.Id, item.Id, item.Quantity, order.Customer.Email, order.Date, itemSeq, messageLength));
                }
                else if (item.ItemType == OrderMessage.Type.ELETRONIC)
                {
                    SendEletronic(new OrderItemSplitted(order.Id, item.Id, item.Quantity, order.Customer.Email, order.Date, itemSeq, messageLength));
                }
                itemSeq++;
            }
        }

        private static void SendBook(OrderItemSplitted orderItem)
        {
            using (IModel channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct);
                byte[] body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(orderItem));
                channel.BasicPublish(exchange: exchangeName, routingKey: "book", basicProperties: null, body: body);
            }
        }

        private static void SendEletronic(OrderItemSplitted orderItem)
        {
            using (IModel channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct);
                byte[] body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(orderItem));
                channel.BasicPublish(exchange: exchangeName, routingKey: "eletronic", basicProperties: null, body: body);
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
