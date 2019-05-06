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
        static string routerExchangeName = "ROUTER_EXCHANGE";
        static string processedQueueName = "PROCESSED_QUEUE";

        static void Main(string[] args)
        {
            connection = CreateConnection();

            // Start listening response
            IModel aggregatorChannel = connection.CreateModel();
            aggregatorChannel.QueueDeclare(queue: processedQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            EventingBasicConsumer consumer = new EventingBasicConsumer(aggregatorChannel);
            consumer.Received += ConsumerReceived;
            aggregatorChannel.BasicConsume(queue: processedQueueName, autoAck: true, consumer: consumer);

            CreateOrderAndSend();
        }

        private static void CreateOrderAndSend()
        {
            bool continueSending = true;
            int orderId = 0;
            while(continueSending == true)
            {
                int customerId = orderId % 2 == 0 ? 1 : 2;
                List<string> routingList = new List<string>();
                double amount = 0;
                if (orderId % 2 == 0)
                {
                    routingList.Add("C");
                    amount = 1000;
                }
                else if (orderId % 3 == 0)
                {
                    routingList.Add("B");
                    routingList.Add("C");
                    amount = 500;
                }
                else
                {
                    routingList.Add("B");
                    amount = 300;
                }

                Messages.RoutingSlip routing = new Messages.RoutingSlip(routingList);
                Messages.Header header = new Messages.Header(routing, true, DateTime.Now);
                Messages.Out.Order order = new Messages.Out.Order(orderId, customerId, amount);
                Messages.Message<Messages.Out.Order> message = new Messages.Message<Messages.Out.Order>(header, order);

                SendMessage(message, "A");
                
                Thread.Sleep(5000);
                orderId++;
            }
        }

        static void SendMessage(Messages.Message<Messages.Out.Order> message, string forward)
        {
            using (IModel channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: routerExchangeName, type: ExchangeType.Direct);
                byte[] body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                channel.BasicPublish(exchange: routerExchangeName, routingKey: forward, basicProperties: null, body: body);
                Console.WriteLine($"Order {message.Body.OrderId} sent: {message.Header.Date}");
            }
        }

        private static void ConsumerReceived(object sender, BasicDeliverEventArgs args)
        {
            string message = Encoding.UTF8.GetString(args.Body);
            Messages.Message<Messages.In.Response> response = JsonConvert.DeserializeObject<Messages.Message<Messages.In.Response>>(message);
            if (response.Header.Success == false)
            {
                Console.WriteLine($"Order {response.Body.OrderId} from customer {response.Body.CustomerId} fails: Message: {response.Body.Message}");
            }
            else
            {
                Console.WriteLine($"Order {response.Body.OrderId} from customer {response.Body.CustomerId} succeeded");
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
