using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace RPC.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var random = new Random();
            var requests = GenerateRequests(random);
            var responses = new Dictionary<string, string>();

            PrintDictionary(requests, responses);

            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            Console.WriteLine("Press ENTER to start processing");
            Console.ReadLine();

            var connection = factory.CreateConnection();

            var responseQueueName = StartResponseQueue(connection, responses);

            var channel = connection.CreateModel();
                
            foreach (var item in requests)
            {
                var body = Encoding.UTF8.GetBytes(item.Value);
                var properties = channel.CreateBasicProperties();
                properties.CorrelationId = item.Key;
                properties.ReplyTo = responseQueueName;
                channel.BasicPublish("", "bb_06_rpc", properties, body);
            }

            Console.WriteLine("Waiting for responses...");

            while (responses.Count < requests.Count())
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(500));
                Console.Clear();
                PrintDictionary(requests, responses);
            }
            
            Console.WriteLine("Processing finished");
            Console.ReadLine();
        }

        private static string StartResponseQueue(IConnection connection, IDictionary<string, string> responses)
        {
            var channel = connection.CreateModel();

            var queueName = channel.QueueDeclare().QueueName;

            var consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(queueName, true, consumer);

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var ea = consumer.Queue.Dequeue();
                    var body = Encoding.UTF8.GetString(ea.Body);

                    responses.Add(ea.BasicProperties.CorrelationId, body);
                }
            });

            return queueName;
        }

        private static IEnumerable<KeyValuePair<string, string>> GenerateRequests(Random random)
        {
            var requests = new Dictionary<string, string>();

            for (var i = 0; i < 10; i++)
            {
                var d1 = random.Next(10);
                var d2 = random.Next(10);

                requests.Add(Guid.NewGuid().ToString(), string.Format("{0} + {1}", d1, d2));
            }

            return requests;
        }

        private static void PrintDictionary(IEnumerable<KeyValuePair<string, string>> requests, IDictionary<string, string> responses)
        {
            foreach (var item in requests)
            {
                Console.WriteLine("[{0}] {1} = {2}", 
                    item.Key, 
                    item.Value, 
                    responses.ContainsKey(item.Key) ? responses[item.Key] : null);
            }
        }
    }
}
