using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;

namespace DelayedDelivery.Producer
{
    class Program
    {
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
                    var exchangeName = "4dev_09_x_delayed_delivery";
                    channel.ExchangeDeclare(exchangeName, "fanout");

                    var queueName = "4dev_09_delayed_messages";

                    var properties = new Dictionary<string, object>();
                    properties.Add("x-dead-letter-exchange", exchangeName);
                    properties.Add("x-message-ttl", 10000);

                    channel.QueueDeclare(queueName, true, false, false, properties);

                    for (var i = 0; i < 10; i++)
                    {
                        var body = Encoding.UTF8.GetBytes(i.ToString());
                        channel.BasicPublish("", queueName, null, body);
                    }

                    Console.WriteLine("Messages published");
                    Console.ReadLine();
                }
            }
        }
    }
}
