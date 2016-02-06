using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using Kulv.YCF.MessageQueue;
using Kulv.YCF.Model.DbModel;

namespace Publish
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //DateTime stopDate = DateTime.Now.AddSeconds(300);

                OrderLog orderLog = new OrderLog();
                //orderLog.CreatedById = 1;
                //orderLog.CreatedByName = "Hunk";
                //orderLog.CreatedDate = DateTime.Now;
                //orderLog.LogTypeId = "35";
                //orderLog.Remark = "测试队列";
                //orderLog.OrderId = 1792631;

                string queueConnectionString = "host=192.168.9.24;virtualHost=net;username=netadmin;password=netadmin";

                IAdvancedBus ad = MqClientFactory.CreateRabbitMqClient(queueConnectionString).GetAdvancedBus();
                var exchange = ad.ExchangeDeclare("TestCreateExchange", EasyNetQ.Topology.ExchangeType.Direct, false);

                ad.ExchangeDelete(exchange);
                //MqClientFactory.CreateRabbitMqClient(queueConnectionString).SendAsync<OrderLog>("orderLogQueue", orderLog);

                Thread.Sleep(30);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.Read();
        }
    }

    public interface IAdvanceAPI
    {
        //
        // Summary:
        //     Declare an exchange
        //
        // Parameters:
        //   name:
        //     The exchange name
        //
        //   type:
        //     The type of exchange, The default is ExchangeType.Direct
        //
        //   passive:
        //     Throw an exception rather than create the exchange if it doens't exist. The default is false
        //
        //   durable:
        //     Durable exchanges remain active when a server restarts.
        //
        //   autoDelete:
        //     If set, the exchange is deleted when all queues have finished using it.
        //
        //   internal:
        //     If set, the exchange may not be used directly by publishers, but only when bound
        //     to other exchanges.
        //
        //   alternateExchange:
        //     Route messages to this exchange if they cannot be routed.
        //
        //   delayed:
        //     If set, declars x-delayed-type exchange for routing delayed messages.
        //
        // Returns:
        //     The exchange
        IExchange ExchangeDeclare(string name, string type, bool passive = false, bool durable = true, bool autoDelete = false, bool @internal = false, string alternateExchange = null, bool delayed = false);
        
    }

}
