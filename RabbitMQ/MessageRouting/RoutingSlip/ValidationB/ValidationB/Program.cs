using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using ValidationB.Validations;

namespace ValidationB
{
    class Program
    {
        static IConnection connection;
        static string routerExchangeName = "ROUTER_EXCHANGE";
        static string processedQueueName = "PROCESSED_QUEUE";

        static void Main(string[] args)
        {
            connection = CreateConnection();
            IModel receiveChannel = connection.CreateModel();
            receiveChannel.ExchangeDeclare(exchange: routerExchangeName, type: ExchangeType.Direct);
            string queueName = receiveChannel.QueueDeclare().QueueName;
            receiveChannel.QueueBind(queue: queueName, exchange: routerExchangeName, routingKey: "B");
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
            Messages.Message<Messages.Orders.Order> msgObj = JsonConvert.DeserializeObject<Messages.Message<Messages.Orders.Order>>(message);
            Process(msgObj);
        }

        public static void Process(Messages.Message<Messages.Orders.Order> message)
        {
            AValidation validation = new Validation();
            bool success = validation.Process(message.Body);
            if (success == true)
            {
                SendMessageToNextValidation(message);
            }
            else
            {
                Messages.Header header = new Messages.Header()
                {
                    Date = message.Header.Date,
                    Success = false,
                    RoutingSlip = null
                };
                string messageConcatenated = "";
                foreach (var notification in validation.Notifications)
                {
                    messageConcatenated += notification.Message;
                }
                Messages.Response.Response response = new Messages.Response.Response()
                {
                    CustomerId = message.Body.CustomerId,
                    OrderId = message.Body.OrderId,
                    Message = messageConcatenated
                };
                Messages.Message<Messages.Response.Response> responseMessage = new Messages.Message<Messages.Response.Response>(header, response);
                SendResponseMessage(responseMessage);
            }
        }

        static void SendResponseMessage(Messages.Message<Messages.Response.Response> response)
        {
            using (IModel channel = connection.CreateModel())
            {
                byte[] body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                channel.BasicPublish(exchange: "", routingKey: "PROCESSED_QUEUE", basicProperties: null, body: body);
                Console.WriteLine($"Send response. Date: {response.Header.Date}");
            }
        }

        static void SendMessageToNextValidation(Messages.Message<Messages.Orders.Order> message)
        {
            if (message.Header.RoutingSlip.Forward != null)
            {
                message.Header.RoutingSlip.Forward.ForEach(c => Console.WriteLine($"Forward list contains {c}"));
                if (message.Header.RoutingSlip.Forward[0] != null)
                {
                    Console.WriteLine($"Foward to {message.Header.RoutingSlip.Forward[0]}");
                    string forward = message.Header.RoutingSlip.Forward[0];
                    message.Header.RoutingSlip.Forward.RemoveAt(0);
                    using (IModel channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare(exchange: routerExchangeName, type: ExchangeType.Direct);
                        byte[] body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                        channel.BasicPublish(exchange: routerExchangeName, routingKey: forward, basicProperties: null, body: body);
                    }
                    Console.WriteLine($"OrderId: {message.Body.OrderId} send forward to {forward}.");
                }
            }
            else
            {
                Console.WriteLine($"Should send response to Order");
                Messages.Header header = new Messages.Header()
                {
                    Date = message.Header.Date,
                    Success = true,
                    RoutingSlip = null
                };
                Messages.Response.Response response = new Messages.Response.Response()
                {
                    CustomerId = message.Body.CustomerId,
                    OrderId = message.Body.OrderId,
                    Message = ""
                };
                Messages.Message<Messages.Response.Response> responseMessage = new Messages.Message<Messages.Response.Response>(header, response);
                SendResponseMessage(responseMessage);
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
