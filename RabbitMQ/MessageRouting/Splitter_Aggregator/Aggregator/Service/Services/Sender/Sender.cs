using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Service.Configuration;
using Service.DAO;
using Service.RabbitBus;

namespace Service.Services.Sender
{
    public class Sender : IHostedService
    {
        private Task executingTask;
        private readonly CancellationTokenSource stoppingCts = new CancellationTokenSource();
        private readonly ILogger logger;
        private IConnection connection;
		private IModel processedChannel;
        private IOrderItemsDao orderItemsDao;
        private DelayOptions delayOptions;
        private string processedQueueName;

        public Sender(IRabbitMQBase rabbitMQBase, ILoggerFactory loggerFactory, IOrderItemsDao orderItemsDao, IOptions<DelayOptions> settings)
        {
            try
            {
				this.processedQueueName = "ORDERPROCESSED_QUEUE";
                this.logger = loggerFactory.CreateLogger<Sender>();
                this.connection = rabbitMQBase.CreateConnection();
				this.processedChannel = connection.CreateModel();
				this.processedChannel.QueueDeclare(queue: processedQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                this.orderItemsDao = orderItemsDao;
                this.delayOptions = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            }
            catch(Exception ex)
            {
                this.logger.LogError($"Message: {ex.Message}, Source: {ex.Source}, StackTrace: {ex.StackTrace}");
            }
        }

        protected async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("Aggregator Sender Service is working...");
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    CheckAggregatorMessages();
                    await Task.Delay(delayOptions.CheckAggregatorTime, stoppingToken);
                }
            }
            catch(Exception ex)
            {
                this.logger.LogError($"Message: {ex.Message}, Source: {ex.Source}, StackTrace: {ex.StackTrace}");
            }
            finally
            {
                this.logger.LogInformation("Aggregator Sender Service is stopping.");
                await Task.CompletedTask;
            }
        }

        private void CheckAggregatorMessages()
        {
            IEnumerable<int> orderIds = this.orderItemsDao.GetOrderIds();
            foreach(int orderId in orderIds)
            {
				this.logger.LogInformation($"Checking order id {orderId}");
                IEnumerable<Order> orderItems = this.orderItemsDao.GetOrderItemsByOrderId(orderId);
                if (orderItems.Count() != 0)
                {
                    if (orderItems.First().OrderLength == orderItems.Count())
                    {
                        SendMessage(orderItems);
                        this.orderItemsDao.RemoveByOrderId(orderId);
                    }
                }
            }
        }

        private void SendMessage(IEnumerable<Order> orderItems)
        {
            bool processed = true;
            string message = string.Join(", ", orderItems.Select(i => i.Message));
            if (orderItems.Any(i => i.Processed == false))
                processed = false;
            
            Response order = new Response(orderItems.First().OrderId, processed, message, orderItems.First().Date);
            
            byte[] body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(order));
            this.processedChannel.BasicPublish(exchange: "", routingKey: this.processedQueueName, basicProperties: null, body: body);
            this.logger.LogInformation($"Sent order id {orderItems.First().OrderId}, Data: {orderItems.First().Date}");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            executingTask = Task.Run(() => ExecuteAsync(cancellationToken));

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
