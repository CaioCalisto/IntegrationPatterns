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
using Service.Models;
using Service.Services.Base;

namespace Service.Services
{
    public class Sender : BackgroundService
    {
        private readonly ILogger logger;
        private IConnection connection;
        private IOrderItemsDao orderItemsDao;
        private DelayOptions delayOptions;
        private string processedQueueName;

        public Sender(IRabbitMQBase rabbitMQBase, ILoggerFactory loggerFactory, IOrderItemsDao orderItemsDao, IOptions<DelayOptions> settings)
        {
            this.connection = rabbitMQBase.CreateConnection();
            this.logger = loggerFactory.CreateLogger<Sender>();
            this.orderItemsDao = orderItemsDao;
            this.delayOptions = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            processedQueueName = "ORDERPROCESSED_QUEUE";
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("Aggregator Sender Service is listening...");
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(delayOptions.CheckAggregatorTime);
                CheckAggregatorMessages();
            }
            this.logger.LogInformation("Aggregator Receiver Service is stopping.");
        }

        private void CheckAggregatorMessages()
        {
            IEnumerable<int> orderIds = this.orderItemsDao.GetOrderIds();
            foreach(int orderId in orderIds)
            {
                IEnumerable<Models.OrderItem> orderItems = this.orderItemsDao.GetOrderItemsByOrderId(orderId);
                if (this.orderItemsDao.GetOrderLength(orderId) == orderItems.Count())
                {
                    SendMessage(orderItems);
                    this.orderItemsDao.RemoveByOrderId(orderId);
                }
            }
        }

        private void SendMessage(IEnumerable<OrderItem> orderItems)
        {
            bool processed = true;
            string message = string.Join(", ", orderItems.Select(i => i.Message));
            if (orderItems.Any(i => i.Processed == false))
                processed = false;
            
            Order order = new Order(orderItems.First().OrderId, processed, message, orderItems.First().Date);
            using (IModel processedChannel = connection.CreateModel())
            {
                processedChannel.QueueDeclare(queue: processedQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                byte[] body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(order));
                processedChannel.BasicPublish(exchange: "", routingKey: processedQueueName, basicProperties: null, body: body);
            }
        }
    }
}
