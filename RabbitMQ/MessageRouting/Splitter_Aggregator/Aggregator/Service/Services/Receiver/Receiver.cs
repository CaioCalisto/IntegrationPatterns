using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Service.DAO;
using Service.RabbitBus;

namespace Service.Services.Receiver
{
    public class Receiver : IHostedService
    {
        private Task executingTask;
        private readonly CancellationTokenSource stoppingCts = new CancellationTokenSource();
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

        protected async Task ExecuteAsync(CancellationToken stoppingToken)
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

            await Task.CompletedTask;
        }

        private void ConsumerReceived(object sender, BasicDeliverEventArgs args)
        {
            string message = Encoding.UTF8.GetString(args.Body);
            Order item = JsonConvert.DeserializeObject<Order>(message);
            this.logger.LogInformation($"Order id {item.OrderId}, item {item.ItemId}, Seq {item.ItemSeq}, Date: {item.Date}");
            this.orderItemsDao.AddOrderItem(item);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.executingTask = Task.Run(() => ExecuteAsync(cancellationToken));

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (this.executingTask == null)
                return;

            try
            {
                stoppingCts.Cancel();
            }
            finally
            {
                await Task.WhenAll(this.executingTask);
            }
        }
    }
}
