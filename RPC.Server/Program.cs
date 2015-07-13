using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RPC.Server
{
    class Program
    {
        private static Random random = new Random();

        static void Main(string[] args)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var queueName = "bb_06_rpc";
                    channel.QueueDeclare(queueName, true, false, false, null);;

                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(queueName, true, consumer);

                    Console.WriteLine("Waiting for requests...");

                    while (true)
                    {
                        var ea = consumer.Queue.Dequeue();
                        Task.Factory.StartNew(() => ProcessMessage(ea, connection));

                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);

                        Console.WriteLine("Received: {0}", message);
                    }
                }
            }
        }

        private static void ProcessMessage(BasicDeliverEventArgs ea, IConnection connection)
        {
            var request = Encoding.UTF8.GetString(ea.Body);
            var response = ComputeResponse(request);

            using (var channel = connection.CreateModel())
            {
                var properties = channel.CreateBasicProperties();
                properties.CorrelationId = ea.BasicProperties.CorrelationId;

                var responseBody = Encoding.UTF8.GetBytes(response.ToString());
                
                channel.BasicPublish("", 
                    ea.BasicProperties.ReplyTo,
                    properties,
                    responseBody);
            }
        }

        private static int ComputeResponse(string request)
        {
            Thread.Sleep(TimeSpan.FromSeconds(random.Next(10)));

            Console.WriteLine("Processing: {0}", request);
            var split = request.Split(new[] {'+'}).Select(s => Convert.ToInt32(s.Trim())).ToList();
            return split[0] + split[1];
        }
    }
}
