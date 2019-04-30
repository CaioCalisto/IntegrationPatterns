using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Service.DAO;
using Service.Services.Base;

namespace Service.Services.Receiver
{
    public class Receiver : BackgroundService
    {
        private readonly ILogger logger;
        private IConnection connection;
        private IOrderItemsDao orderItemsDao;
        private string aggregatorQueueName;
       
        public Receiver(IRabbitMQBase rabbitMQBase, ILoggerFactory loggerFactory, IOrderItemsDao orderItemsDao)
        {
            this.connection = rabbitMQBase.CreateConnection();
            this.logger = loggerFactory.CreateLogger<Receiver>();
            this.orderItemsDao = orderItemsDao;
            this.aggregatorQueueName = "AGGREGATOR_QUEUE";
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("Aggregator Receiver Service is listening...");
            IModel aggregatorChannel = connection.CreateModel();
            aggregatorChannel.QueueDeclare(queue: aggregatorQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            EventingBasicConsumer consumer = new EventingBasicConsumer(aggregatorChannel);
            consumer.Received += ConsumerReceived;
            aggregatorChannel.BasicConsume(queue: aggregatorQueueName, autoAck: true, consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
            }
            this.logger.LogInformation("Aggregator Receiver Service is stopping.");
        }

        private void ConsumerReceived(object sender, BasicDeliverEventArgs args)
        {
            string message = Encoding.UTF8.GetString(args.Body);
            Response item = JsonConvert.DeserializeObject<Response>(message);
            this.logger.LogInformation($"Receive order id {item.OrderId}");
            this.orderItemsDao.AddOrderItem(item);
        }
    }
}
