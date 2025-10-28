using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroServicoEstoque.Infrastructure.RabbitMQ.Interfaces
{
    public interface IRabbitMQPublisher
    {
        void Publish(string queueName, string message);
    }
}