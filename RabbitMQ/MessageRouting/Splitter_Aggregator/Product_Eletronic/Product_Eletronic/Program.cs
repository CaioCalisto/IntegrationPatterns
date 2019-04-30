using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace Product_Eletronic
{
    class Program
    {
        static IConnection connection;
        static void Main(string[] args)
        {
            connection = CreateConnection();
            string exchangeName = "PRODUCTS_EXCHANGE";
            IModel receiveChannel = connection.CreateModel();
            receiveChannel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct);
            string queueName = receiveChannel.QueueDeclare().QueueName;
            receiveChannel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "eletronic");
            Console.WriteLine("Eletronic Service waiting for messages");
            EventingBasicConsumer consumer = new EventingBasicConsumer(receiveChannel);
            consumer.Received += ConsumerReceived;
            receiveChannel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            while (true == true)
            {
                Console.WriteLine("Eletronic Service listening...");
                Thread.Sleep(20000);
            }
        }

        private static void ConsumerReceived(object sender, BasicDeliverEventArgs args)
        {
            string message = Encoding.UTF8.GetString(args.Body);
            OrderItem orderItem = JsonConvert.DeserializeObject<OrderItem>(message);
            Console.WriteLine($"Eletronic Processed: order {orderItem.OrderId}");
            SendResponse(orderItem);
        }

        private static void SendResponse(OrderItem orderItem)
        {
            Thread.Sleep(2000);
            string aggregatorQueueName = "AGGREGATOR_QUEUE";
            Response.Response order = new Response.Response(orderItem.OrderId, orderItem.ItemId, true, "Eletronic Processed", orderItem.Date, orderItem.ItemSeq, orderItem.OrderLength);
            using (IModel aggregatorChannel = connection.CreateModel())
            {
                aggregatorChannel.QueueDeclare(queue: aggregatorQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                byte[] body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(order));
                aggregatorChannel.BasicPublish(exchange: "", routingKey: aggregatorQueueName, basicProperties: null, body: body);
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
