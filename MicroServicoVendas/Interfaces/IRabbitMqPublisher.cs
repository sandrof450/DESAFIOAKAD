using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroServicoVendas.Interfaces
{
    public interface IRabbitMqPublisher
    {
        void Publish(string queueName, string message);
    }
}