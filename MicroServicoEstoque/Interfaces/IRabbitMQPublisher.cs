using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroServicoEstoque.Interfaces
{
    public interface IRabbitMQPublisher
    {
        void Publish(string queueName, string message);
    }
}