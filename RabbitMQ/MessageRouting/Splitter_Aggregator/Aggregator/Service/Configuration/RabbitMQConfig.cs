using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Configuration
{
    public class RabbitMQConfig
    {
        public string RabbitMQConnectionName { get; set; }
        public string RabbitMQHostName { get; set; }
        public string RabbitMQUserName { get; set; }
        public string RabbitMQPassword { get; set; }
        public int RabbitMQPort { get; set; }
    }
}
